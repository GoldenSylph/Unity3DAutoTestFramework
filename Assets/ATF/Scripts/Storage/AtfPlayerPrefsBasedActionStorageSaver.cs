using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ATF.Scripts.Storage.Interfaces;
using Bedrin.Helper;
using UnityEditor.IMGUI.Controls;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Serialization;

namespace ATF.Scripts.Storage
{
    public class AtfPlayerPrefsBasedActionStorageSaver : MonoSingleton<AtfPlayerPrefsBasedActionStorageSaver>, IAtfActionStorageSaver
    {

        #region Classes for serialization in JSON
        [Serializable]
        private class FipAndActions
        {
            public string fakeInputParameter;
            public List<AtfAction> actions;

        }

        [Serializable]
        private class FakeInputWithFipAndActions
        {
            public FakeInput fakeInput;
            public List<FipAndActions> fipsAndActions;

            public FipAndActions FindFipAndActionsByFip(object fip)
            {
                return fipsAndActions.Find((e) => e.fakeInputParameter.Equals(fip));
            }

            public IEnumerable<object> GetAllFips()
            {
                return fipsAndActions?.ConvertAll((e) => e.fakeInputParameter);
            }

        }

        [Serializable]
        private class Record
        {
            public string recordName;
            public List<FakeInputWithFipAndActions> fakeInputsWithFipsAndActions;

            public FakeInputWithFipAndActions FindFakeInputWithFipAndActionsByKind(FakeInput kind)
            {
                return fakeInputsWithFipsAndActions?.Find((element) => element.fakeInput.Equals(kind));
            }

            public IEnumerable<FakeInput> GetAllFakeInputs()
            {
                return fakeInputsWithFipsAndActions?.ConvertAll((e) => e.fakeInput);
            }

            public override int GetHashCode()
            {
                // ReSharper disable NonReadonlyMemberInGetHashCode
                return recordName.GetHashCode();
            }

#pragma warning disable 659
            public override bool Equals(object obj)
#pragma warning restore 659
            {
                return obj is Record record && record.recordName.Equals(recordName);
            }
        }
        
        [Serializable]
        private class Slot
        {
            public List<Record> content;

            public Record FindRecordByName(string name)
            {
                return content?.Find((r) => r.recordName.Equals(name));
            }

            public IEnumerable<string> GetAllRecordNames()
            {
                return content?.ConvertAll((record) => record.recordName);
            }

            public static Slot Merge(Slot a, Slot b)
            {
                return new Slot {content = a.content.Union(b.content).ToList()};
            }
        }
        #endregion

        [Header("Debug Settings")]
        
        [SerializeField]
        private string currentRecordName;
        
        [SerializeField]
        private string slotKey;
        
        private Slot _slotToSerialize;
        private bool _isDebugOn;

        public void Initialize()
        {
            slotKey = "SAVED_ACTION_STORAGE";
            _isDebugOn = FindObjectOfType<AtfInitializer>().isDebugPrintOn;
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
                _slotToSerialize.content = temp.content.Where((r) => r.recordName.Equals(GetCurrentRecordName())).ToList();
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
                (temp) =>
                {
                    if (temp.content == null) return;
                    temp.content = temp.content.Where((r) => !r.recordName.Equals(GetCurrentRecordName())).ToList();
                    print(temp.content.Count);
                    PlayerPrefs.SetString(slotKey, JsonUtility.ToJson(temp));
                });
        }

        public void SetActions(IEnumerable actionEnumerable)
        {
            InitSlotIfNeeded();
            var newContent =
                (Dictionary<string, Dictionary<FakeInput, Dictionary<object, Queue<AtfAction>>>>) actionEnumerable;
            if (newContent.Count == 0) return;
            _slotToSerialize.content = Pack(newContent);
        }

        public IEnumerable GetActions()
        {
            return Unpack(_slotToSerialize);
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
                        foreach (var treeViewItemOfAction in fipAndActions.actions.Select(action => action.GetDeserialized()).Select(deserialized => new TreeViewItem
                        {
                            id = DictionaryBasedIdGenerator.GetNewId(deserialized.Content.ToString()),
                            depth = 2,
                            displayName = deserialized.Content.ToString()
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

        private static List<Record> Pack(Dictionary<string, Dictionary<FakeInput, Dictionary<object, Queue<AtfAction>>>> input) 
        {
            var result = new List<Record>();
            foreach (var recordName in input.Keys)
            {
                var newRecord = new Record
                {
                    recordName = recordName,
                    fakeInputsWithFipsAndActions = new List<FakeInputWithFipAndActions>()
                };
                foreach (var fakeInput in input[recordName].Keys)
                {
                    var newFakeInputWithFipAndActions = new FakeInputWithFipAndActions
                    {
                        fakeInput = fakeInput,
                        fipsAndActions = new List<FipAndActions>()
                    };
                    foreach (var fip in input[recordName][fakeInput].Keys)
                    {
                        newFakeInputWithFipAndActions.fipsAndActions.Add(new FipAndActions
                        {
                            fakeInputParameter = fip.ToString(),
                            actions = new List<AtfAction>(input[recordName][fakeInput][fip])
                        });
                    }
                    newRecord.fakeInputsWithFipsAndActions.Add(newFakeInputWithFipAndActions);
                }
                result.Add(newRecord);
            }
            return result;
        }

        private static Dictionary<string, Dictionary<FakeInput, Dictionary<object, Queue<AtfAction>>>> Unpack(Slot slot)
        {
            if (slot == null) return null;
            var result = new Dictionary<string, Dictionary<FakeInput, Dictionary<object, Queue<AtfAction>>>>();
            foreach (var record in slot.content)
            {
                foreach (var fakeInputWithFipAndActions in record.fakeInputsWithFipsAndActions)
                {
                    foreach (var fipAndActions in fakeInputWithFipAndActions.fipsAndActions)
                    {
                        result.Add(record.recordName, new Dictionary<FakeInput, Dictionary<object, Queue<AtfAction>>>());
                        result[record.recordName].Add(fakeInputWithFipAndActions.fakeInput, new Dictionary<object, Queue<AtfAction>>());
                        result[record.recordName][fakeInputWithFipAndActions.fakeInput].Add(ParseFip(fipAndActions.fakeInputParameter), 
                            new Queue<AtfAction>(fipAndActions.actions.ConvertAll((e) => e.GetDeserialized())));
                    }
                }
            }
            return result;
        }

        private static object ParseFip(string fip)
        {
            if (bool.TryParse(fip, out var boolVariant))
            {
                return boolVariant;
            }
            if (float.TryParse(fip, out var floatVariant))
            {
                return floatVariant;
            }
            if (int.TryParse(fip, out var intVariant))
            {
                return intVariant;
            }
            if (Enum.TryParse<KeyCode>(fip, out var keyCodeVariant))
            {
                return keyCodeVariant;
            }
            return fip;
        }
    }
}
