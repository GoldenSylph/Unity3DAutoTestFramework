using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using ATF.Scripts.Storage.Interfaces;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using ATF.Scripts.Storage;

namespace ATF.Scripts.Editor
{
    public enum TreePurpose
    {
        NONE, TO_DRAW_SAVED, TO_DRAW_CURRENT
    }

    public class ATFStorageTreeView : TreeView
    {
        public readonly TreePurpose TreePurpose;
        
        private List<TreeViewItem> AllItems;
        private readonly TreeViewItem Root;
        
        private const string DoubleClickToLoad = "Double click to load...";
        private const string NoSavedActions = "There is no saved actions.";
        private const string NoCurrentActions = "There is no current actions.";

        public ATFStorageTreeView(TreePurpose treePurpose, TreeViewState treeViewState)
            : base(treeViewState)
        {
            TreePurpose = treePurpose;
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
                case TreePurpose.TO_DRAW_CURRENT:
                    AllItems.Add(new TreeViewItem {
                        id = ATFIdHelper.GetNewId(NoCurrentActions),
                        depth = 0,
                        displayName = NoCurrentActions
                    });
                    break;

                case TreePurpose.TO_DRAW_SAVED:
                    AllItems.Add(new TreeViewItem {
                        id = ATFIdHelper.GetNewId(NoSavedActions),
                        depth = 0,
                        displayName = NoSavedActions
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
            AllItems.RemoveAll(item => item.displayName.Equals(NoCurrentActions) || item.displayName.Equals(NoSavedActions));
            if (!AllItems.Except(items).Any() && items.TrueForAll(item => item.depth == 0))
            {
                Debug.Log("in first");
                items.ForEach(item => item.children = new List<TreeViewItem>
                {
                    new TreeViewItem
                    {
                        id = ATFIdHelper.GetNewId(null),
                        displayName = DoubleClickToLoad
                    }
                });
                AllItems = items;
            }
            if (items[0].id == -2 && items.Skip(1).All(item => item.depth > 0))
            {
                var parentIdx = AllItems.FindIndex(item => item.displayName.Equals(items[0].displayName));
                AllItems[parentIdx].children = items.Skip(1) as List<TreeViewItem>;
                SetupParentsAndChildrenFromDepths(AllItems[parentIdx], AllItems[parentIdx].children);
            }
            Reload();
        }

        protected override void DoubleClickedItem(int id)
        {
            if (TreePurpose != TreePurpose.TO_DRAW_SAVED) return;
            Debug.Log($"Double clicked {id}");
            var clickedItem = FindItem(id, Root);
            if (clickedItem == null || !clickedItem.displayName.Equals(DoubleClickToLoad) 
                                    || clickedItem.displayName.Equals(NoSavedActions) 
                                    || clickedItem.displayName.Equals(NoCurrentActions)
                                    || clickedItem.depth == 0) return;
            var clickedItemParent = clickedItem.parent;
            clickedItemParent.children.Clear();
            Debug.Log($"Loading for parent ({clickedItemParent})");
            Reload();
        }

        protected override void ContextClickedItem(int id)
        {
            Debug.Log($"Clicked {id}");
            var clickedItem = FindItem(id, Root);
            if (clickedItem == null || clickedItem.depth > 0
                                    || clickedItem.displayName.Equals(DoubleClickToLoad) 
                                    || clickedItem.displayName.Equals(NoSavedActions) 
                                    || clickedItem.displayName.Equals(NoCurrentActions)) return;
            switch (TreePurpose)
            {
                case TreePurpose.TO_DRAW_SAVED:
                    //Load to current
                    Debug.Log($"Loading to current tree view {id}");
                    break;
                    
                case TreePurpose.TO_DRAW_CURRENT:
                    Debug.Log($"Loading to recorder {id}");
                    break;
                
                case TreePurpose.NONE:
                    throw new System.ArgumentOutOfRangeException(string.Empty, "Tree purpose is NONE!");
                default:
                    throw new System.ArgumentOutOfRangeException();
                    
            }
        }
    }
}
