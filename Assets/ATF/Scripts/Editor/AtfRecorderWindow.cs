using ATF.Scripts.Recorder;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace ATF.Scripts.Editor
{
    public class AtfRecorderWindow : EditorWindow
    {

        public IAtfRecorder recorder;

        private string _newNameOfRecording;

        private void OnFocus()
        {
            if (!EditorApplication.isPlaying) return;
            recorder = FindObjectOfType<AtfQueueBasedRecorder>();
        }

        private void OnGUI()
        {
            var recorderLoaded = recorder != null;
            if (EditorApplication.isPlaying)
            {
                GUILayout.Label("Recorder Settings", EditorStyles.boldLabel);
                if (recorderLoaded)
                {
                    GUILayout.Label($"Recorder realisation: {recorder.GetType().Name}", EditorStyles.label);
                    GUILayout.Label("Recorder state", EditorStyles.boldLabel);

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.Toggle("Is Playing", recorder.IsPlaying());
                    EditorGUILayout.Toggle("Is Recording", recorder.IsRecording());
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.Toggle("Is Recording Paused", recorder.IsRecordingPaused());
                    EditorGUILayout.Toggle("Is Playing Paused", recorder.IsPlayPaused());
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.EndHorizontal();
                    
                    GUILayout.Label("Recording control", EditorStyles.boldLabel);
                    if (!recorder.IsPlaying() && !recorder.IsRecording())
                    {
                        _newNameOfRecording = EditorGUILayout.TextField("Name of the recording", _newNameOfRecording);
                        if (Event.current.keyCode == KeyCode.Return)
                        {
                            recorder.SetCurrentRecordName(_newNameOfRecording);  
                        }
                    }
                    GUILayout.Label($"Current recording name: {recorder.GetCurrentRecordName()}", EditorStyles.label);

                    if (!recorder.IsPlaying())
                    {
                        EditorGUILayout.BeginHorizontal();
                        if (!recorder.IsRecording() && GUILayout.Button("Start"))
                        {
                            recorder.StartRecord();
                        }

                        if (recorder.IsRecording() && !recorder.IsRecordingPaused() && GUILayout.Button("Pause"))
                        {
                            recorder.PauseRecord();
                        }

                        if (recorder.IsRecording() && recorder.IsRecordingPaused() && GUILayout.Button("Continue"))
                        {
                            recorder.ContinueRecord();
                        }

                        if (recorder.IsRecording() && GUILayout.Button("Stop"))
                        {
                            recorder.StopRecord();
                        }

                        EditorGUILayout.EndHorizontal();
                    }


                    if (recorder.IsRecording()) return;
                    GUILayout.Label("Replay control", EditorStyles.boldLabel);
                    EditorGUILayout.BeginHorizontal();
                    if (!recorder.IsPlaying() && GUILayout.Button("Start"))
                    {
                        recorder.PlayRecord();
                    }

                    if (recorder.IsPlaying() && !recorder.IsPlayPaused() && GUILayout.Button("Pause"))
                    {
                        recorder.PausePlay();
                    }

                    if (recorder.IsPlaying() && recorder.IsPlayPaused() && GUILayout.Button("Continue"))
                    {
                        recorder.ContinuePlay();
                    }

                    if (recorder.IsPlaying() && GUILayout.Button("Stop"))
                    {
                        recorder.StopPlay();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.Label("Recorder realisation: Waiting to focus...", EditorStyles.label);
                }
            }
            else
            {
                GUILayout.Label("Waiting for Play Mode...", EditorStyles.boldLabel);
            }
        }
    }
}
