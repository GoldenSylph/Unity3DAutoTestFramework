using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ATF.Integrator;

namespace ATF.Editor
{
    public class ATFIntegratorWindow : EditorWindow
    {
        public IATFIntegrator Integrator;

        private void OnFocus()
        {
            if (EditorApplication.isPlaying)
            {
                Integrator = FindObjectOfType<ATFFileSystemBasedIntegrator>();
            }
        }

        private void OnGUI()
        {
            bool integratorLoaded = Integrator != null;
            if (EditorApplication.isPlaying)
            {
                GUILayout.Label("Integrator Control Panel", EditorStyles.boldLabel);
                if (integratorLoaded)
                {
                    GUILayout.Label(string.Format("Integrator current realisation: {0}", Integrator.GetType().Name), EditorStyles.label);
                } else
                {
                    GUILayout.Label("Integrator current realisation: Waiting to focus...", EditorStyles.label);
                }
            }
            else
            {
                GUILayout.Label("Waiting for Play Mode...", EditorStyles.boldLabel);
            }
        }
    }
}
