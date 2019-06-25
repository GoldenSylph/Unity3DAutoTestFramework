using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using ATF.Scripts.Storage.Interfaces;
using System.Linq;

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
        private readonly TreeViewItem Root;
        
        public ATFStorageTreeView(TreePurpose treePurpose, TreeViewState treeViewState)
            : base(treeViewState)
        {
            TreePurpose = treePurpose;
            Root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
            Reload();
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override TreeViewItem BuildRoot()
        {
            return Root;
        }

        public void ClearAllItems()
        {
            AllItems?.Clear();
        }
        
        public List<TreeViewItem> UpdateParentItems(List<TreeViewItem> items)
        {
            if (AllItems == null)
            {
                AllItems = new List<TreeViewItem>();
                switch (TreePurpose)
                {
                    case TreePurpose.TO_DRAW_CURRENT:
                        AllItems.Add(new TreeViewItem {
                            depth = 0,
                            displayName = "There is no current actions."
                        });
                        break;

                    case TreePurpose.TO_DRAW_SAVED:
                        AllItems.Add(new TreeViewItem {
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
            else
            {
                if (items == null || AllItems.Intersect(items).Count() == AllItems.Count) return AllItems;
                items.ForEach(item => item.children = CreateChildListForCollapsedParent());
                AllItems.AddRange(items);
            }
            return AllItems;
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            return UpdateParentItems(null);
        }

        protected override void DoubleClickedItem(int id)
        {
            Debug.Log($"double clicked item {id}");
            base.DoubleClickedItem(id);
        }

    }
}
