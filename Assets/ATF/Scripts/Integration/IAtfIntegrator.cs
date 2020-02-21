using System;
using System.Collections.Generic;

namespace ATF.Scripts.Integration
{
    public interface IAtfIntegrator : IAtfInitializable, IAtfGetSetRecordName
    {
        void SetUrls(IEnumerable<string> filePaths);
        void Integrate();
        void IntegrateAndReplace();
    }
}
