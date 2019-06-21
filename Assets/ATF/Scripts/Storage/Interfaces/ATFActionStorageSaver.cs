using System.Collections;

namespace ATF.Scripts.Storage.Interfaces
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
