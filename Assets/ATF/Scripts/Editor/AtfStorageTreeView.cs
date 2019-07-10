using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using ATF.Scripts.Storage.Interfaces;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using ATF.Scripts.Recorder;
using ATF.Scripts.Storage;
using Bedrin.Helper;

namespace ATF.Scripts.Editor
{
    public enum TreePurpose
    {
        NONE, DRAW_CURRENT_KINDS_AND_ACTIONS, DRAW_CURRENT_NAMES, DRAW_SAVED_NAMES, DRAW_SAVED_KINDS_AND_ACTIONS
    }

    public class AtfStorageTreeView : TreeView
    {
        public readonly TreePurpose TreePurpose;
        public AtfStorageTreeView KindsAndActionsTreeView;
        
        private List<TreeViewItem> AllItems;
        private readonly TreeViewItem Root;
        
        private const string NoCurrentKindsAndActionsSelected = "No current record selected.";
        private const string NoSavedKindsAndActionsSelected = "No saved record selected.";
        private const string NoCurrentActionsLoaded = "No current actions loaded.";
        private const string NoRecordsSaved = "No records saved.";

        // ReSharper disable once MemberCanBePrivate.Global
        public readonly IAtfRecorder Recorder;
        public readonly IAtfActionStorage Storage;

        public delegate void RecordNameUsedHandler(string name, AtfStorageTreeView context);
        public event RecordNameUsedHandler RecordNameChanged;
        public event RecordNameUsedHandler RecordNameUsedInLoad;
        
        public AtfStorageTreeView(TreePurpose treePurpose, TreeViewState treeViewState, IAtfRecorder recorder, IAtfActionStorage storage)
            : base(treeViewState)
        {
            TreePurpose = treePurpose;
            Recorder = recorder;
            Storage = storage;
            Root = new TreeViewItem {id = 0, depth = -1, displayName = "Root"};
            InitializeAllItems();
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            SetupParentsAndChildrenFromDepths(Root, AllItems);
            return Root;
        }

        private void InitializeAllItems()
        {
            AllItems = new List<TreeViewItem>();
            switch (TreePurpose)
            {
                case TreePurpose.DRAW_CURRENT_NAMES:
                    AllItems.Add(new TreeViewItem {
                        id = DictionaryBasedIdGenerator.GetNewId(NoCurrentActionsLoaded),
                        depth = 0,
                        displayName = NoCurrentActionsLoaded
                    });
                    break;

                case TreePurpose.DRAW_CURRENT_KINDS_AND_ACTIONS:
                    AllItems.Add(new TreeViewItem {
                        id = DictionaryBasedIdGenerator.GetNewId(NoCurrentKindsAndActionsSelected),
                        depth = 0,
                        displayName = NoCurrentKindsAndActionsSelected
                    });
                    break;

                case TreePurpose.DRAW_SAVED_NAMES:
                    AllItems.Add(new TreeViewItem {
                        id = DictionaryBasedIdGenerator.GetNewId(NoRecordsSaved),
                        depth = 0,
                        displayName = NoRecordsSaved
                    });
                    break;

                case TreePurpose.DRAW_SAVED_KINDS_AND_ACTIONS:
                    AllItems.Add(new TreeViewItem {
                        id = DictionaryBasedIdGenerator.GetNewId(NoSavedKindsAndActionsSelected),
                        depth = 0,
                        displayName = NoSavedKindsAndActionsSelected
                    });
                    break;
                
                case TreePurpose.NONE:
                    throw new System.ArgumentOutOfRangeException(string.Empty, "Tree purpose is NONE!");

                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }

        public void UpdateItems(List<TreeViewItem> items)
        {
            if (items == null) return;
            AllItems.RemoveAll(item => item.displayName.Equals(NoCurrentActionsLoaded) 
                                       || item.displayName.Equals(NoCurrentKindsAndActionsSelected)
                                       || item.displayName.Equals(NoRecordsSaved));
            AllItems = items;
            SetupDepthsFromParentsAndChildren(AllItems[0]);
            Reload();
        }

        protected override void DoubleClickedItem(int id)
        {
            base.DoubleClickedItem(id);
            var clickedItem = FindItem(id, Root);
            if (clickedItem == null || clickedItem.depth > 0
                                    || clickedItem.displayName.Equals(NoCurrentKindsAndActionsSelected) 
                                    || clickedItem.displayName.Equals(NoCurrentActionsLoaded)
                                    || clickedItem.displayName.Equals(NoRecordsSaved)) return;
            switch (TreePurpose)
            {
                case TreePurpose.DRAW_CURRENT_NAMES:
                    KindsAndActionsTreeView?.UpdateItems(Storage.GetCurrentActions(clickedItem.displayName));
                    break;
                
                case TreePurpose.DRAW_SAVED_NAMES:
                    KindsAndActionsTreeView?.UpdateItems(Storage.GetSavedActions(clickedItem.displayName));
                    break;
                
                case TreePurpose.DRAW_SAVED_KINDS_AND_ACTIONS:
                case TreePurpose.DRAW_CURRENT_KINDS_AND_ACTIONS:
                    break;
                
                case TreePurpose.NONE:
                    throw new System.ArgumentOutOfRangeException(string.Empty, "Tree purpose is NONE!");

                default:
                    throw new System.ArgumentOutOfRangeException();
                    
            }
        }

        protected override void ContextClickedItem(int id)
        {
            base.ContextClickedItem(id);
            var clickedItem = FindItem(id, Root);
            if (clickedItem == null || clickedItem.depth > 0
                                    || clickedItem.displayName.Equals(NoCurrentKindsAndActionsSelected) 
                                    || clickedItem.displayName.Equals(NoCurrentActionsLoaded)
                                    || clickedItem.displayName.Equals(NoRecordsSaved)) return;
            switch (TreePurpose)
            {
                
                case TreePurpose.DRAW_CURRENT_NAMES:
                    RecordNameChanged?.Invoke(clickedItem.displayName, this);
                    Recorder.SetCurrentRecordName(clickedItem.displayName);
                    break;
                
                case TreePurpose.DRAW_SAVED_NAMES:
                    RecordNameUsedInLoad?.Invoke(clickedItem.displayName, this);
                    Storage.SetCurrentRecordName(clickedItem.displayName);
                    break;

                case TreePurpose.DRAW_SAVED_KINDS_AND_ACTIONS:
                case TreePurpose.DRAW_CURRENT_KINDS_AND_ACTIONS:
                    break;

                case TreePurpose.NONE:
                    throw new System.ArgumentOutOfRangeException(string.Empty, "Tree purpose is NONE!");

                default:
                    throw new System.ArgumentOutOfRangeException();
                    
            }
        }
    }
}
