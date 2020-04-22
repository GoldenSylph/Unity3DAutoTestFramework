using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using ATF.Scripts.DI;
using ATF.Scripts.Helper;
using ATF.Scripts.Recorder;
using ATF.Scripts.Storage.Interfaces;
using ATF.Scripts.Storage.Utils;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Serialization;

namespace ATF.Scripts.Storage
{
    [Injectable]
    [AtfSystem]
    public class AtfDictionaryBasedActionStorage : MonoSingleton<AtfDictionaryBasedActionStorage>, IAtfActionStorage
    {

        [Inject(typeof(AtfQueueBasedRecorder))]
        public IAtfRecorder recorder;

        [Inject(typeof(AtfPlayerPrefsBasedActionStorageSaver))]
        public IAtfActionStorageSaver saver;

        private Dictionary<string, Dictionary<FakeInput, Dictionary<object, AtfActionRleQueue>>> _actionStorage;
        private Dictionary<FakeInput, Dictionary<object, AtfActionRleQueue>> _playStorage;

        private string _currentRecordName;

        public object GetPartOfRecord(FakeInput kind, object fakeInputParameter)
        {
            if (kind == FakeInput.NONE 
                || _playStorage == null 
                || !_playStorage.ContainsKey(kind)
                || !_playStorage[kind].ContainsKey(fakeInputParameter))
            {
                return null;
            }
            try
            {
                if (AtfInitializer.Instance.isDebugPrintOn)
                {
                    print($"Action to deliver remain: {_playStorage[kind][fakeInputParameter].Count}");
                }
                if (recorder.IsPlaying() && !recorder.IsPlayPaused())
                {
                    return _playStorage[kind][fakeInputParameter].Dequeue().Content;
                }
                return _playStorage[kind][fakeInputParameter].Peek().Content;
            } catch (Exception e)
            {
                Debug.Log(e);
                if (AtfInitializer.Instance.isDebugPrintOn) print("Clearing play cache");
                recorder.StopPlay();
                ClearPlayStorage();
            }
            return null;
        }

        public void Enqueue(string recordName, FakeInput kind, object fakeInputParameter, AtfAction atfAction)
        {
            
            if (!_actionStorage.ContainsKey(recordName))
            {
                _actionStorage.Add(recordName, new Dictionary<FakeInput, Dictionary<object, AtfActionRleQueue>>());
            }

            if (!_actionStorage[recordName].ContainsKey(kind))
            {
                _actionStorage[recordName].Add(kind, new Dictionary<object, AtfActionRleQueue>());
            }

            if (!_actionStorage[recordName][kind].ContainsKey(fakeInputParameter))
            {
                _actionStorage[recordName][kind][fakeInputParameter] = new AtfActionRleQueue();
            }
            
            _actionStorage[recordName][kind][fakeInputParameter].Enqueue(atfAction);
        }

        public AtfAction Peek(string recordName, FakeInput kind, object fakeInputParameter)
        {
            return IfNameAndKindAndFipIsNotExistInStorageReturnDefault(recordName, kind, fakeInputParameter,
                () => _actionStorage[recordName][kind][fakeInputParameter].Peek(), null);
        }

        public AtfAction Dequeue(string recordName, FakeInput kind, object fakeInputParameter)
        {
            return IfNameAndKindAndFipIsNotExistInStorageReturnDefault(recordName, kind, fakeInputParameter,
                () => _actionStorage[recordName][kind][fakeInputParameter].Dequeue(), null);
        }

        public override void Initialize()
        {
            _actionStorage = new Dictionary<string, Dictionary<FakeInput, Dictionary<object, AtfActionRleQueue>>>();
            base.Initialize();
        }

        public bool PrepareToPlayRecord(string recordName)
        {
            if (!_actionStorage.ContainsKey(recordName)) return false;
            _playStorage = ReturnNewCopyOf(_actionStorage[recordName]);
            return true;
        }

        public void ClearPlayStorage()
        {
            _playStorage = null;
        }

        public string GetCurrentRecordName()
        {
            if (string.IsNullOrEmpty(_currentRecordName))
            {
                _currentRecordName = "DefaultRecord";
            }
            return _currentRecordName;
        }

        public void SetCurrentRecordName(string recordName)
        {
            _currentRecordName = recordName;
        }

        public void LoadStorage()
        {
            saver.SetCurrentRecordName(GetCurrentRecordName());
            saver.LoadRecord();
            var loadedData = (Dictionary<string, Dictionary<FakeInput, Dictionary<object, AtfActionRleQueue>>>) saver.GetActions();
            _actionStorage = Merged(_actionStorage, loadedData);
        }

        public void SaveStorage()
        {
            saver.SetCurrentRecordName(GetCurrentRecordName());
            saver.SetActions(_actionStorage);
            saver.SaveRecord();
        }

        public void ScrapSavedStorage()
        {
            saver.SetCurrentRecordName(GetCurrentRecordName());
            saver.ScrapRecord();
        }

        public List<TreeViewItem> GetSavedRecordNames()
        {
            return saver.GetSavedNames();
        }

        public List<TreeViewItem> GetCurrentRecordNames()
        {
            var result = _actionStorage.Keys
                .Select(key => new TreeViewItem
                {
                    id = DictionaryBasedIdGenerator.GetNewId(key), depth = 0, displayName = key
                }).ToList();
            return result.Count == 0 ? null : result;
        }

        public List<TreeViewItem> GetCurrentActions(string recordName)
        {
            var result = new List<TreeViewItem>();
            if (!_actionStorage.ContainsKey(recordName)) return null;
            foreach (var fakeInputAndDictionary in _actionStorage[recordName])
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
                    foreach (var actionTreeViewItem in fakeInputParameterAndQueue.Value.Select((action, index) => new TreeViewItem
                    {
                        id = DictionaryBasedIdGenerator.GetNewId(action.Content.ToString()),
                        depth = 2,
                        displayName = $"{action.Content} - {fakeInputParameterAndQueue.Value.GetRepetitions(index)} repetitions"
                    }))
                    {
                        fakeInputParameterViewItem.AddChild(actionTreeViewItem);
                    }
                }
            }
            return result;
        }

        public List<TreeViewItem> GetSavedActions(string recordName)
        {
            return saver.GetSavedRecordDetails(recordName);
        }

        public static Dictionary<string, Dictionary<FakeInput, Dictionary<object, AtfActionRleQueue>>> ReturnNewCopyOf(Dictionary<string, 
            Dictionary<FakeInput, Dictionary<object, AtfActionRleQueue>>> etalon)
        {
            var result =
                new Dictionary<string, Dictionary<FakeInput, Dictionary<object, AtfActionRleQueue>>>();
            foreach (var recordName in etalon.Keys)
            {
                result[recordName] = ReturnNewCopyOf(etalon[recordName]);
            }
            return result;
        }

        private T IfNameAndKindAndFipIsNotExistInStorageReturnDefault<T>(string recordName, FakeInput kind, object fakeInputParameter, Func<T> toReturn, T defaultValue)
        {
            if (!_actionStorage.ContainsKey(recordName) || !_actionStorage[recordName].ContainsKey(kind) || !_actionStorage[recordName][kind].ContainsKey(fakeInputParameter))
            {
                return defaultValue;
            }

            return toReturn();
        }

        private static Dictionary<string, Dictionary<FakeInput, Dictionary<object, AtfActionRleQueue>>> Merged(
            Dictionary<string, Dictionary<FakeInput, Dictionary<object, AtfActionRleQueue>>> first,
            Dictionary<string, Dictionary<FakeInput, Dictionary<object, AtfActionRleQueue>>> second)
        {
            foreach (var key in second.Keys)
            {
                first.Add(key, second[key]);
            }
            return first;
        }
        
        private static Dictionary<FakeInput, Dictionary<object, AtfActionRleQueue>> ReturnNewCopyOf(Dictionary<FakeInput, Dictionary<object, AtfActionRleQueue>> etalon)
        {
            var result = new Dictionary<FakeInput, Dictionary<object, AtfActionRleQueue>>();
            if (etalon == null) return result;
            foreach (var fi in etalon.Keys)
            {
                result[fi] = new Dictionary<object, AtfActionRleQueue>();
                foreach (var objectToQueue in etalon[fi])
                {
                    result[fi][objectToQueue.Key] = new AtfActionRleQueue(objectToQueue.Value);
                }
            }
            return result;
        }
    }
}