using System;
using System.Collections.Generic;
using System.Linq;
using ATF.Scripts.Editor.StorageTreeView.TreeDataModel;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ATF.Scripts.Editor.StorageTreeView
{
    public class ATFStorageTreeView : TreeViewWithTreeModel<ATFStorageTreeElement>
    {
        private const float KRowHeights = 20f;
        private const float KToggleWidth = 18f;
        public bool ShowControls = true;

        // All columns
        public enum MyColumns
        {
            Name,
            FakeInputKind,
            FakeInputContent,
        }

        public enum SortOption
        {
            Name,
            FakeInputKind,
            FakeInputContent
        }

        // Sort options per column
        private readonly SortOption[] MSortOptions =
        {
            SortOption.Name,
            SortOption.FakeInputKind,
            SortOption.FakeInputContent
        };

        public static void TreeToList(TreeViewItem root, IList<TreeViewItem> result)
        {
            if (root == null)
                throw new NullReferenceException("root");
            if (result == null)
                throw new NullReferenceException("result");

            result.Clear();

            if (root.children == null)
                return;

            var stack = new Stack<TreeViewItem>();
            for (var i = root.children.Count - 1; i >= 0; i--)
                stack.Push(root.children[i]);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                result.Add(current);

                if (!current.hasChildren || current.children[0] == null) continue;
                for (var i = current.children.Count - 1; i >= 0; i--)
                {
                    stack.Push(current.children[i]);
                }
            }
        }

        public ATFStorageTreeView(TreeViewState state, MultiColumnHeader multicolumnHeader, TreeModel<ATFStorageTreeElement> model) : base(state, multicolumnHeader, model)
        {
            Assert.AreEqual(MSortOptions.Length, Enum.GetValues(typeof(MyColumns)).Length, "Ensure number of sort options are in sync with number of MyColumns enum values");

            // Custom setup
            rowHeight = KRowHeights;
            columnIndexForTreeFoldouts = 2;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            customFoldoutYOffset = (KRowHeights - EditorGUIUtility.singleLineHeight) * 0.5f; // center foldout in the row since we also center content. See RowGUI
            extraSpaceBeforeIconAndLabel = KToggleWidth;
            multicolumnHeader.sortingChanged += OnSortingChanged;

            Reload();
        }


        // Note we We only build the visible rows, only the backend has the full tree information. 
        // The treeview only creates info for the row list.
        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            var rows = base.BuildRows(root);
            SortIfNeeded(root, rows);
            return rows;
        }

        private void OnSortingChanged(MultiColumnHeader multiColumnHeader)
        {
            SortIfNeeded(rootItem, GetRows());
        }

        private void SortIfNeeded(TreeViewItem root, IList<TreeViewItem> rows)
        {
            if (rows.Count <= 1)
                return;

            if (multiColumnHeader.sortedColumnIndex == -1)
            {
                return; // No column to sort for (just use the order the data are in)
            }

            // Sort the roots of the existing tree items
            SortByMultipleColumns();
            TreeToList(root, rows);
            Repaint();
        }

        private void SortByMultipleColumns()
        {
            var sortedColumns = multiColumnHeader.state.sortedColumns;

            if (sortedColumns.Length == 0)
                return;

            var myTypes = rootItem.children.Cast<TreeViewItem<ATFStorageTreeElement>>();
            var orderedQuery = InitialOrder(myTypes, sortedColumns);
            for (var i = 1; i < sortedColumns.Length; i++)
            {
                var sortOption = MSortOptions[sortedColumns[i]];
                var ascending = multiColumnHeader.IsSortedAscending(sortedColumns[i]);

                switch (sortOption)
                {
                    case SortOption.Name:
                        orderedQuery = orderedQuery.ThenBy(l => l.data.recordName, ascending);
                        break;
                    case SortOption.FakeInputKind:
                        orderedQuery = orderedQuery.ThenBy(l => l.data.kindOfInput, ascending);
                        break;
                    case SortOption.FakeInputContent:
                        orderedQuery = orderedQuery.ThenBy(l => l.data.inputContent, ascending);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            rootItem.children = orderedQuery.Cast<TreeViewItem>().ToList();
        }

        IOrderedEnumerable<TreeViewItem<ATFStorageTreeElement>> InitialOrder(IEnumerable<TreeViewItem<ATFStorageTreeElement>> myTypes, int[] history)
        {
            var sortOption = MSortOptions[history[0]];
            var ascending = multiColumnHeader.IsSortedAscending(history[0]);
            switch (sortOption)
            {
                case SortOption.Name:
                    return myTypes.Order(l => l.data.recordName, ascending);
                case SortOption.FakeInputKind:
                    return myTypes.Order(l => l.data.kindOfInput, ascending);
                case SortOption.FakeInputContent:
                    return myTypes.Order(l => l.data.inputContent, ascending);

                default:
                    Assert.IsTrue(false, "Unhandled enum");
                    break;
            }

            // default
            return myTypes.Order(l => l.data.Name, ascending);
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (TreeViewItem<ATFStorageTreeElement>)args.item;

            for (var i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, (MyColumns)args.GetColumn(i), ref args);
            }
        }

        private void CellGUI(Rect cellRect, TreeViewItem<ATFStorageTreeElement> item, MyColumns column, ref RowGUIArgs args)
        {
            // Center cell rect vertically (makes it easier to place controls, icons etc in the cells)
            CenterRectUsingSingleLineHeight(ref cellRect);

            switch (column)
            {
                case MyColumns.Name:
                    {
                        // Do toggle
                        var toggleRect = cellRect;
                        toggleRect.x += GetContentIndent(item);
                        toggleRect.width = KToggleWidth;
                        if (toggleRect.xMax < cellRect.xMax)
                            item.data.enabled = EditorGUI.Toggle(toggleRect, item.data.enabled); // hide when outside cell rect

                        // Default icon and label
                        args.rowRect = cellRect;
                        base.RowGUI(args);
                    }
                    break;

                case MyColumns.FakeInputContent:
                case MyColumns.FakeInputKind:
                    {
                        if (ShowControls)
                        {
                            cellRect.xMin += 5f; // When showing controls make some extra spacing

                            switch (column)
                            {
                                case MyColumns.FakeInputKind:
                                    item.data.kindOfInput = (FakeInput) Enum.Parse(typeof(FakeInput), EditorGUI.TextField(cellRect, item.data.kindOfInput.ToString()));
                                    break;
                                case MyColumns.FakeInputContent:
                                    item.data.inputContent = EditorGUI.TextField(cellRect, item.data.inputContent.ToString());//EditorGUI.ObjectField(cellRect, GUIContent.none, item.data.InputContent, typeof(Material), false);
                                    break;
                                case MyColumns.Name:
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(column), column, null);
                            }
                        }
                        else
                        {
                            var value = "Missing";
                            switch (column)
                            {
                                case MyColumns.FakeInputKind:
                                    value = item.data.kindOfInput.ToString();
                                    break;
                                case MyColumns.FakeInputContent:
                                    value = item.data.inputContent.ToString();
                                    break;
                                case MyColumns.Name:
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(column), column, null);
                            }

                            DefaultGUI.LabelRightAligned(cellRect, value, args.selected, args.focused);
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(column), column, null);
            }
        }

        // Rename
        //--------

        protected override bool CanRename(TreeViewItem item)
        {
            // Only allow rename if we can show the rename overlay with a certain width (label might be clipped by other columns)
            var renameRect = GetRenameRect(treeViewRect, 0, item);
            return renameRect.width > 30;
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            // Set the backend name and reload the tree to reflect the new model
            if (!args.acceptedRename) return;
            var element = TreeModel.Find(args.itemID);
            element.Name = args.newName;
            Reload();
        }

        protected override Rect GetRenameRect(Rect rowRect, int row, TreeViewItem item)
        {
            Rect cellRect = GetCellRectForTreeFoldouts(rowRect);
            CenterRectUsingSingleLineHeight(ref cellRect);
            return base.GetRenameRect(cellRect, row, item);
        }

        // Misc
        //--------

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return true;
        }

        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float treeViewWidth)
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Name"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 150,
                    minWidth = 60,
                    autoResize = false,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Kind"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 150,
                    minWidth = 60,
                    autoResize = false,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Content"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 150,
                    minWidth = 60,
                    autoResize = false,
                    allowToggleVisibility = false
                }
            };

            Assert.AreEqual(columns.Length, Enum.GetValues(typeof(MyColumns)).Length, "Number of columns should match number of enum values: You probably forgot to update one of them.");

            var state = new MultiColumnHeaderState(columns);
            return state;
        }
    }

    internal static class ATFExtensionMethods
    {
        public static IOrderedEnumerable<T> Order<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector, bool ascending)
        {
            return ascending ? source.OrderBy(selector) : source.OrderByDescending(selector);
        }

        public static IOrderedEnumerable<T> ThenBy<T, TKey>(this IOrderedEnumerable<T> source, Func<T, TKey> selector, bool ascending)
        {
            return ascending ? source.ThenBy(selector) : source.ThenByDescending(selector);
        }
    }
}
