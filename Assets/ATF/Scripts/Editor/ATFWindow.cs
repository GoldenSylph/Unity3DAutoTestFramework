using UnityEditor;

namespace ATF.Scripts.Editor
{
    public class AtfWindow : EditorWindow
    {
        [MenuItem("ATF/Recorder")]
        public static void ShowRecorder()
        {
            GetWindow(typeof(AtfRecorderWindow));
        }

        [MenuItem("ATF/Storage")]
        public static void ShowStorage()
        {
            GetWindow(typeof(AtfStorageWindow));
        }

        [MenuItem("ATF/Integrator")]
        public static void ShowIntegrator()
        {
            GetWindow(typeof(AtfIntegratorWindow));
        }
    }
}
