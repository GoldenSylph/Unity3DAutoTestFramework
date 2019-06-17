using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ATF.Storage;

namespace ATF.Editor
{
    public class ATFStorageWindow : EditorWindow
    {
        public IATFActionStorage Storage;
        
        private void OnFocus()
        {
            if (EditorApplication.isPlaying)
            {
                Storage = FindObjectOfType<ATFDictionaryBasedActionStorage>();
            }
        }

        private void OnGUI()
        {
            bool storageLoaded = Storage != null;

            if (EditorApplication.isPlaying)
            {
                GUILayout.Label("Storage Settings", EditorStyles.boldLabel);
                if (storageLoaded)
                {
                    GUILayout.Label(string.Format("Storage current realisation: {0}", Storage.GetType()), EditorStyles.label);
                }
                else
                {
                    GUILayout.Label("Storage current realisation: Waiting to focus...", EditorStyles.label);
                }
            }
            else
            {
                GUILayout.Label("Waiting for Play Mode...", EditorStyles.boldLabel);
            }
        }
    }
}