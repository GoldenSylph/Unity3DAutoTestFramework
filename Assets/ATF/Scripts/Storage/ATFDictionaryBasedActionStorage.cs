using System;
using System.Collections.Generic;
using System.Linq;
using ATF.Scripts.Recorder;
using ATF.Scripts.Storage.Interfaces;
using Bedrin.DI;
using Bedrin.Helper;
using UnityEditor.IMGUI.Controls;
using UnityEngine.Serialization;

namespace ATF.Scripts.Storage
{
    public static class ATFIdHelper
    {
        private static int _idCounter;
        private static Dictionary<string, int> _ids;        
        
        public static int GetNewId(string displayName)
        {
            if (_ids == null)
            {
                _ids = new Dictionary<string, int>();
            }

            if (displayName == null)
            {
                return ++_idCounter;
            }
            
            if (_ids.ContainsKey(displayName))
            {
                return _ids[displayName];
            }

            _ids[displayName] = ++_idCounter;
            return _ids[displayName];
        }
    }
    
    [Injectable]
    public class ATFDictionaryBasedActionStorage : MonoSingleton<ATFDictionaryBasedActionStorage>, IATFActionStorage
    {

        [Inject(typeof(ATFQueueBasedRecorder))]
        public IATFRecorder recorder;

        [Inject(typeof(ATFPlayerPrefsBasedActionStorageSaver))]
        public IATFActionStorageSaver saver;

        private Dictionary<string, Dictionary<FakeInput, Queue<Action>>> ActionStorage;
        private Dictionary<FakeInput, Queue<Action>> PlayStorage;

        public object GetPartOfRecord(FakeInput kind)
        {
            if (kind == FakeInput.NONE || PlayStorage == null || !PlayStorage.ContainsKey(kind)) return null;
            try
            {
                if (DependencyInjector.DEBUG_ON)
                {
                    print($"Action to deliver remain: {PlayStorage[kind].Count}");
                }
                if (recorder.IsPlaying() && !recorder.IsPlayPaused())
                {
                    return PlayStorage[kind].Dequeue().content;
                }

                return PlayStorage[kind].Peek().content;
            } catch (Exception)
            {
                if (DependencyInjector.DEBUG_ON) print("Clearing play cache");
                recorder.StopPlay();
                ClearPlayStorage();
            }
            return null;
        }

        public void Enqueue(string recordName, FakeInput kind, Action action)
        {
            BeforeIfNameAndKindAreNotExistInStorage(recordName, kind, 
                (r, k) => {
                    ActionStorage[r][k].Enqueue(action);
                },
                (r, k) => {
                    ActionStorage.Add(recordName, new Dictionary<FakeInput, Queue<Action>>());
                    ActionStorage[recordName].Add(kind, new Queue<Action>());
                });
        }

        public Action Peek(string recordName, FakeInput kind)
        {
            return IfNameAndKindIsNotExistInStorageReturnDefault(recordName, kind,
                () => ActionStorage[recordName][kind].Peek(), null);
        }

        public Action Dequeue(string recordName, FakeInput kind)
        {
            return IfNameAndKindIsNotExistInStorageReturnDefault(recordName, kind,
                () => ActionStorage[recordName][kind].Dequeue(), null);
        }

        public void Initialize()
        {
            ActionStorage = new Dictionary<string, Dictionary<FakeInput, Queue<Action>>>();
        }

        public bool PrepareToPlayRecord(string recordName)
        {
            if (!ActionStorage.ContainsKey(recordName)) return false;
            PlayStorage = ReturnNewCopyOf(ActionStorage[recordName]);
            return true;
        }

        public void ClearPlayStorage()
        {
            PlayStorage = null;
        }

        public void LoadStorage(string recordName)
        {
            saver.SetRecordName(recordName);
            var loadedData = saver.GetActions();
            switch (loadedData)
            {
                case Dictionary<string, Dictionary<FakeInput, Queue<Action>>> data:
                    ActionStorage = ReturnNewCopyOf(data);
                    break;
                case Dictionary<FakeInput, Queue<Action>> data:
                    ActionStorage[recordName] = ReturnNewCopyOf(data);
                    break;
            }
        }

        public void SaveStorage(string recordName)
        {
            saver.SetRecordName(recordName);
            saver.SetActions(ActionStorage);
        }

        public void ScrapSavedStorage(string recordName)
        {
            saver.SetRecordName(recordName);
            saver.ScrapSavedActions();
        }

        public List<TreeViewItem> GetSavedRecordNames()
        {
            return null;
        }

        public List<TreeViewItem> GetCurrentRecordNames()
        {
            var result = new List<TreeViewItem>();
            foreach (var key in ActionStorage.Keys)
            {
                result.Add(new TreeViewItem
                {
                    id = ATFIdHelper.GetNewId(key),
                    depth = 0,
                    displayName = key
                });
            }
            return result.Count == 0 ? null : result;
        }

        public List<TreeViewItem> GetCurrentActions(string recordName)
        {
            var result = new List<TreeViewItem>();
            if (!ActionStorage.ContainsKey(recordName)) return null;
            result.Add(new TreeViewItem
            {
                id = -2,
                depth = 0,
                displayName = recordName
            });
            foreach (var pair in ActionStorage[recordName])
            {
                var rootFakeInput = new TreeViewItem
                {
                    id = ATFIdHelper.GetNewId(pair.Key.ToString()),
                    depth = 1,
                    displayName = pair.Key.ToString()
                };
                var childrenActions = new List<TreeViewItem>();
                foreach (var action in pair.Value)
                {
                    var actionTreeViewItem = new TreeViewItem
                    {
                        id = ATFIdHelper.GetNewId(action.content.ToString()),
                        depth = 2,
                        displayName = action.content.ToString()
                    };
                    rootFakeInput.AddChild(actionTreeViewItem);
                    childrenActions.Add(actionTreeViewItem);
                }
                result.Add(rootFakeInput);
                result.AddRange(childrenActions);
            }
            return result;
        }

        public List<TreeViewItem> GetSavedActions(string recordName)
        {
            return null;
        }

        public static Dictionary<FakeInput, Queue<Action>> ReturnNewCopyOf(Dictionary<FakeInput, Queue<Action>> etalon)
        {
            var result = new Dictionary<FakeInput, Queue<Action>>();
            if (etalon == null) return result;
            foreach (var fi in etalon.Keys)
            {
                foreach (var q in etalon.Values)
                {
                    result[fi] = new Queue<Action>(q);
                }
            }
            return result;
        }

        public static Dictionary<string, Dictionary<FakeInput, Queue<Action>>> ReturnNewCopyOf(Dictionary<string,
            Dictionary<FakeInput, Queue<Action>>> etalon)
        {
            var result =
                new Dictionary<string, Dictionary<FakeInput, Queue<Action>>>();
            foreach (var recordName in etalon.Keys)
            {
                result[recordName] = ReturnNewCopyOf(etalon[recordName]);
            }
            return result;
        }

        private void BeforeIfNameAndKindAreNotExistInStorage(string recordName, FakeInput kind,
            Action<string, FakeInput> then, Action<string, FakeInput> beforeAction)
        {
            if (!ActionStorage.ContainsKey(recordName) || !ActionStorage[recordName].ContainsKey(kind))
            {
                beforeAction(recordName, kind);

            }
            then(recordName, kind);
        }

        private T IfNameAndKindIsNotExistInStorageReturnDefault<T>(string recordName, FakeInput kind, Func<T> toReturn, T defaultValue)
        {
            if (!ActionStorage.ContainsKey(recordName) || !ActionStorage[recordName].ContainsKey(kind))
            {
                return defaultValue;
            }

            return toReturn();
        }

        
    }
}