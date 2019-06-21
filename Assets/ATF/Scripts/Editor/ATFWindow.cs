using UnityEditor;

namespace ATF.Scripts.Editor
{
    public class ATFWindow : EditorWindow
    {
        [MenuItem("ATF/Recorder")]
        public static void ShowRecorder()
        {
            GetWindow(typeof(ATFRecorderWindow));
        }

        [MenuItem("ATF/Storage")]
        public static void ShowStorage()
        {
            GetWindow(typeof(ATFStorageWindow));
        }

        [MenuItem("ATF/Integrator")]
        public static void ShowIntegrator()
        {
            GetWindow(typeof(ATFIntegratorWindow));
        }
    }
}
