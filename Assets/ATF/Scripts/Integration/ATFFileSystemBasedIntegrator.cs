using Bedrin.Helper;

namespace ATF.Scripts.Integration
{
    public class ATFFileSystemBasedIntegrator : MonoSingleton<ATFFileSystemBasedIntegrator>,  IATFIntegrator
    {
        public void Initialize()
        {
            print("Integrator initialized");
        }
    }
}
