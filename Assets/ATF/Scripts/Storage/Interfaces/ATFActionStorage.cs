using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace ATF.Scripts.Storage.Interfaces
{
    public interface IATFActionStorage : IATFInitializable
    {
        object GetPartOfRecord(FakeInput kind, object fakeInputParameter);
        void Enqueue(string recordName, FakeInput kind, object fakeInputParameter, Action action);
        Action Dequeue(string recordName, FakeInput kind, object fakeInputParameter);
        Action Peek(string recordName, FakeInput kind, object fakeInputParameter);
        bool PrepareToPlayRecord(string recordName);
        void ClearPlayStorage();
        void SaveStorage(string recordName);
        void LoadStorage(string recordName);
        void ScrapSavedStorage(string recordName);
        List<TreeViewItem> GetSavedRecordNames();
        List<TreeViewItem> GetCurrentRecordNames();
        List<TreeViewItem> GetCurrentActions(string recordName);
        List<TreeViewItem> GetSavedActions(string recordName);
    }
}
