using System;
using System.Collections;
using System.Collections.Generic;
using ATF.Scripts.Recorder;
using ATF.Scripts.Storage;
using ATF.Scripts.Storage.Interfaces;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Serialization;

namespace ATF.Scripts.Editor
{
    public class AtfStorageWindow : EditorWindow
    {
        [SerializeField]
        private TreeViewState treeViewStateForCurrentNames;
        
        [SerializeField]
        private TreeViewState treeViewStateForCurrentKindsAndActions;
        
        [SerializeField]
        private TreeViewState treeViewStateForSavedNames;
        
        [SerializeField]
        private TreeViewState treeViewStateForSavedKindsAndActions;
        
        private AtfStorageTreeView _treeViewForCurrentNames;
        private SearchField _searchFieldForCurrentNames;

        private AtfStorageTreeView _treeViewForCurrentKindsAndActions;
        private SearchField _searchFieldForCurrentKindsAndActions;
        
        private AtfStorageTreeView _treeViewForSavedNames;
        private SearchField _searchFieldForSavedNames;
        
        private AtfStorageTreeView _treeViewForSavedKindsAndActions;
        private SearchField _searchFieldForSavedKindsAndActions;
        
        public IAtfActionStorage storage;
        public IAtfRecorder recorder;

        private bool _showDetailsOfSavedRecord;
        private bool _showDetailsOfCurrentRecord = true;

        private void OnFocus()
        {
            if (!EditorApplication.isPlaying) return;
            storage = FindObjectOfType<AtfDictionaryBasedActionStorage>();
            recorder = FindObjectOfType<AtfQueueBasedRecorder>();
            InitTreeViewOf(ref _treeViewForCurrentNames, ref _searchFieldForCurrentNames, ref treeViewStateForCurrentNames, TreePurpose.DRAW_CURRENT_NAMES, recorder, storage);
            InitTreeViewOf(ref _treeViewForCurrentKindsAndActions, ref _searchFieldForCurrentKindsAndActions, 
                ref treeViewStateForCurrentKindsAndActions, TreePurpose.DRAW_CURRENT_KINDS_AND_ACTIONS, recorder, storage);
            InitTreeViewOf(ref _treeViewForSavedNames, ref _searchFieldForSavedNames, 
                ref treeViewStateForSavedNames, TreePurpose.DRAW_SAVED_NAMES, recorder, storage);
            InitTreeViewOf(ref _treeViewForSavedKindsAndActions, ref _searchFieldForSavedKindsAndActions, 
                ref treeViewStateForSavedKindsAndActions, TreePurpose.DRAW_SAVED_KINDS_AND_ACTIONS, recorder, storage);
            _treeViewForCurrentNames.KindsAndActionsTreeView = _treeViewForCurrentKindsAndActions;
            _treeViewForCurrentNames.RecordNameChanged += RepaintRecorderWindow;
            _treeViewForSavedNames.KindsAndActionsTreeView = _treeViewForSavedKindsAndActions;
        }

        private static void RepaintRecorderWindow(string recordName, AtfStorageTreeView context)
        {
            AtfWindow.GetRecorderWindow().Repaint();
        }

        private static void InitTreeViewOf(ref AtfStorageTreeView view, ref SearchField field, ref TreeViewState state, TreePurpose purpose, IAtfRecorder recorder, IAtfActionStorage storage)
        {
            if (view != null && field != null) return;
            if (state == null)
            {
                state = new TreeViewState();
            }
            view = new AtfStorageTreeView(purpose, state, recorder, storage);
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
        
        private static void DoTreeViewFor(AtfStorageTreeView view)
        {
            var rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
            view.OnGUI(rect);
            switch (view.TreePurpose)
            {
                case TreePurpose.DRAW_CURRENT_NAMES:
                    view.UpdateItems(view.Storage.GetCurrentRecordNames());
                    break;
                
                case TreePurpose.DRAW_SAVED_NAMES:
                    view.UpdateItems(view.Storage.GetSavedRecordNames());
                    break;
                
                case TreePurpose.DRAW_SAVED_KINDS_AND_ACTIONS:
                case TreePurpose.DRAW_CURRENT_KINDS_AND_ACTIONS:
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
                
                GUILayout.Label($"Current recording name: {storage.GetCurrentRecordName()}", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                _showDetailsOfSavedRecord = EditorGUILayout.Toggle("Display saved details", _showDetailsOfSavedRecord);
                _showDetailsOfCurrentRecord = EditorGUILayout.Toggle("Display current details", _showDetailsOfCurrentRecord);
                EditorGUILayout.EndHorizontal();
                
                if (!recorder.IsPlaying() && !recorder.IsRecording())
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Save"))
                    {
                        storage.SaveStorage();
                    }
                    if (GUILayout.Button("Load"))
                    {
                        storage.LoadStorage();
                    }
                    if (GUILayout.Button("Scrap"))
                    {
                        storage.ScrapSavedStorage();
                    }
                    EditorGUILayout.EndHorizontal();
                }

                GUILayout.Label("Current records", EditorStyles.boldLabel);
                DoToolbarFor(_treeViewForCurrentNames, _searchFieldForCurrentNames);
                DoTreeViewFor(_treeViewForCurrentNames);

                if (_showDetailsOfCurrentRecord)
                {
                    GUILayout.Label("Current commands and actions queues", EditorStyles.boldLabel);
                    DoToolbarFor(_treeViewForCurrentKindsAndActions, _searchFieldForCurrentKindsAndActions);
                    DoTreeViewFor(_treeViewForCurrentKindsAndActions);
                }
                
                GUILayout.Label("Saved records", EditorStyles.boldLabel);
                DoToolbarFor(_treeViewForSavedNames, _searchFieldForSavedNames);
                DoTreeViewFor(_treeViewForSavedNames);

                if (!_showDetailsOfSavedRecord) return;
                GUILayout.Label("Saved commands and actions queues", EditorStyles.boldLabel);
                DoToolbarFor(_treeViewForSavedKindsAndActions, _searchFieldForSavedKindsAndActions);
                DoTreeViewFor(_treeViewForSavedKindsAndActions);
                
            }
            else
            {
                GUILayout.Label("Waiting for Play Mode...", EditorStyles.boldLabel);
            }
        }
    }
}