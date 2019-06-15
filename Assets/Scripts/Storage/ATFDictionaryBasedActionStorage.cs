using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bedrin.Helper;
using ATF.InputTest;
using Bedrin.DI;

namespace ATF.Storage
{
    [Injectable]
    public class ATFDictionaryBasedActionStorage : MonoSingleton<ATFDictionaryBasedActionStorage>, IATFActionStorage
    {
        private Dictionary<string, Dictionary<FakeInput, Queue<Action>>> ActionStorage;
        private Dictionary<string, Dictionary<FakeInput, bool>> LeversStorage;

        public object GetContentOfRecordingAndType(string recordName, FakeInput kind)
        {
            if (LeversStorage[recordName][kind])
            {
                return ActionStorage[recordName][kind].Peek().content;
            }
            return null;
        }

        public void SetLever(string recordName, FakeInput kind, bool value)
        {
            LeversStorage[recordName][kind] = value;
        }

        public void Enqueue(string recordName, FakeInput kind, Action action)
        {
            if (ActionStorage[recordName][kind] == null)
            {
                ActionStorage[recordName][kind] = new Queue<Action>();
            }
            ActionStorage[recordName][kind].Enqueue(action);
        }

        public Action Dequeue(string recordName, FakeInput kind)
        {
            return ActionStorage[recordName][kind].Dequeue();
        }

        public float GetPeekDuration(string recordName, FakeInput kind)
        {
            return ActionStorage[recordName][kind].Peek().duration;
        }

        public void Initialize()
        {
            ActionStorage = new Dictionary<string, Dictionary<FakeInput, Queue<Action>>>();
            LeversStorage = new Dictionary<string, Dictionary<FakeInput, bool>>();
        }

    }
}