using System;
using System.Collections.Generic;
using ATF.Scripts.Storage;
using ATF.Scripts.Storage.Interfaces;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ATF.Scripts.Editor
{
    public class ATFStorageWindow : EditorWindow
    {
        [SerializeField]
        private TreeViewState TreeViewState;

        // The TreeView is not serializable it should be reconstructed from the tree data.
        private ATFStorageTreeView TreeView;
        private SearchField SearchField;

        public IATFActionStorage storage;

        private void OnFocus()
        {
            if (EditorApplication.isPlaying)
            {
                storage = FindObjectOfType<ATFDictionaryBasedActionStorage>();
            }
        }

        private void DoToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Space(100);
            GUILayout.FlexibleSpace();
            TreeView.searchString = SearchField.OnToolbarGUI(TreeView.searchString);
            GUILayout.EndHorizontal();
        }

        private void DoTreeView()
        {
            Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
            TreeView.OnGUI(rect);
        }

        private void InitTreeIfNeeded()
        {
            if (TreeView == null || SearchField == null)
            {
                if (TreeViewState == null)
                {
                    TreeViewState = new TreeViewState();
                }
                TreeView = new ATFStorageTreeView(storage, TreePurpose.TO_DRAW_CURRENT, TreeViewState);
                SearchField = new SearchField();
                SearchField.downOrUpArrowKeyPressed += TreeView.SetFocusAndEnsureSelectedItem;
            }
        } 

        private void OnGUI()
        {
            var stateLoaded = storage != null;
            if (EditorApplication.isPlaying)
            {
                GUILayout.Label("Storage Settings", EditorStyles.boldLabel);
                GUILayout.Label(
                    stateLoaded
                        ? $"Storage current realisation: {storage.GetType().Name}"
                        : "Storage current realisation: Waiting to focus...", EditorStyles.label);

                if (stateLoaded)
                {
                    InitTreeIfNeeded();
                    DoToolbar();
                    DoTreeView();
                }
            }
            else
            {
                GUILayout.Label("Waiting for Play Mode...", EditorStyles.boldLabel);
            }
        }
    }
}