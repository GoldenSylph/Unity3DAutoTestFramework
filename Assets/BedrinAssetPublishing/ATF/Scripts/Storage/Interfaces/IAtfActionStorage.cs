using System.Collections.Generic;
using ATF.Scripts.Helper;
using ATF.Scripts.Storage.Utils;
using UnityEditor.IMGUI.Controls;

namespace ATF.Scripts.Storage.Interfaces
{
    public interface IAtfActionStorage : IAtfGetSetRecordName
    {
        object GetPartOfRecord(FakeInput kind, object fakeInputParameter);
        void Enqueue(string recordName, FakeInput kind, object fakeInputParameter, AtfAction atfAction);
        AtfAction Dequeue(string recordName, FakeInput kind, object fakeInputParameter);
        AtfAction Peek(string recordName, FakeInput kind, object fakeInputParameter);
        bool PrepareToPlayRecord(string recordName);
        void ClearPlayStorage();
        void SaveStorage();
        void LoadStorage();
        void ScrapSavedStorage();
        List<TreeViewItem> GetSavedRecordNames();
        List<TreeViewItem> GetCurrentRecordNames();
        List<TreeViewItem> GetCurrentActions(string recordName);
        List<TreeViewItem> GetSavedActions(string recordName);
    }
}
