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
                    GUILayout.Label(string.Format("Storage current realisation: {0}", Storage.GetType().Name), EditorStyles.label);
                }
                else
                {
                    GUILayout.Label("Storage current realisation: Waiting to focus...", EditorStyles.label);
                }

                //Save/Load Control
                //search view if searching not all if all then searchview is disabled and no filters acqiuired
                //get saved searched record names

                //Tree view

            }
            else
            {
                GUILayout.Label("Waiting for Play Mode...", EditorStyles.boldLabel);
            }
        }
    }
}