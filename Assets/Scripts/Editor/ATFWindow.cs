using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ATF.Editor
{
    public class ATFWindow : ScriptableWizard
    {
        [MenuItem("ATF/View Debug Window...")]
        public static void ViewDebugWindow()
        {
            DisplayWizard<ATFWindow>("ATF Debug Window", "Start Record", "Play");
        }
    }
}
