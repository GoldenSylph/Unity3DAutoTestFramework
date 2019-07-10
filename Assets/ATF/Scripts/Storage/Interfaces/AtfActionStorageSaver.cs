using System.Collections;

namespace ATF.Scripts.Storage.Interfaces
{
    public interface IAtfActionStorageSaver : IAtfInitializable, IAtfGetSetRecordName
    {
        void SaveAll();
        void LoadAll();
        void SaveRecord();
        void LoadRecord();
        void ScrapRecord();
        void ScrapAll();

        IEnumerable GetActions();
        void SetActions(IEnumerable actionEnumerable);
        void ScrapSavedActions();
    }
}
