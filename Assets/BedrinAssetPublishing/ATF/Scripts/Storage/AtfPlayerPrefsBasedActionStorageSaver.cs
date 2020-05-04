using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using ATF.Scripts.DI;
using ATF.Scripts.Helper;
using ATF.Scripts.Storage.Interfaces;
using ATF.Scripts.Storage.Utils;
using UnityEditor.IMGUI.Controls;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace ATF.Scripts.Storage
{
    [Injectable]
    [AtfSystem]
    public class AtfPlayerPrefsBasedActionStorageSaver : MonoSingleton<AtfPlayerPrefsBasedActionStorageSaver>, IAtfActionStorageSaver
    {
        [Inject(typeof(AtfGreedyPacker))]
        public IAtfPacker packer;
        
        [Header("Debug Settings")]
        [SerializeField]
        private string currentRecordName;
        
        [SerializeField]
        private string slotKey;
        
        private Slot _slotToSerialize;
        private bool _isDebugOn;

        public override void Initialize()
        {
            slotKey = "SAVED_ACTION_STORAGE";
            _isDebugOn = FindObjectOfType<AtfInitializer>().isDebugPrintOn;
            base.Initialize();
        }

        private void Print(object o)
        {
            if (_isDebugOn)
            {
                print(o);
            }
        }

        public void LoadRecord()
        {
            InitSlotIfNeeded();
            IfHasSlotKey((temp) =>
            {
                if (temp.content == null) return;
                _slotToSerialize.content = temp.content.Where(r => r.recordName.Equals(GetCurrentRecordName())).ToList();
            });
        }

        public void SaveRecord()
        {
            IfHasSlotKey(
                (temp) =>
                {
                    if (_slotToSerialize?.content == null) return;
                    Print("Has key, updating...");
                    _slotToSerialize.content = _slotToSerialize.content
                        .Where((r) => r.recordName.Equals(GetCurrentRecordName())).ToList();
                    var serializedSlot = JsonUtility.ToJson(Slot.Merge(_slotToSerialize, temp));
                    Print(serializedSlot);
                    PlayerPrefs.SetString(slotKey, serializedSlot);
                },
                () =>
                {
                    if (_slotToSerialize.content == null) return;
                    Print("Hasn't key, creating...");
                    _slotToSerialize.content = _slotToSerialize.content
                        .Where((r) => r.recordName.Equals(GetCurrentRecordName())).ToList();
                    var serializedSlot = JsonUtility.ToJson(_slotToSerialize);
                    Print(serializedSlot);
                    PlayerPrefs.SetString(slotKey, serializedSlot);
                });
        }
        
        public void ScrapRecord()
        {
            IfHasSlotKey(
                temp =>
                {
                    if (temp.content == null) return;
                    temp.content = temp.content.Where(r => !r.recordName.Equals(GetCurrentRecordName())).ToList();
                    print($"Records saved: {temp.content.Count}");
                    PlayerPrefs.SetString(slotKey, JsonUtility.ToJson(temp));
                });
        }

        public void SetActions(IEnumerable actionEnumerable)
        {
            InitSlotIfNeeded();
            var newContent =
                (Dictionary<string, Dictionary<FakeInput, Dictionary<object, AtfActionRleQueue>>>) actionEnumerable;
            if (newContent.Count == 0) return;
            _slotToSerialize.content = packer.Pack(newContent);
        }

        public IEnumerable GetActions()
        {
            return packer.Unpack(_slotToSerialize);
        }
        
        public List<TreeViewItem> GetSavedNames()
        {
            var result = new List<TreeViewItem>();
            IfHasSlotKey(
                (temp) =>
                {
                    if (temp.content == null || temp.content.Count == 0)
                    {
                        // ReSharper disable once InconsistentNaming
                        const string NO_RECORDS_SAVED = "No records saved.";
                        result.Add(new TreeViewItem {
                            id = DictionaryBasedIdGenerator.GetNewId(NO_RECORDS_SAVED),
                            depth = 0,
                            displayName = NO_RECORDS_SAVED
                        });
                    }
                    else
                    {
                        result.AddRange(
                            temp.GetAllRecordNames()
                                .Select(key => new TreeViewItem
                                {
                                    id = DictionaryBasedIdGenerator.GetNewId(key.ToString()), 
                                    depth = 0, 
                                    displayName = key.ToString()
                                }));
                    }
                });
            return result;
        }

        public List<TreeViewItem> GetSavedRecordDetails(string recordName)
        {
            var result = new List<TreeViewItem>();
            IfHasSlotKey((slotFromPlayerPrefs) =>
            {
                if (slotFromPlayerPrefs?.content == null) return;
                
                var record = slotFromPlayerPrefs.FindRecordByName(recordName);
                foreach (var inputKind in record.GetAllFakeInputs())
                {
                    var treeViewItemOfInputKind = new TreeViewItem
                    {
                        id = DictionaryBasedIdGenerator.GetNewId(inputKind.ToString()),
                        depth = 0,
                        displayName = inputKind.ToString()
                    };
                    var fakeInputWithFipAndActions = record.FindFakeInputWithFipAndActionsByKind(inputKind);
                    foreach (var fakeInputParameter in fakeInputWithFipAndActions.GetAllFips())
                    {
                        var treeViewItemOfFip = new TreeViewItem
                        {
                            id = DictionaryBasedIdGenerator.GetNewId(fakeInputParameter.ToString()),
                            depth = 1,
                            displayName = fakeInputParameter.ToString()
                        };
                        var fipAndActions = fakeInputWithFipAndActions.FindFipAndActionsByFip(fakeInputParameter);
                        foreach (var treeViewItemOfAction in fipAndActions.metadata
                            .Select((metadata, index) =>
                            {
                                metadata.action.GetDeserialized();
                                var treeViewItemResult = new TreeViewItem
                                {
                                    id = DictionaryBasedIdGenerator.GetNewId(metadata.action.Content.ToString()),
                                    depth = 2,
                                    displayName =
                                        $"{metadata.action.Content} - {metadata.repetitions} repetitions"
                                };
                                return treeViewItemResult;
                            }))
                        {
                            treeViewItemOfFip.AddChild(treeViewItemOfAction);
                        }
                        treeViewItemOfInputKind.AddChild(treeViewItemOfFip);
                    }
                    result.Add(treeViewItemOfInputKind);
                }
            });
            return result;
        }

        public void ExportFile(string fullPath)
        {
            if (PlayerPrefs.HasKey(slotKey))
            {
                var serializedSlot = PlayerPrefs.GetString(slotKey);
                using (var writer = new StreamWriter(fullPath))  
                {  
                    writer.Write(serializedSlot);  
                }
                print($"Export is complete: {fullPath}");
            }
            else
            {
                Debug.LogError("There is no saved storage.");
            }
        }

        public void ImportFile(string fullPath)
        {
            using (var reader = new StreamReader(fullPath))
            {
                var serializedSlot = reader.ReadToEnd();
                PlayerPrefs.SetString(slotKey, serializedSlot);
            }
        }

        public void SetCurrentRecordName(string recordName)
        {
            currentRecordName = recordName;
        }

        public string GetCurrentRecordName()
        {
            return currentRecordName;
        }

        private void InitSlotIfNeeded()
        {
            if (_slotToSerialize == null)
            {
                _slotToSerialize = new Slot();
            }
        }
        
        private void IfHasSlotKey(Action<Slot> thenDo, Action elseDo = null)
        {
            if (PlayerPrefs.HasKey(slotKey))
            {
                thenDo(JsonUtility.FromJson<Slot>(PlayerPrefs.GetString(slotKey)));
            }
            else
            {
                elseDo?.Invoke();
            }
        }
        
    }
}
