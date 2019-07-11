using System.Collections;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace ATF.Scripts.Storage.Interfaces
{
    public interface IAtfActionStorageSaver : IAtfInitializable, IAtfGetSetRecordName
    {
        void SaveRecord();
        void LoadRecord();
        void ScrapRecord();

        IEnumerable GetActions();
        void SetActions(IEnumerable actionEnumerable);
        List<TreeViewItem> GetSavedNames();
        List<TreeViewItem> GetSavedRecordDetails(string recordName);
    }
}
