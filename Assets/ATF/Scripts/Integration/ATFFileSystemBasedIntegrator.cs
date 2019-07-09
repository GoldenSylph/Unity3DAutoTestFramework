using Bedrin.Helper;

namespace ATF.Scripts.Integration
{
    public class AtfFileSystemBasedIntegrator : MonoSingleton<AtfFileSystemBasedIntegrator>,  IAtfIntegrator
    {
        public void Initialize()
        {
            print("Integrator initialized");
        }
    }
}
