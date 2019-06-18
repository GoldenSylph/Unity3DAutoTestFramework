using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ATF.Storage
{
    public interface IATFActionStorageSaver : IATFInitializable
    {
        void SaveAll();
        void LoadAll();
        void SaveRecord();
        void LoadRecord();
        void ScrapRecord();
        void ScrapAll();

        void SetRecordName(string recordName);
        string GetRecordName();

        IEnumerable GetActions();
        void SetActions(IEnumerable actionEnumerable);
        void ScrapSavedActions();
    }
}
