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

        private string NameOfTheCurrentRecord;

        
        public IATFActionStorage storage;

        private void OnFocus()
        {
            if (!EditorApplication.isPlaying) return;
            storage = FindObjectOfType<ATFDictionaryBasedActionStorage>();
            NameOfTheCurrentRecord = "DefaultRecord";
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
        
        private static void DoTreeViewFor(ATFStorageTreeView view, IATFActionStorage storage)
        {
            var rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
            view.OnGUI(rect);
            switch (view.TreePurpose)
            {
                case TreePurpose.TO_DRAW_CURRENT:
                    view.UpdateItems(storage.GetCurrentRecordNames());
                    break;
                case TreePurpose.TO_DRAW_SAVED:
                    view.UpdateItems(storage.GetSavedRecordNames());
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
                NameOfTheCurrentRecord = EditorGUILayout.TextField("Record to load:", NameOfTheCurrentRecord);
                GUILayout.Label($"Current recording name: {NameOfTheCurrentRecord}", EditorStyles.label);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Save"))
                {
                    storage.SaveStorage(NameOfTheCurrentRecord);
                }
                if (GUILayout.Button("Load"))
                {
                    storage.LoadStorage(NameOfTheCurrentRecord);
                }
                if (GUILayout.Button("Scrap"))
                {
                    storage.ScrapSavedStorage(NameOfTheCurrentRecord);
                }
                EditorGUILayout.EndHorizontal();
                
                DoToolbarFor(TreeViewForCurrent, SearchFieldForCurrent);
                DoTreeViewFor(TreeViewForCurrent, storage);
                DoToolbarFor(TreeViewForSaved, SearchFieldForSaved);
                DoTreeViewFor(TreeViewForSaved, storage);
                
            }
            else
            {
                GUILayout.Label("Waiting for Play Mode...", EditorStyles.boldLabel);
            }
        }
    }
}