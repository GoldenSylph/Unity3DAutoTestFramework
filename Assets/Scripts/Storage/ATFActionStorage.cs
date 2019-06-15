using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bedrin.Helper;
using Bedrin.DI;

namespace ATF.Storage
{
    public interface IATFActionStorage : IATFInitializable
    {
        object GetContentOfRecordingAndType(string recordName, FakeInput kind);
        void SetLever(string recordName, FakeInput kind, bool value);
        void Enqueue(string recordName, FakeInput kind, Action action);
        Action Dequeue(string recordName, FakeInput kind);
        float GetPeekDuration(string recordName, FakeInput kind);
    }
}
