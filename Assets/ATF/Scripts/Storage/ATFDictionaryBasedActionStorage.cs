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

        private Dictionary<string, Dictionary<FakeInput, Dictionary<object, Queue<Action>>>> ActionStorage;
        private Dictionary<FakeInput, Dictionary<object, Queue<Action>>> PlayStorage;

        public object GetPartOfRecord(FakeInput kind, object fakeInputParameter)
        {
            if (kind == FakeInput.NONE || PlayStorage == null || !PlayStorage.ContainsKey(kind) 
                || !PlayStorage[kind].ContainsKey(fakeInputParameter)) return null;
            try
            {
                if (DependencyInjector.DebugOn)
                {
                    print($"Action to deliver remain: {PlayStorage[kind].Count}");
                }
                if (recorder.IsPlaying() && !recorder.IsPlayPaused())
                {
                    return PlayStorage[kind][fakeInputParameter].Dequeue().content;
                }
                return PlayStorage[kind][fakeInputParameter].Peek().content;
            } catch (Exception)
            {
                if (DependencyInjector.DebugOn) print("Clearing play cache");
                recorder.StopPlay();
                ClearPlayStorage();
            }
            return null;
        }

        public void Enqueue(string recordName, FakeInput kind, object fakeInputParameter, Action action)
        {
            BeforeIfNameAndKindAndFIPAreNotExistInStorage(recordName, kind, fakeInputParameter,
                (r, k, fip) => {
                    ActionStorage[r][k][fip].Enqueue(action);
                },
                (r, k, fip) => {
                    ActionStorage.Add(r, new Dictionary<FakeInput, Dictionary<object, Queue<Action>>>());
                    ActionStorage[r].Add(k, new Dictionary<object, Queue<Action>>());
                    ActionStorage[r][k][fip] = new Queue<Action>();
                });
        }

        public Action Peek(string recordName, FakeInput kind, object fakeInputParameter)
        {
            return IfNameAndKindAndFIPIsNotExistInStorageReturnDefault(recordName, kind, fakeInputParameter,
                () => ActionStorage[recordName][kind][fakeInputParameter].Peek(), null);
        }

        public Action Dequeue(string recordName, FakeInput kind, object fakeInputParameter)
        {
            return IfNameAndKindAndFIPIsNotExistInStorageReturnDefault(recordName, kind, fakeInputParameter,
                () => ActionStorage[recordName][kind][fakeInputParameter].Dequeue(), null);
        }

        public void Initialize()
        {
            ActionStorage = new Dictionary<string, Dictionary<FakeInput, Dictionary<object, Queue<Action>>>>();
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
                case Dictionary<string, Dictionary<FakeInput, Dictionary<object, Queue<Action>>>> data:
                    ActionStorage = ReturnNewCopyOf(data);
                    break;
                case Dictionary<FakeInput, Dictionary<object, Queue<Action>>> data:
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
            foreach (var fakeInputAndDictionary in ActionStorage[recordName])
            {
                var rootFakeInput = new TreeViewItem
                {
                    id = ATFIdHelper.GetNewId(fakeInputAndDictionary.Key.ToString()),
                    depth = 0,
                    displayName = fakeInputAndDictionary.Key.ToString()
                };
                var childrenActions = new List<TreeViewItem>();
                foreach (var fakeInputParameterAndQueue in fakeInputAndDictionary.Value)
                {
                    var displayNameForFip = fakeInputParameterAndQueue.Key.ToString();
                    var fakeInputParameterViewItem = new TreeViewItem
                    {
                        id = ATFIdHelper.GetNewId(displayNameForFip),
                        depth = 1,
                        displayName = displayNameForFip
                    };
                    foreach (var action in fakeInputParameterAndQueue.Value)
                    {
                        var actionTreeViewItem = new TreeViewItem
                        {
                            id = ATFIdHelper.GetNewId(action.content.ToString()),
                            depth = 2,
                            displayName = action.content.ToString()
                        };
                    }
//                    rootFakeInput.AddChild(actionTreeViewItem);
//                    childrenActions.Add(actionTreeViewItem);
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

        public static Dictionary<FakeInput, Dictionary<object, Queue<Action>>> ReturnNewCopyOf(Dictionary<FakeInput, Dictionary<object, Queue<Action>>> etalon)
        {
            var result = new Dictionary<FakeInput, Dictionary<object, Queue<Action>>>();
            if (etalon == null) return result;
            foreach (var fi in etalon.Keys)
            {
                foreach (var objectToQueue in etalon.Values)
                {
                    result[fi] = new Dictionary<object, Queue<Action>>();
                    foreach (var pair in objectToQueue)
                    {
                        result[fi][pair.Key] = new Queue<Action>(pair.Value);
                    }
                }
            }
            return result;
        }

        public static Dictionary<string, Dictionary<FakeInput, Dictionary<object, Queue<Action>>>> ReturnNewCopyOf(Dictionary<string, 
            Dictionary<FakeInput, Dictionary<object, Queue<Action>>>> etalon)
        {
            var result =
                new Dictionary<string, Dictionary<FakeInput, Dictionary<object, Queue<Action>>>>();
            foreach (var recordName in etalon.Keys)
            {
                result[recordName] = ReturnNewCopyOf(etalon[recordName]);
            }
            return result;
        }

        private void BeforeIfNameAndKindAndFIPAreNotExistInStorage(string recordName, FakeInput kind, object fakeInputParameter,
            Action<string, FakeInput, object> then, Action<string, FakeInput, object> beforeAction)
        {
            if (!ActionStorage.ContainsKey(recordName) || !ActionStorage[recordName].ContainsKey(kind) || !ActionStorage[recordName][kind].ContainsKey(fakeInputParameter))
            {
                beforeAction(recordName, kind, fakeInputParameter);

            }
            then(recordName, kind, fakeInputParameter);
        }

        private T IfNameAndKindAndFIPIsNotExistInStorageReturnDefault<T>(string recordName, FakeInput kind, object fakeInputParameter, Func<T> toReturn, T defaultValue)
        {
            if (!ActionStorage.ContainsKey(recordName) || !ActionStorage[recordName].ContainsKey(kind) || !ActionStorage[recordName][kind].ContainsKey(fakeInputParameter))
            {
                return defaultValue;
            }

            return toReturn();
        }

        
    }
}