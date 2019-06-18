using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bedrin.Helper;
using Bedrin.DI;
using ATF.Recorder;
using System;

namespace ATF.Storage
{
    [Injectable]
    public class ATFDictionaryBasedActionStorage : MonoSingleton<ATFDictionaryBasedActionStorage>, IATFActionStorage
    {

        [Inject(typeof(ATFQueueBasedRecorder))]
        public static readonly IATFRecorder RECORDER;

        [Inject(typeof(ATFPlayerPrefsBasedActionStorageSaver))]
        public static readonly IATFActionStorageSaver SAVER;

        private Dictionary<string, Dictionary<FakeInput, Queue<Action>>> ActionStorage;
        private Dictionary<FakeInput, Queue<Action>> PlayStorage;

        public object GetPartOfRecord(FakeInput kind)
        {
            if (kind != FakeInput.NONE && PlayStorage != null && PlayStorage.ContainsKey(kind))
            {
                try
                {
                    if (DependencyInjector.DebugOn)
                    {
                        print(string.Format("Action to deliver remain: {0}", PlayStorage[kind].Count));
                    }
                    if (RECORDER.IsPlaying() && !RECORDER.IsPlayPaused())
                    {
                        return PlayStorage[kind].Dequeue().content;
                    } else
                    {
                        return PlayStorage[kind].Peek().content;
                    }
                } catch (Exception)
                {
                    if (DependencyInjector.DebugOn) print("Clearing play cache");
                    RECORDER.StopPlay();
                    ClearPlayStorage();
                }
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
            return IfNameAndKindIsNotExitstInStorageReturnDefault(recordName, kind,
                () => ActionStorage[recordName][kind].Peek(), null);
        }

        public Action Dequeue(string recordName, FakeInput kind)
        {
            return IfNameAndKindIsNotExitstInStorageReturnDefault(recordName, kind,
                () => ActionStorage[recordName][kind].Dequeue(), null);
        }

        public void Initialize()
        {
            ActionStorage = new Dictionary<string, Dictionary<FakeInput, Queue<Action>>>();
        }

        public bool PrepareToPlayRecord(string recordName)
        {
            if (ActionStorage.ContainsKey(recordName))
            {
                PlayStorage = ReturnNewCopyOf(ActionStorage[recordName]);
                return true;
            } else
            {
                return false;
            }
        }

        public void ClearPlayStorage()
        {
            PlayStorage = null;
        }

        public void LoadStorage(string recordName)
        {
            SAVER.SetRecordName(recordName);
            IEnumerable loadedData = SAVER.GetActions();
            if (loadedData is Dictionary<string, Dictionary<FakeInput, Queue<Action>>>) {
                ActionStorage = ReturnNewCopyOf(loadedData as Dictionary<string, Dictionary<FakeInput, Queue<Action>>>);
            }
            else if (loadedData is Dictionary<FakeInput, Queue<Action>>)
            {
                ActionStorage[recordName] = ReturnNewCopyOf(loadedData as Dictionary<FakeInput, Queue<Action>>);
            }
        }

        public void SaveStorage(string recordName)
        {
            SAVER.SetRecordName(recordName);
            SAVER.SetActions(ActionStorage);
        }

        public void ScrapSavedStorage(string recordName)
        {
            SAVER.SetRecordName(recordName);
            SAVER.ScrapSavedActions();
        }

        public static Dictionary<FakeInput, Queue<Action>> ReturnNewCopyOf(Dictionary<FakeInput, Queue<Action>> etalon)
        {
            Dictionary<FakeInput, Queue<Action>> result = new Dictionary<FakeInput, Queue<Action>>();
            foreach (FakeInput fi in etalon.Keys)
            {
                foreach (Queue<Action> q in etalon.Values)
                {
                    result[fi] = new Queue<Action>(q);
                }
            }
            return result;
        }

        public static Dictionary<string, Dictionary<FakeInput, Queue<Action>>> ReturnNewCopyOf(Dictionary<string,
            Dictionary<FakeInput, Queue<Action>>> etalon)
        {
            Dictionary<string, Dictionary<FakeInput, Queue<Action>>> result =
                new Dictionary<string, Dictionary<FakeInput, Queue<Action>>>();
            foreach (string recordName in etalon.Keys)
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

        private T IfNameAndKindIsNotExitstInStorageReturnDefault<T>(string recordName, FakeInput kind, Func<T> toReturn, T defaultValue)
        {
            if (!ActionStorage.ContainsKey(recordName) || !ActionStorage[recordName].ContainsKey(kind))
            {
                return defaultValue;
            }
            else
            {
                return toReturn();
            }
        }
    }
}