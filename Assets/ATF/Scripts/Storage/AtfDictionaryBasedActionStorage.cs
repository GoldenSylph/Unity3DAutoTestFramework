using System;
using System.Collections;
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
    [Injectable]
    public class AtfDictionaryBasedActionStorage : MonoSingleton<AtfDictionaryBasedActionStorage>, IAtfActionStorage
    {

        [Inject(typeof(AtfQueueBasedRecorder))]
        public IAtfRecorder recorder;

        [Inject(typeof(AtfPlayerPrefsBasedActionStorageSaver))]
        public IAtfActionStorageSaver saver;

        private Dictionary<string, Dictionary<FakeInput, Dictionary<object, Queue<AtfAction>>>> ActionStorage;
        private Dictionary<FakeInput, Dictionary<object, Queue<AtfAction>>> PlayStorage;

        private string CurrentRecordName;
        
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
                    return PlayStorage[kind][fakeInputParameter].Dequeue().Content;
                }
                return PlayStorage[kind][fakeInputParameter].Peek().Content;
            } catch (Exception)
            {
                if (DependencyInjector.DebugOn) print("Clearing play cache");
                recorder.StopPlay();
                ClearPlayStorage();
            }
            return null;
        }

        public void Enqueue(string recordName, FakeInput kind, object fakeInputParameter, AtfAction atfAction)
        {
            BeforeIfNameAndKindAndFipAreNotExistInStorage(recordName, kind, fakeInputParameter,
                (r, k, fip) => {
                    ActionStorage[r][k][fip].Enqueue(atfAction);
                },
                (r, k, fip) => {
                    ActionStorage.Add(r, new Dictionary<FakeInput, Dictionary<object, Queue<AtfAction>>>());
                    ActionStorage[r].Add(k, new Dictionary<object, Queue<AtfAction>>());
                    ActionStorage[r][k][fip] = new Queue<AtfAction>();
                });
        }

        public AtfAction Peek(string recordName, FakeInput kind, object fakeInputParameter)
        {
            return IfNameAndKindAndFipIsNotExistInStorageReturnDefault(recordName, kind, fakeInputParameter,
                () => ActionStorage[recordName][kind][fakeInputParameter].Peek(), null);
        }

        public AtfAction Dequeue(string recordName, FakeInput kind, object fakeInputParameter)
        {
            return IfNameAndKindAndFipIsNotExistInStorageReturnDefault(recordName, kind, fakeInputParameter,
                () => ActionStorage[recordName][kind][fakeInputParameter].Dequeue(), null);
        }

        public void Initialize()
        {
            ActionStorage = new Dictionary<string, Dictionary<FakeInput, Dictionary<object, Queue<AtfAction>>>>();
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

        public string GetCurrentRecordName()
        {
            if (string.IsNullOrEmpty(CurrentRecordName))
            {
                CurrentRecordName = "DefaultRecord";
            }
            return CurrentRecordName;
        }

        public void SetCurrentRecordName(string recordName)
        {
            CurrentRecordName = recordName;
        }

        public void LoadStorage()
        {
            saver.SetCurrentRecordName(GetCurrentRecordName());
            var loadedData = saver.GetActions();
            switch (loadedData)
            {
                case Dictionary<string, Dictionary<FakeInput, Dictionary<object, Queue<AtfAction>>>> data:
                    ActionStorage = ReturnNewCopyOf(data);
                    break;
                case Dictionary<FakeInput, Dictionary<object, Queue<AtfAction>>> data:
                    ActionStorage[GetCurrentRecordName()] = ReturnNewCopyOf(data);
                    break;
            }
        }

        public void SaveStorage()
        {
            saver.SetCurrentRecordName(GetCurrentRecordName());
            saver.SetActions(ActionStorage);
        }

        public void ScrapSavedStorage()
        {
            saver.SetCurrentRecordName(GetCurrentRecordName());
            saver.ScrapSavedActions();
        }

        public List<TreeViewItem> GetSavedRecordNames()
        {
            saver.SetCurrentRecordName(GetCurrentRecordName());
            var temp = saver.GetActions();
            return null;
        }

        public List<TreeViewItem> GetCurrentRecordNames()
        {
            var result = new List<TreeViewItem>();
            foreach (var key in ActionStorage.Keys)
            {
                result.Add(new TreeViewItem
                {
                    id = DictionaryBasedIdGenerator.GetNewId(key),
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
                    id = DictionaryBasedIdGenerator.GetNewId(fakeInputAndDictionary.Key.ToString()),
                    depth = 0,
                    displayName = fakeInputAndDictionary.Key.ToString()
                };
                result.Add(rootFakeInput);
                foreach (var fakeInputParameterAndQueue in fakeInputAndDictionary.Value)
                {
                    var displayNameForFip = fakeInputParameterAndQueue.Key.ToString();
                    var fakeInputParameterViewItem = new TreeViewItem
                    {
                        id = DictionaryBasedIdGenerator.GetNewId(displayNameForFip),
                        depth = 1,
                        displayName = displayNameForFip
                    };
                    rootFakeInput.AddChild(fakeInputParameterViewItem);
                    foreach (var action in fakeInputParameterAndQueue.Value)
                    {
                        var actionTreeViewItem = new TreeViewItem
                        {
                            id = DictionaryBasedIdGenerator.GetNewId(action.Content.ToString()),
                            depth = 2,
                            displayName = action.Content.ToString()
                        };
                        fakeInputParameterViewItem.AddChild(actionTreeViewItem);
                    }
                }
            }
            return result;
        }

        public List<TreeViewItem> GetSavedActions(string recordName)
        {
            return null;
        }

        public static Dictionary<FakeInput, Dictionary<object, Queue<AtfAction>>> ReturnNewCopyOf(Dictionary<FakeInput, Dictionary<object, Queue<AtfAction>>> etalon)
        {
            var result = new Dictionary<FakeInput, Dictionary<object, Queue<AtfAction>>>();
            if (etalon == null) return result;
            foreach (var fi in etalon.Keys)
            {
                foreach (var objectToQueue in etalon.Values)
                {
                    result[fi] = new Dictionary<object, Queue<AtfAction>>();
                    foreach (var pair in objectToQueue)
                    {
                        result[fi][pair.Key] = new Queue<AtfAction>(pair.Value);
                    }
                }
            }
            return result;
        }

        public static Dictionary<string, Dictionary<FakeInput, Dictionary<object, Queue<AtfAction>>>> ReturnNewCopyOf(Dictionary<string, 
            Dictionary<FakeInput, Dictionary<object, Queue<AtfAction>>>> etalon)
        {
            var result =
                new Dictionary<string, Dictionary<FakeInput, Dictionary<object, Queue<AtfAction>>>>();
            foreach (var recordName in etalon.Keys)
            {
                result[recordName] = ReturnNewCopyOf(etalon[recordName]);
            }
            return result;
        }

        private void BeforeIfNameAndKindAndFipAreNotExistInStorage(string recordName, FakeInput kind, object fakeInputParameter,
            Action<string, FakeInput, object> then, Action<string, FakeInput, object> beforeAction)
        {
            if (!ActionStorage.ContainsKey(recordName) || !ActionStorage[recordName].ContainsKey(kind) || !ActionStorage[recordName][kind].ContainsKey(fakeInputParameter))
            {
                beforeAction(recordName, kind, fakeInputParameter);

            }
            then(recordName, kind, fakeInputParameter);
        }

        private T IfNameAndKindAndFipIsNotExistInStorageReturnDefault<T>(string recordName, FakeInput kind, object fakeInputParameter, Func<T> toReturn, T defaultValue)
        {
            if (!ActionStorage.ContainsKey(recordName) || !ActionStorage[recordName].ContainsKey(kind) || !ActionStorage[recordName][kind].ContainsKey(fakeInputParameter))
            {
                return defaultValue;
            }

            return toReturn();
        }

        
    }
}