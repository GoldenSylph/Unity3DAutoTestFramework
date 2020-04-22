using System;
using ATF.Scripts.Integration;
using ATF.Scripts.Integration.Interfaces;
using ATF.Scripts.Recorder;
using ATF.Scripts.Storage.Interfaces;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ATF.Scripts.Editor
{
    public class AtfWindow : EditorWindow
    {
        [MenuItem("Tools/ATF/Recorder")]
        public static EditorWindow GetRecorderWindow()
        {
            return GetWindow(typeof(AtfRecorderWindow));
        }

        [MenuItem("Tools/ATF/Storage")]
        public static EditorWindow GetStorageWindow()
        {
            return GetWindow(typeof(AtfStorageWindow));
        }

        [MenuItem("Tools/ATF/Integrator")]
        public static EditorWindow GetIntegratorWindow()
        {
            return GetWindow(typeof(AtfIntegratorWindow));
        }
        
        public static void RepaintRecorderWindow(string recordName, AtfStorageTreeView context)
        {
            GetRecorderWindow().Repaint();
        }

        private static void InitSpecificTreeViewOf(ref AtfStorageTreeView view, ref SearchField field,
            ref TreeViewState state, TreePurpose purpose, Func<TreeViewState, TreePurpose, AtfStorageTreeView> treeViewSelector)
        {
            if (view != null && field != null) return;
            if (state == null)
            {
                state = new TreeViewState();
            }
            view = treeViewSelector(state, purpose);
            field = new SearchField();
            field.downOrUpArrowKeyPressed += view.SetFocusAndEnsureSelectedItem;
        }
        
        public static void InitTreeViewOf(ref AtfStorageTreeView view, ref SearchField field, ref TreeViewState state, TreePurpose purpose, IAtfRecorder recorder, IAtfActionStorage storage)
        {
            InitSpecificTreeViewOf(ref view, ref field, ref state, purpose,
                (s, p) => new AtfStorageTreeView(p, s, recorder, storage));
        }
        
        public static void InitTreeViewOf(ref AtfStorageTreeView view, ref SearchField field, ref TreeViewState state, TreePurpose purpose, IAtfIntegrator integrator)
        {
            InitSpecificTreeViewOf(ref view, ref field, ref state, purpose,
                (s, p) => new AtfStorageTreeView(p, s, integrator));
        }
        
        public static void DoToolbarFor(TreeView view, SearchField field)
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Space(100);
            GUILayout.FlexibleSpace();
            view.searchString = field.OnToolbarGUI(view.searchString);
            GUILayout.EndHorizontal();
        }
        
        public static void DoTreeViewFor(AtfStorageTreeView view)
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

                case TreePurpose.PATHS:
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
    }
}
