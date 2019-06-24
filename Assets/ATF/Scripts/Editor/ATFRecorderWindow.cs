﻿using ATF.Scripts.Recorder;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace ATF.Scripts.Editor
{
    public class ATFRecorderWindow : EditorWindow
    {

        public IATFRecorder recorder;

        private void OnFocus()
        {
            if (EditorApplication.isPlaying)
            {
                recorder = FindObjectOfType<ATFQueueBasedRecorder>();
            }
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
                    recorder.SetCurrentRecordingName(EditorGUILayout.TextField("Name of the recording", recorder.GetCurrentRecordingName()));
                    GUILayout.Label($"Current recording name: {recorder.GetCurrentRecordingName()}", EditorStyles.label);

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
