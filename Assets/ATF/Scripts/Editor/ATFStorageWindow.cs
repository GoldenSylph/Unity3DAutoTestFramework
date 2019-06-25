using System;
using System.Collections.Generic;
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
        private TreeViewState treeViewStateForCurrent;
        
        [SerializeField]
        private TreeViewState treeViewStateForSaved;

        private ATFStorageTreeView TreeViewForCurrent;
        private SearchField SearchFieldForCurrent;

        private ATFStorageTreeView TreeViewForSaved;
        private SearchField SearchFieldForSaved;

        
        public IATFActionStorage storage;

        private void OnFocus()
        {
            if (!EditorApplication.isPlaying) return;
            storage = FindObjectOfType<ATFDictionaryBasedActionStorage>();
            InitTreeViewOf(ref TreeViewForCurrent, ref SearchFieldForCurrent, ref treeViewStateForCurrent, TreePurpose.TO_DRAW_CURRENT);
            InitTreeViewOf(ref TreeViewForSaved, ref SearchFieldForSaved, ref treeViewStateForSaved, TreePurpose.TO_DRAW_SAVED);
        }

        private static void InitTreeViewOf(ref ATFStorageTreeView view, ref SearchField field, ref TreeViewState state, TreePurpose purpose)
        {
            if (view != null && field != null) return;
            if (state == null)
            {
                state = new TreeViewState();
            }
            view = new ATFStorageTreeView(purpose, state);
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
        
        private static void DoTreeViewFor(TreeView view)
        {
            var rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
            view.OnGUI(rect);
            
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
                
                DoToolbarFor(TreeViewForCurrent, SearchFieldForCurrent);
                DoTreeViewFor(TreeViewForCurrent);
                DoToolbarFor(TreeViewForSaved, SearchFieldForSaved);
                DoTreeViewFor(TreeViewForSaved);
                
            }
            else
            {
                GUILayout.Label("Waiting for Play Mode...", EditorStyles.boldLabel);
            }
        }
    }
}