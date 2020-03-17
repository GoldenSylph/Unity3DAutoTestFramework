using System.Collections.Generic;
using ATF.Scripts.Helper;

namespace ATF.Scripts.Integration.Interfaces
{
    public interface IAtfIntegrator : IAtfGetSetRecordName
    {
        void SetUris(IEnumerable<string> filePaths);
        void Integrate();
        void IntegrateAndReplace();
        void IntegrateAll();
        void SaveUris();
        IEnumerable<string> LoadUris();
    }
}
