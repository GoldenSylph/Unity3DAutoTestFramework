using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using ATF.Scripts.Storage.Interfaces;

namespace ATF.Scripts.Editor
{
    public enum TreePurpose
    {
        TO_DRAW_SAVED, TO_DRAW_CURRENT
    }

    public class ATFStorageTreeView : TreeView
    {
        private readonly TreePurpose TreePurpose;

        private List<TreeViewItem> AllItems;

        public ATFStorageTreeView(IATFActionStorage storage, TreePurpose treePurpose, TreeViewState treeViewState)
            : base(treeViewState)
        {
            Reload();
            TreePurpose = treePurpose;
        }

        protected override TreeViewItem BuildRoot()
        {
            // BuildRoot is called every time Reload is called to ensure that TreeViewItems 
            // are created from data. Here we just create a fixed set of items, in a real world example
            // a data model should be passed into the TreeView and the items created from the model.

            // This section illustrates that IDs should be unique and that the root item is required to 
            // have a depth of -1 and the rest of the items increment from that.
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
            List<TreeViewItem> allItems = null;
            switch (TreePurpose)
            {
                case TreePurpose.TO_DRAW_CURRENT:
                    allItems = Storage.GetElementsToDraw("There is no actions stored at now.");
                    break;

                case TreePurpose.TO_DRAW_SAVED:
                    allItems = Storage.GetElementsToDraw("There is no actions saved at now.");
                    break;

                default:
                    throw new System.ArgumentOutOfRangeException();
            }
            // Utility method that initializes the TreeViewItem.children and -parent for all items.
            SetupParentsAndChildrenFromDepths(root, allItems);
            
            // Return root of the tree
            return root;
        }

        public void SetItems(List<TreeViewItem> items)
        {
            AllItems = items;
        }
    }
}
