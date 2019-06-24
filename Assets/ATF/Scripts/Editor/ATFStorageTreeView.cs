using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using ATF.Scripts.Storage.Interfaces;

namespace ATF.Scripts.Editor
{
    public enum TreePurpose
    {
        NONE, TO_DRAW_SAVED, TO_DRAW_CURRENT
    }

    public class ATFStorageTreeView : TreeView
    {
        private readonly TreePurpose TreePurpose;

        private List<TreeViewItem> AllItems;

        private TreeViewItem Root;

        public ATFStorageTreeView(TreePurpose treePurpose, TreeViewState treeViewState)
            : base(treeViewState)
        {
            TreePurpose = treePurpose;
            Root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            if (AllItems == null)
            {
                AllItems = new List<TreeViewItem>();
                switch (TreePurpose)
                {
                    case TreePurpose.TO_DRAW_CURRENT:
                        AllItems.Add(new TreeViewItem() {
                            depth = 0,
                            displayName = "There is no current actions."
                        });
                        break;

                    case TreePurpose.TO_DRAW_SAVED:
                        AllItems.Add(new TreeViewItem() {
                            depth = 0,
                            displayName = "There is no saved actions."
                        });
                        break;

                    case TreePurpose.NONE:
                        throw new System.ArgumentOutOfRangeException();
                    
                    default:
                        throw new System.ArgumentOutOfRangeException();
                }
            }
            SetupParentsAndChildrenFromDepths(Root, AllItems);
            return Root;
        }

        public void SetItems(List<TreeViewItem> items)
        {
            AllItems = items;
            SetupParentsAndChildrenFromDepths(Root, AllItems);
        }
    }
}
