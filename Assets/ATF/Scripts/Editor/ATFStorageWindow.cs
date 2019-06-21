using System;
using System.Collections.Generic;
using ATF.Scripts.Editor.StorageTreeView;
using ATF.Scripts.Editor.StorageTreeView.TreeDataModel;
using ATF.Scripts.Editor.StorageTreeView.TreeDataModel.Asset;
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

        [NonSerialized] private bool MInitialized;
        
        [SerializeField]
        private TreeViewState mTreeViewState; // Serialized in the window layout file so it survives assembly reloading
        
        [SerializeField] private MultiColumnHeaderState mMultiColumnHeaderState;
        private SearchField MSearchField;
        private ATFStorageActionTreeAsset MMyTreeAsset;

        public IATFActionStorage storage;

        private ATFStorageTreeView TreeView { get; set; }

        private void OnFocus()
        {
            if (EditorApplication.isPlaying)
            {
                storage = FindObjectOfType<ATFDictionaryBasedActionStorage>();
            }
        }

        private void OnGUI()
        {
            var storageLoaded = storage != null;
            if (EditorApplication.isPlaying)
            {
                GUILayout.Label("Storage Settings", EditorStyles.boldLabel);
                GUILayout.Label(
                    storageLoaded
                        ? $"Storage current realisation: {storage.GetType().Name}"
                        : "Storage current realisation: Waiting to focus...", EditorStyles.label);

                if (storageLoaded)
                {
                    InitIfNeeded();
                    SearchBar(ToolbarRect);
                    DoTreeView(MultiColumnTreeViewRect);
                    BottomToolBar(BottomToolbarRect);
                } else
                {
                    GUILayout.Label("Adding ", EditorStyles.label);
                }

            }
            else
            {
                GUILayout.Label("Waiting for Play Mode...", EditorStyles.boldLabel);
            }
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            var myTreeAsset = EditorUtility.InstanceIDToObject(instanceId) as ATFStorageActionTreeAsset;
            if (myTreeAsset == null) return false; // we did not handle the open
            var window = GetWindow<ATFStorageWindow>();
            window.SetTreeAsset(myTreeAsset);
            return true;
        }

        private void SetTreeAsset(ATFStorageActionTreeAsset myTreeAsset)
        {
            MMyTreeAsset = myTreeAsset;
            MInitialized = false;
        }

        private Rect MultiColumnTreeViewRect => new Rect(20, 30, position.width - 40, position.height - 60);

        private Rect ToolbarRect => new Rect(20f, 10f, position.width - 40f, 20f);

        private Rect BottomToolbarRect => new Rect(20f, position.height - 18f, position.width - 40f, 16f);

        private void InitIfNeeded()
        {
            if (MInitialized) return;
            // Check if it already exists (deserialized from window layout file or scriptable object)
            if (mTreeViewState == null)
                mTreeViewState = new TreeViewState();

            var firstInit = mMultiColumnHeaderState == null;
            var headerState = ATFStorageTreeView.CreateDefaultMultiColumnHeaderState(MultiColumnTreeViewRect.width);
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(mMultiColumnHeaderState, headerState))
                MultiColumnHeaderState.OverwriteSerializedFields(mMultiColumnHeaderState, headerState);
            mMultiColumnHeaderState = headerState;

            var multiColumnHeader = new AtfMultiColumnHeader(headerState);
            if (firstInit)
                multiColumnHeader.ResizeToFit();

            var treeModel = new TreeModel<ATFStorageTreeElement>(GetData());

            TreeView = new ATFStorageTreeView(mTreeViewState, multiColumnHeader, treeModel);

            MSearchField = new SearchField();
            MSearchField.downOrUpArrowKeyPressed += TreeView.SetFocusAndEnsureSelectedItem;

            MInitialized = true;
        }

        private IList<ATFStorageTreeElement> GetData()
        {
            if (MMyTreeAsset != null && MMyTreeAsset.TreeElements != null && MMyTreeAsset.TreeElements.Count > 0)
            {
                return MMyTreeAsset.TreeElements;
            }
            else
            {
                MMyTreeAsset = CreateInstance<ATFStorageActionTreeAsset>();
                MMyTreeAsset.TreeElements = new List<ATFStorageTreeElement>();
                //adding some elements
                
                return MMyTreeAsset.TreeElements;
            }
        }

        private void OnSelectionChange()
        {
            if (!MInitialized)
                return;

            var myTreeAsset = Selection.activeObject as ATFStorageActionTreeAsset;
            if (myTreeAsset == null || myTreeAsset == MMyTreeAsset) return;
            
            MMyTreeAsset = myTreeAsset;
            TreeView.TreeModel.SetData(GetData());
            TreeView.Reload();
        }

        private void SearchBar(Rect rect)
        {
            TreeView.searchString = MSearchField.OnGUI(rect, TreeView.searchString);
        }

        private void DoTreeView(Rect rect)
        {
            TreeView.OnGUI(rect);
        }

        private void BottomToolBar(Rect rect)
        {
            GUILayout.BeginArea(rect);

            using (new EditorGUILayout.HorizontalScope())
            {

                const string style = "miniButton";
                if (GUILayout.Button("Expand All", style))
                {
                    TreeView.ExpandAll();
                }

                if (GUILayout.Button("Collapse All", style))
                {
                    TreeView.CollapseAll();
                }

                GUILayout.FlexibleSpace();

                GUILayout.Label(MMyTreeAsset != null ? AssetDatabase.GetAssetPath(MMyTreeAsset) : string.Empty);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Set sorting", style))
                {
                    var myColumnHeader = (AtfMultiColumnHeader)TreeView.multiColumnHeader;
                    myColumnHeader.SetSortingColumns(new int[] { 4, 3, 2 }, new[] { true, false, true });
                    myColumnHeader.mode = AtfMultiColumnHeader.Mode.LargeHeader;
                }


                GUILayout.Label("Header: ", "minilabel");
                if (GUILayout.Button("Large", style))
                {
                    var myColumnHeader = (AtfMultiColumnHeader)TreeView.multiColumnHeader;
                    myColumnHeader.mode = AtfMultiColumnHeader.Mode.LargeHeader;
                }
                if (GUILayout.Button("Default", style))
                {
                    var myColumnHeader = (AtfMultiColumnHeader)TreeView.multiColumnHeader;
                    myColumnHeader.mode = AtfMultiColumnHeader.Mode.DefaultHeader;
                }
                if (GUILayout.Button("No sort", style))
                {
                    var myColumnHeader = (AtfMultiColumnHeader)TreeView.multiColumnHeader;
                    myColumnHeader.mode = AtfMultiColumnHeader.Mode.MinimumHeaderWithoutSorting;
                }

                GUILayout.Space(10);

                if (GUILayout.Button("values <-> controls", style))
                {
                    TreeView.ShowControls = !TreeView.ShowControls;
                }
            }

            GUILayout.EndArea();
        }
    }

    internal class AtfMultiColumnHeader : MultiColumnHeader
    {
        private Mode MMode;

        public enum Mode
        {
            LargeHeader,
            DefaultHeader,
            MinimumHeaderWithoutSorting
        }

        public AtfMultiColumnHeader(MultiColumnHeaderState state)
            : base(state)
        {
            mode = Mode.DefaultHeader;
        }

        public Mode mode
        {
            private get
            {
                return MMode;
            }
            set
            {
                MMode = value;
                switch (MMode)
                {
                    case Mode.LargeHeader:
                        canSort = true;
                        height = 37f;
                        break;
                    case Mode.DefaultHeader:
                        canSort = true;
                        height = DefaultGUI.defaultHeight;
                        break;
                    case Mode.MinimumHeaderWithoutSorting:
                        canSort = false;
                        height = DefaultGUI.minimumHeight;
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        protected override void ColumnHeaderGUI(MultiColumnHeaderState.Column column, Rect headerRect, int columnIndex)
        {
            // Default column header gui
            base.ColumnHeaderGUI(column, headerRect, columnIndex);

            // Add additional info for large header
            if (mode == Mode.LargeHeader)
            {
                // Show example overlay stuff on some of the columns
                if (columnIndex > 2)
                {
                    headerRect.xMax -= 3f;
                    var oldAlignment = EditorStyles.largeLabel.alignment;
                    EditorStyles.largeLabel.alignment = TextAnchor.UpperRight;
                    GUI.Label(headerRect, 36 + columnIndex + "%", EditorStyles.largeLabel);
                    EditorStyles.largeLabel.alignment = oldAlignment;
                }
            }
        }
    }
}