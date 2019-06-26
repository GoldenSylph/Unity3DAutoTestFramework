﻿using System;
using System.Collections.Generic;
using ATF.Scripts.Recorder;
using ATF.Scripts.Storage.Interfaces;
using Bedrin.DI;
using Bedrin.Helper;
using UnityEditor.IMGUI.Controls;

namespace ATF.Scripts.Storage
{
    public class ATFIdHelper
    {
        private static int _idCounter;
        
        public static int GetNewId()
        {
            return ++_idCounter;
        }
    }
    
    [Injectable]
    public class ATFDictionaryBasedActionStorage : MonoSingleton<ATFDictionaryBasedActionStorage>, IATFActionStorage
    {

        [Inject(typeof(ATFQueueBasedRecorder))]
        public static readonly IATFRecorder Recorder;

        [Inject(typeof(ATFPlayerPrefsBasedActionStorageSaver))]
        public static readonly IATFActionStorageSaver Saver;

        private Dictionary<string, Dictionary<FakeInput, Queue<Action>>> ActionStorage;
        private Dictionary<FakeInput, Queue<Action>> PlayStorage;

        public object GetPartOfRecord(FakeInput kind)
        {
            if (kind == FakeInput.NONE || PlayStorage == null || !PlayStorage.ContainsKey(kind)) return null;
            try
            {
                if (DependencyInjector.DebugOn)
                {
                    print($"Action to deliver remain: {PlayStorage[kind].Count}");
                }
                if (Recorder.IsPlaying() && !Recorder.IsPlayPaused())
                {
                    return PlayStorage[kind].Dequeue().content;
                }

                return PlayStorage[kind].Peek().content;
            } catch (Exception)
            {
                if (DependencyInjector.DebugOn) print("Clearing play cache");
                Recorder.StopPlay();
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
            Saver.SetRecordName(recordName);
            var loadedData = Saver.GetActions();
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
            Saver.SetRecordName(recordName);
            Saver.SetActions(ActionStorage);
        }

        public void ScrapSavedStorage(string recordName)
        {
            Saver.SetRecordName(recordName);
            Saver.ScrapSavedActions();
        }

        public List<TreeViewItem> GetSavedRecordNames()
        {
            throw new NotImplementedException();
        }

        public List<TreeViewItem> GetCurrentRecordNames()
        {
            var result = new List<TreeViewItem>();
            foreach (var key in ActionStorage.Keys)
            {
                result.Add(new TreeViewItem
                {
                    id = ATFIdHelper.GetNewId(),
                    depth = 0,
                    displayName = key
                });
            }
            return result;
        }

        public List<TreeViewItem> GetCurrentActions(string recordName)
        {
            var result = new List<TreeViewItem>();
            foreach (var pair in ActionStorage[recordName])
            {
                result.Add(new TreeViewItem
                {
                    id = ATFIdHelper.GetNewId(),
                    depth = 0,
                    displayName = pair.
                });
            }
            return result;
        }

        public List<TreeViewItem> GetSavedActions(string recordName)
        {
            throw new NotImplementedException();
        }

        public static Dictionary<FakeInput, Queue<Action>> ReturnNewCopyOf(Dictionary<FakeInput, Queue<Action>> etalon)
        {
            var result = new Dictionary<FakeInput, Queue<Action>>();
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