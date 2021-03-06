﻿using System.Collections;
using System.Collections.Generic;
using ATF.Scripts.Helper;
using UnityEditor.IMGUI.Controls;

namespace ATF.Scripts.Storage.Interfaces
{
    public interface IAtfActionStorageSaver : IAtfGetSetRecordName
    {
        void SaveRecord();
        void LoadRecord();
        void ScrapRecord();

        IEnumerable GetActions();
        void SetActions(IEnumerable actionEnumerable);
        List<TreeViewItem> GetSavedNames();
        List<TreeViewItem> GetSavedRecordDetails(string recordName);
        void ExportFile(string fullPath);
        void ImportFile(string fullPath);
    }
}
