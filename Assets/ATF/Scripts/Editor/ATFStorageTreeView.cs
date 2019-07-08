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

namespace ATF.Scripts.Editor
{
    public enum TreePurpose
    {
        NONE, DRAW_KINDS_AND_ACTIONS, DRAW_CURRENT_NAMES, DRAW_SAVED_NAMES
    }

    public class ATFStorageTreeView : TreeView
    {
        public readonly TreePurpose TreePurpose;
        public ATFStorageTreeView KindsAndActionsTreeView;
        
        private List<TreeViewItem> AllItems;
        private readonly TreeViewItem Root;
        
        private const string NoKindsAndActionsSelected = "No record selected.";
        private const string NoCurrentActionsLoaded = "No current actions loaded.";
        private const string NoRecordsSaved = "No records saved.";

        private readonly IATFRecorder Recorder;
        private readonly IATFActionStorage Storage;
        
        public ATFStorageTreeView(TreePurpose treePurpose, TreeViewState treeViewState, IATFRecorder recorder, IATFActionStorage storage)
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
                        id = ATFIdHelper.GetNewId(NoCurrentActionsLoaded),
                        depth = 0,
                        displayName = NoCurrentActionsLoaded
                    });
                    break;

                case TreePurpose.DRAW_KINDS_AND_ACTIONS:
                    AllItems.Add(new TreeViewItem {
                        id = ATFIdHelper.GetNewId(NoKindsAndActionsSelected),
                        depth = 0,
                        displayName = NoKindsAndActionsSelected
                    });
                    break;

                case TreePurpose.DRAW_SAVED_NAMES:
                    AllItems.Add(new TreeViewItem {
                        id = ATFIdHelper.GetNewId(NoRecordsSaved),
                        depth = 0,
                        displayName = NoRecordsSaved
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
                                       || item.displayName.Equals(NoKindsAndActionsSelected)
                                       || item.displayName.Equals(NoRecordsSaved));
            AllItems = items;
            Reload();
        }

        protected override void DoubleClickedItem(int id)
        {
            base.DoubleClickedItem(id);
            var clickedItem = FindItem(id, Root);
            if (clickedItem == null || clickedItem.depth > 0
                                    || clickedItem.displayName.Equals(NoKindsAndActionsSelected) 
                                    || clickedItem.displayName.Equals(NoCurrentActionsLoaded)
                                    || clickedItem.displayName.Equals(NoRecordsSaved)) return;
            switch (TreePurpose)
            {
                case TreePurpose.DRAW_CURRENT_NAMES:
                    KindsAndActionsTreeView?.UpdateItems(Storage.GetCurrentActions(clickedItem.displayName));
                    Debug.Log($"Loading current kinds and actions {id}");
                    break;
                
                case TreePurpose.DRAW_KINDS_AND_ACTIONS:
                case TreePurpose.DRAW_SAVED_NAMES:
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
                                    || clickedItem.displayName.Equals(NoKindsAndActionsSelected) 
                                    || clickedItem.displayName.Equals(NoCurrentActionsLoaded)
                                    || clickedItem.displayName.Equals(NoRecordsSaved)) return;
            switch (TreePurpose)
            {
                
                case TreePurpose.DRAW_CURRENT_NAMES:
                    Recorder?.SetCurrentRecordingName(clickedItem.displayName);
                    Debug.Log($"Loading to recorder {id}");
                    break;

                case TreePurpose.DRAW_KINDS_AND_ACTIONS:
                    break;
                
                case TreePurpose.DRAW_SAVED_NAMES:
                    Debug.Log($"Loading saved names {id}");
                    break;
                
                case TreePurpose.NONE:
                    throw new System.ArgumentOutOfRangeException(string.Empty, "Tree purpose is NONE!");
                
                default:
                    throw new System.ArgumentOutOfRangeException();
                    
            }
        }
    }
}
