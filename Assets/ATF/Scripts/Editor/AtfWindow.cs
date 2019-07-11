using UnityEditor;

namespace ATF.Scripts.Editor
{
    public class AtfWindow : EditorWindow
    {
        [MenuItem("ATF/Recorder")]
        public static EditorWindow GetRecorderWindow()
        {
            return GetWindow(typeof(AtfRecorderWindow));
        }

        [MenuItem("ATF/Storage")]
        public static EditorWindow GetStorageWindow()
        {
            return GetWindow(typeof(AtfStorageWindow));
        }

        [MenuItem("ATF/Integrator")]
        public static EditorWindow GetIntegratorWindow()
        {
            return GetWindow(typeof(AtfIntegratorWindow));
        }
    }
}
