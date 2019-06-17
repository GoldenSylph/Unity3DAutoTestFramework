using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bedrin.Helper;
using Bedrin.DI;

namespace ATF.Storage
{
    public interface IATFActionStorage : IATFInitializable
    {
        object GetPartOfRecord(FakeInput kind);
        void Enqueue(string recordName, FakeInput kind, Action action);
        Action Dequeue(string recordName, FakeInput kind);
        Action Peek(string recordName, FakeInput kind);
        bool PrepareToPlayRecord(string recordName);
        void ClearPlayStorage();
    }
}
