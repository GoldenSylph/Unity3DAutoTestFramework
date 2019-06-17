using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bedrin.Helper;

namespace ATF.Integrator
{
    public class ATFFileSystemBasedIntegrator : MonoSingleton<ATFFileSystemBasedIntegrator>,  IATFIntegrator
    {
        public void Initialize()
        {
            print("Integrator initialized");
        }
    }
}
