using System.Collections.Generic;
using ATF.Scripts.Editor2.StorageTreeView;

namespace ATF.Scripts.Storage.Interfaces
{
    public interface IATFActionStorage : IATFInitializable
    {
        object GetPartOfRecord(FakeInput kind);
        void Enqueue(string recordName, FakeInput kind, Action action);
        Action Dequeue(string recordName, FakeInput kind);
        Action Peek(string recordName, FakeInput kind);
        bool PrepareToPlayRecord(string recordName);
        void ClearPlayStorage();
        void SaveStorage(string recordName);
        void LoadStorage(string recordName);
        void ScrapSavedStorage(string recordName);
        List<ATFStorageTreeElement> PrepareToDrawOnEditor();
    }
}
