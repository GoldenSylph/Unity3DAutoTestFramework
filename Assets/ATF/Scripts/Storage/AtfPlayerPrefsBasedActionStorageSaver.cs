using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ATF.Scripts.Storage.Interfaces;
using Bedrin.Helper;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Serialization;
using Bedrin.Helper;

namespace ATF.Scripts.Storage
{
    public class AtfPlayerPrefsBasedActionStorageSaver : MonoSingleton<AtfPlayerPrefsBasedActionStorageSaver>, IAtfActionStorageSaver
    {

        [Serializable]
        private class Slot
        {
            public Dictionary<string, Dictionary<FakeInput, Dictionary<object, Queue<AtfAction>>>> content;
        }

        [Header("Debug Settings")]
        
        [SerializeField]
        private string currentRecordName;
        
        [SerializeField]
        private string slotKey;
        
        private Slot SlotToSerialize;

        public void Initialize()
        {
            slotKey = "SAVED_ACTION_STORAGE";
        }

        public void LoadRecord()
        {
            InitSlotIfNeeded();
            IfHasSlotKey((temp) =>
            {
                SlotToSerialize.content = temp.content;
            });
        }

        public void SaveRecord()
        {
            IfHasSlotKey(
                (temp) =>
                {
                    if (temp.content == null)
                    {
                        temp.content =
                            new Dictionary<string, Dictionary<FakeInput, Dictionary<object, Queue<AtfAction>>>>();
                    }
                    //TODO
                    temp.content[GetCurrentRecordName()] = SlotToSerialize.content[GetCurrentRecordName()];
                    PlayerPrefs.SetString(slotKey, JsonUtility.ToJson(temp));    
                },
                () =>
                {
                    PlayerPrefs.SetString(slotKey, JsonUtility.ToJson(SlotToSerialize));    
                });
        }
        
        public void ScrapRecord()
        {
            IfHasSlotKey(
                (temp) =>
                {
                    if (temp.content == null) return;
                    temp.content.Remove(GetCurrentRecordName());
                    PlayerPrefs.SetString(slotKey, JsonUtility.ToJson(temp));
                });
        }

        public void SetActions(IEnumerable actionEnumerable)
        {
            InitSlotIfNeeded();
            SlotToSerialize.content = (Dictionary<string, Dictionary<FakeInput, Dictionary<object, Queue<AtfAction>>>>) actionEnumerable;
        }

        public List<TreeViewItem> GetSavedNames()
        {
            InitSlotIfNeeded();
            var result = new List<TreeViewItem>();
            IfHasSlotKey(
                (temp) =>
                {
                    if (temp?.content == null) return;
                    foreach (var key in temp.content.Keys)
                    {
                        result.Add(new TreeViewItem
                        {
                            id = DictionaryBasedIdGenerator.GetNewId(key.ToString()),
                            depth = 0,
                            displayName = key.ToString()
                        });
                    }
                });
            return result;
        }

        public List<TreeViewItem> GetSavedRecordDetails(string recordName)
        {
            var result = new List<TreeViewItem>();
            IfHasSlotKey((temp) =>
            {
                if (temp?.content == null) return;
                foreach (var inputKind in temp.content[recordName].Keys)
                {
                    var treeViewItemOfInputKind = new TreeViewItem
                    {
                        id = DictionaryBasedIdGenerator.GetNewId(inputKind.ToString()),
                        depth = 0,
                        displayName = inputKind.ToString()
                    };
                    foreach (var fakeInputParameter in temp.content[recordName][inputKind].Keys)
                    {
                        var treeViewItemOfFip = new TreeViewItem
                        {
                            id = DictionaryBasedIdGenerator.GetNewId(fakeInputParameter.ToString()),
                            depth = 1,
                            displayName = fakeInputParameter.ToString()
                        };
                        foreach (var action in temp.content[recordName][inputKind][fakeInputParameter])
                        {
                            var treeViewItemOfAction = new TreeViewItem
                            {
                                id = DictionaryBasedIdGenerator.GetNewId(action.Content.ToString()),
                                depth = 2,
                                displayName = action.Content.ToString()
                            };
                            treeViewItemOfFip.AddChild(treeViewItemOfAction);
                        }
                        treeViewItemOfInputKind.AddChild(treeViewItemOfFip);
                    }
                    result.Add(treeViewItemOfInputKind);
                }
            });
            return result;
        }

        public IEnumerable GetActions()
        {
            return SlotToSerialize?.content;
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
            if (SlotToSerialize == null)
            {
                SlotToSerialize = new Slot();
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
