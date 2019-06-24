using ATF.Scripts.Integration;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace ATF.Scripts.Editor
{
    public class ATFIntegratorWindow : EditorWindow
    {
        public IATFIntegrator integrator;

        private void OnFocus()
        {
            if (EditorApplication.isPlaying)
            {
                integrator = FindObjectOfType<ATFFileSystemBasedIntegrator>();
            }
        }

        private void OnGUI()
        {
            var integratorLoaded = integrator != null;
            if (EditorApplication.isPlaying)
            {
                GUILayout.Label("Integrator Control Panel", EditorStyles.boldLabel);
                GUILayout.Label(
                    integratorLoaded
                        ? $"Integrator current realisation: {integrator.GetType().Name}"
                        : "Integrator current realisation: Waiting to focus...", EditorStyles.label);
            }
            else
            {
                GUILayout.Label("Waiting for Play Mode...", EditorStyles.boldLabel);
            }
        }
    }
}
