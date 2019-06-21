using System;
using System.Collections.Generic;
using System.Linq;
using ATF.Scripts.Editor.StorageTreeView.TreeDataModel;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ATF.Scripts.Editor.StorageTreeView
{

	public class TreeViewItem<T> : TreeViewItem where T : TreeElement
	{
		public T data { get; set; }

		public TreeViewItem (int id, int depth, string displayName, T data) : base (id, depth, displayName)
		{
			this.data = data;
		}
	}

	public class TreeViewWithTreeModel<T> : TreeView where T : TreeElement
	{
        readonly List<TreeViewItem> m_Rows = new List<TreeViewItem>(100);
		public event System.Action TreeChanged;

        public TreeModel<T> TreeModel { get; private set; }
        public event Action<IList<TreeViewItem>>  BeforeDroppingDraggedItems;


		public TreeViewWithTreeModel (TreeViewState state, TreeModel<T> model) : base (state)
		{
			Init (model);
		}

		public TreeViewWithTreeModel (TreeViewState state, MultiColumnHeader multiColumnHeader, TreeModel<T> model)
			: base(state, multiColumnHeader)
		{
			Init (model);
		}

		void Init (TreeModel<T> model)
		{
			TreeModel = model;
			TreeModel.ModelChanged += ModelChanged;
		}

		void ModelChanged()
		{
            TreeChanged?.Invoke();
            Reload();
		}

		protected override TreeViewItem BuildRoot()
		{
			int depthForHiddenRoot = -1;
			return new TreeViewItem<T>(TreeModel.Root.Id, depthForHiddenRoot, TreeModel.Root.Name, TreeModel.Root);
		}

		protected override IList<TreeViewItem> BuildRows (TreeViewItem root)
		{
			if (TreeModel.Root == null)
			{
				Debug.LogError ("tree model root is null. did you call SetData()?");
			}

			m_Rows.Clear ();
			if (!string.IsNullOrEmpty(searchString))
			{
				Search (TreeModel.Root, searchString, m_Rows);
			}
			else
			{
				if (TreeModel.Root.HasChildren)
					AddChildrenRecursive(TreeModel.Root, 0, m_Rows);
			}

			// We still need to setup the child parent information for the rows since this 
			// information is used by the TreeView internal logic (navigation, dragging etc)
			SetupParentsAndChildrenFromDepths (root, m_Rows);

			return m_Rows;
		}

		void AddChildrenRecursive (T parent, int depth, IList<TreeViewItem> newRows)
		{
			foreach (T child in parent.Children)
			{
				var item = new TreeViewItem<T>(child.Id, depth, child.Name, child);
				newRows.Add(item);

				if (child.HasChildren)
				{
					if (IsExpanded(child.Id))
					{
						AddChildrenRecursive (child, depth + 1, newRows);
					}
					else
					{
						item.children = CreateChildListForCollapsedParent();
					}
				}
			}
		}

		void Search(T searchFromThis, string search, List<TreeViewItem> result)
		{
			if (string.IsNullOrEmpty(search))
				throw new ArgumentException("Invalid search: cannot be null or empty", "search");

			const int kItemDepth = 0; // tree is flattened when searching

			Stack<T> stack = new Stack<T>();
			foreach (var element in searchFromThis.Children)
				stack.Push((T)element);
			while (stack.Count > 0)
			{
				T current = stack.Pop();
				// Matches search?
				if (current.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					result.Add(new TreeViewItem<T>(current.Id, kItemDepth, current.Name, current));
				}

				if (current.Children != null && current.Children.Count > 0)
				{
					foreach (var element in current.Children)
					{
						stack.Push((T)element);
					}
				}
			}
			SortSearchResult(result);
		}

		protected virtual void SortSearchResult (List<TreeViewItem> rows)
		{
			rows.Sort ((x,y) => EditorUtility.NaturalCompare (x.displayName, y.displayName)); // sort by displayName by default, can be overriden for multicolumn solutions
		}
	
		protected override IList<int> GetAncestors (int id)
		{
			return TreeModel.GetAncestors(id);
		}

		protected override IList<int> GetDescendantsThatHaveChildren (int id)
		{
			return TreeModel.GetDescendantsThatHaveChildren(id);
		}


		// Dragging
		//-----------
	
		const string k_GenericDragID = "GenericDragColumnDragging";

		protected override bool CanStartDrag (CanStartDragArgs args)
		{
			return true;
		}

		protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
		{
			if (hasSearch)
				return;

			DragAndDrop.PrepareStartDrag();
			var draggedRows = GetRows().Where(item => args.draggedItemIDs.Contains(item.id)).ToList();
			DragAndDrop.SetGenericData(k_GenericDragID, draggedRows);
			DragAndDrop.objectReferences = new UnityEngine.Object[] { }; // this IS required for dragging to work
			string title = draggedRows.Count == 1 ? draggedRows[0].displayName : "< Multiple >";
			DragAndDrop.StartDrag (title);
		}

		protected override DragAndDropVisualMode HandleDragAndDrop (DragAndDropArgs args)
		{
            // Check if we can handle the current drag data (could be dragged in from other areas/windows in the editor)
            if (!(DragAndDrop.GetGenericData(k_GenericDragID) is List<TreeViewItem> draggedRows))
                return DragAndDropVisualMode.None;

            // Parent item is null when dragging outside any tree view items.
            switch (args.dragAndDropPosition)
			{
				case DragAndDropPosition.UponItem:
				case DragAndDropPosition.BetweenItems:
					{
						bool validDrag = ValidDrag(args.parentItem, draggedRows);
						if (args.performDrop && validDrag)
						{
							T parentData = ((TreeViewItem<T>)args.parentItem).data;
							OnDropDraggedElementsAtIndex(draggedRows, parentData, args.insertAtIndex == -1 ? 0 : args.insertAtIndex);
						}
						return validDrag ? DragAndDropVisualMode.Move : DragAndDropVisualMode.None;
					}

				case DragAndDropPosition.OutsideItems:
					{
						if (args.performDrop)
							OnDropDraggedElementsAtIndex(draggedRows, TreeModel.Root, TreeModel.Root.Children.Count);

						return DragAndDropVisualMode.Move;
					}
				default:
					Debug.LogError("Unhandled enum " + args.dragAndDropPosition);
					return DragAndDropVisualMode.None;
			}
		}

		public virtual void OnDropDraggedElementsAtIndex (List<TreeViewItem> draggedRows, T parent, int insertIndex)
		{
            BeforeDroppingDraggedItems?.Invoke(draggedRows);

            var draggedElements = new List<TreeElement> ();
			foreach (var x in draggedRows)
				draggedElements.Add (((TreeViewItem<T>) x).data);
		
			var selectedIDs = draggedElements.Select (x => x.Id).ToArray();
			TreeModel.MoveElements (parent, insertIndex, draggedElements);
			SetSelection(selectedIDs, TreeViewSelectionOptions.RevealAndFrame);
		}


		bool ValidDrag(TreeViewItem parent, List<TreeViewItem> draggedItems)
		{
			TreeViewItem currentParent = parent;
			while (currentParent != null)
			{
				if (draggedItems.Contains(currentParent))
					return false;
				currentParent = currentParent.parent;
			}
			return true;
		}
	
	}

}