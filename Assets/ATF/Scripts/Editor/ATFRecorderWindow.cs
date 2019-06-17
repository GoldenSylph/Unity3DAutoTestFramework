using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ATF.Recorder;
using UnityEditor;

namespace ATF.Editor
{
    public class ATFRecorderWindow : EditorWindow
    {

        public IATFRecorder Recorder;

        private void OnFocus()
        {
            if (EditorApplication.isPlaying)
            {
                Recorder = FindObjectOfType<ATFQueueBasedRecorder>();
            }
        }

        private void OnGUI()
        {
            bool recorderLoaded = Recorder != null;
            if (EditorApplication.isPlaying)
            {
                GUILayout.Label("Recorder Settings", EditorStyles.boldLabel);
                if (recorderLoaded)
                {
                    GUILayout.Label(string.Format("Recorder current realisation: {0}", Recorder.GetType()), EditorStyles.label);
                    GUILayout.Label("Recorder state", EditorStyles.boldLabel);

                    EditorGUILayout.Toggle("Is Playing", Recorder.IsPlaying());
                    EditorGUILayout.Toggle("Is Recording", Recorder.IsRecording());
                    EditorGUILayout.Toggle("Is Recording Paused", Recorder.IsRecordingPaused());
                    EditorGUILayout.Toggle("Is Playing Paused", Recorder.IsPlayPaused());

                    GUILayout.Label("Recording control", EditorStyles.boldLabel);
                    Recorder.SetCurrentRecordingName(EditorGUILayout.TextField("Name of the recording", Recorder.GetCurrentRecordingName()));
                    GUILayout.Label(string.Format("Current recording name: {0}", Recorder.GetCurrentRecordingName()), EditorStyles.label);

                    if (!Recorder.IsPlaying())
                    {
                        EditorGUILayout.BeginHorizontal();
                        if (!Recorder.IsRecording() && GUILayout.Button("Start"))
                        {
                            Recorder.StartRecord();
                        }

                        if (Recorder.IsRecording() && !Recorder.IsRecordingPaused() && GUILayout.Button("Pause"))
                        {
                            Recorder.PauseRecord();
                        }

                        if (Recorder.IsRecording() && Recorder.IsRecordingPaused() && GUILayout.Button("Continue"))
                        {
                            Recorder.ContinueRecord();
                        }

                        if (Recorder.IsRecording() && GUILayout.Button("Stop"))
                        {
                            Recorder.StopRecord();
                        }

                        EditorGUILayout.EndHorizontal();
                    }


                    if (!Recorder.IsRecording())
                    {
                        GUILayout.Label("Replay control", EditorStyles.boldLabel);
                        EditorGUILayout.BeginHorizontal();
                        if (!Recorder.IsPlaying() && GUILayout.Button("Start"))
                        {
                            Recorder.PlayRecord();
                        }

                        if (Recorder.IsPlaying() && !Recorder.IsPlayPaused() && GUILayout.Button("Pause"))
                        {
                            Recorder.PausePlay();
                        }

                        if (Recorder.IsPlaying() && Recorder.IsPlayPaused() && GUILayout.Button("Continue"))
                        {
                            Recorder.ContinuePlay();
                        }

                        if (Recorder.IsPlaying() && GUILayout.Button("Stop"))
                        {
                            Recorder.StopPlay();
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                else
                {
                    GUILayout.Label("Recorder current realisation: Waiting to focus...", EditorStyles.label);
                }
            }
            else
            {
                GUILayout.Label("Waiting for Play Mode...", EditorStyles.boldLabel);
            }
        }
    }
}
