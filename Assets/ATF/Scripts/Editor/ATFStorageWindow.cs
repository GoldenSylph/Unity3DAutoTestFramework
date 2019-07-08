using System;
using System.Collections.Generic;
using ATF.Scripts.Recorder;
using ATF.Scripts.Storage;
using ATF.Scripts.Storage.Interfaces;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Serialization;

namespace ATF.Scripts.Editor
{
    public class ATFStorageWindow : EditorWindow
    {
        [SerializeField]
        private TreeViewState treeViewStateForCurrentNames;
        
        [SerializeField]
        private TreeViewState treeViewStateForCurrentKindsAndActions;
        
        [SerializeField]
        private TreeViewState treeViewStateForSavedNames;
        
        private ATFStorageTreeView TreeViewForCurrentNames;
        private SearchField SearchFieldForCurrentNames;

        private ATFStorageTreeView TreeViewForCurrentKindsAndActions;
        private SearchField SearchFieldForCurrentKindsAndActions;
        
        private ATFStorageTreeView TreeViewForSavedNames;
        private SearchField SearchFieldForSavedNames;
        
        public IATFActionStorage storage;
        public IATFRecorder recorder;

        private void OnFocus()
        {
            if (!EditorApplication.isPlaying) return;
            storage = FindObjectOfType<ATFDictionaryBasedActionStorage>();
            recorder = FindObjectOfType<ATFQueueBasedRecorder>();
            InitTreeViewOf(ref TreeViewForCurrentNames, ref SearchFieldForCurrentNames, ref treeViewStateForCurrentNames, TreePurpose.DRAW_CURRENT_NAMES, recorder, storage);
            InitTreeViewOf(ref TreeViewForCurrentKindsAndActions, ref SearchFieldForCurrentKindsAndActions, ref treeViewStateForCurrentKindsAndActions, TreePurpose.DRAW_KINDS_AND_ACTIONS, recorder, storage);
            InitTreeViewOf(ref TreeViewForSavedNames, ref SearchFieldForSavedNames, ref treeViewStateForSavedNames, TreePurpose.DRAW_SAVED_NAMES, recorder, storage);
            TreeViewForCurrentNames.KindsAndActionsTreeView = TreeViewForCurrentKindsAndActions;
        }

        private static void InitTreeViewOf(ref ATFStorageTreeView view, ref SearchField field, ref TreeViewState state, TreePurpose purpose, IATFRecorder recorder, IATFActionStorage storage)
        {
            if (view != null && field != null) return;
            if (state == null)
            {
                state = new TreeViewState();
            }
            view = new ATFStorageTreeView(purpose, state, recorder, storage);
            field = new SearchField();
            field.downOrUpArrowKeyPressed += view.SetFocusAndEnsureSelectedItem;
        }

        private static void DoToolbarFor(TreeView view, SearchField field)
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Space(100);
            GUILayout.FlexibleSpace();
            view.searchString = field.OnToolbarGUI(view.searchString);
            GUILayout.EndHorizontal();
        }
        
        private static void DoTreeViewFor(ATFStorageTreeView view, IATFActionStorage storage)
        {
            var rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
            view.OnGUI(rect);
            switch (view.TreePurpose)
            {
                case TreePurpose.DRAW_CURRENT_NAMES:
                    view.UpdateItems(storage.GetCurrentRecordNames());
                    break;
                
                case TreePurpose.DRAW_SAVED_NAMES:
                    view.UpdateItems(storage.GetSavedRecordNames());
                    break;
                
                case TreePurpose.DRAW_KINDS_AND_ACTIONS:
                    break;
                
                case TreePurpose.NONE:
                    throw new ArgumentOutOfRangeException(string.Empty, "Tree purpose is NONE!");
                default:
                    throw new ArgumentOutOfRangeException();
            }
            view.Reload();
            
        }

        private void OnGUI()
        {
            var stateLoaded = storage != null;
            if (EditorApplication.isPlaying)
            {
                GUILayout.Label("Storage Settings", EditorStyles.boldLabel);
                GUILayout.Label(
                    stateLoaded
                        ? $"Storage realisation: {storage.GetType().Name}"
                        : "Storage realisation: Waiting to focus...", EditorStyles.label);
                if (!stateLoaded) return;
                
                GUILayout.Label("Save/Load control", EditorStyles.boldLabel);
                GUILayout.Label($"Current recording name: {recorder.GetCurrentRecordingName()}", EditorStyles.label);
                if (!recorder.IsPlaying() && !recorder.IsRecording())
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Save"))
                    {
                        storage.SaveStorage(recorder.GetCurrentRecordingName());
                    }
                    if (GUILayout.Button("Load"))
                    {
                        storage.LoadStorage(recorder.GetCurrentRecordingName());
                    }
                    if (GUILayout.Button("Scrap"))
                    {
                        storage.ScrapSavedStorage(recorder.GetCurrentRecordingName());
                    }
                    EditorGUILayout.EndHorizontal();
                }

                GUILayout.Label("Current records", EditorStyles.boldLabel);
                DoToolbarFor(TreeViewForCurrentNames, SearchFieldForCurrentNames);
                DoTreeViewFor(TreeViewForCurrentNames, storage);
                
                GUILayout.Label("Commands and actions queue", EditorStyles.boldLabel);
                DoToolbarFor(TreeViewForCurrentKindsAndActions, SearchFieldForCurrentKindsAndActions);
                DoTreeViewFor(TreeViewForCurrentKindsAndActions, storage);
                
                GUILayout.Label("Saved records", EditorStyles.boldLabel);
                DoToolbarFor(TreeViewForSavedNames, SearchFieldForSavedNames);
                DoTreeViewFor(TreeViewForSavedNames, storage);
                
            }
            else
            {
                GUILayout.Label("Waiting for Play Mode...", EditorStyles.boldLabel);
            }
        }
    }
}