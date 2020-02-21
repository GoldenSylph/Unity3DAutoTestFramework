using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using ATF.Scripts.Storage.Interfaces;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using ATF.Scripts.Integration;
using ATF.Scripts.Recorder;
using ATF.Scripts.Storage;
using Bedrin.Helper;

namespace ATF.Scripts.Editor
{
    public enum TreePurpose
    {
        NONE, DRAW_CURRENT_KINDS_AND_ACTIONS, DRAW_CURRENT_NAMES, DRAW_SAVED_NAMES, DRAW_SAVED_KINDS_AND_ACTIONS, PATHS
    }

    public class AtfStorageTreeView : TreeView
    {
        public readonly TreePurpose TreePurpose;
        public AtfStorageTreeView KindsAndActionsTreeView;
        
        private List<TreeViewItem> _allItems;
        private readonly TreeViewItem _root;
        
        // ReSharper disable once MemberCanBePrivate.Global
        public const string NO_CURRENT_KINDS_AND_ACTIONS_SELECTED = "No current record selected.";
        // ReSharper disable once MemberCanBePrivate.Global
        public const string NO_SAVED_KINDS_AND_ACTIONS_SELECTED = "No saved record selected.";
        // ReSharper disable once MemberCanBePrivate.Global
        public const string NO_CURRENT_ACTIONS_LOADED = "No current actions loaded.";
        // ReSharper disable once MemberCanBePrivate.Global
        public const string NO_RECORDS_SAVED = "No records saved.";
        // ReSharper disable once MemberCanBePrivate.Global
        public const string NO_PATHS_ACCEPTED = "No paths accepted.";

        // ReSharper disable once MemberCanBePrivate.Global
        public readonly IAtfRecorder Recorder;
        public readonly IAtfActionStorage Storage;
        
        // ReSharper disable once MemberCanBePrivate.Global
        public readonly IAtfIntegrator Integrator;

        public delegate void RecordNameUsedHandler(string name, AtfStorageTreeView context);
        public event RecordNameUsedHandler RecordNameChanged;
        public event RecordNameUsedHandler RecordNameUsedInLoad;
        public event RecordNameUsedHandler PathChanged;
        
        public AtfStorageTreeView(TreePurpose treePurpose, TreeViewState treeViewState, IAtfRecorder recorder, IAtfActionStorage storage)
            : base(treeViewState)
        {
            TreePurpose = treePurpose;
            Recorder = recorder;
            Storage = storage;
            _root = new TreeViewItem {id = 0, depth = -1, displayName = "Root"};
            InitializeAllItems();
            Reload();
        }
        
        public AtfStorageTreeView(TreePurpose treePurpose, TreeViewState treeViewState, IAtfIntegrator integrator)
            : base(treeViewState)
        {
            TreePurpose = treePurpose;
            Integrator = integrator;
            _root = new TreeViewItem {id = 0, depth = -1, displayName = "Root"};
            InitializeAllItems();
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            SetupParentsAndChildrenFromDepths(_root, _allItems);
            return _root;
        }

        private void InitializeAllItems()
        {
            _allItems = new List<TreeViewItem>();
            switch (TreePurpose)
            {
                case TreePurpose.DRAW_CURRENT_NAMES:
                    _allItems.Add(new TreeViewItem {
                        id = DictionaryBasedIdGenerator.GetNewId(NO_CURRENT_ACTIONS_LOADED),
                        depth = 0,
                        displayName = NO_CURRENT_ACTIONS_LOADED
                    });
                    break;

                case TreePurpose.DRAW_CURRENT_KINDS_AND_ACTIONS:
                    _allItems.Add(new TreeViewItem {
                        id = DictionaryBasedIdGenerator.GetNewId(NO_CURRENT_KINDS_AND_ACTIONS_SELECTED),
                        depth = 0,
                        displayName = NO_CURRENT_KINDS_AND_ACTIONS_SELECTED
                    });
                    break;

                case TreePurpose.DRAW_SAVED_NAMES:
                    _allItems.Add(new TreeViewItem {
                        id = DictionaryBasedIdGenerator.GetNewId(NO_RECORDS_SAVED),
                        depth = 0,
                        displayName = NO_RECORDS_SAVED
                    });
                    break;

                case TreePurpose.DRAW_SAVED_KINDS_AND_ACTIONS:
                    _allItems.Add(new TreeViewItem {
                        id = DictionaryBasedIdGenerator.GetNewId(NO_SAVED_KINDS_AND_ACTIONS_SELECTED),
                        depth = 0,
                        displayName = NO_SAVED_KINDS_AND_ACTIONS_SELECTED
                    });
                    break;
                
                case TreePurpose.PATHS:
                    _allItems.Add(new TreeViewItem {
                        id = DictionaryBasedIdGenerator.GetNewId(NO_PATHS_ACCEPTED),
                        depth = 0,
                        displayName = NO_PATHS_ACCEPTED
                    });
                    break;
                    
                case TreePurpose.NONE:
                    throw new System.ArgumentOutOfRangeException(string.Empty, $"Tree purpose is invalid: {TreePurpose}");

                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }

        public void UpdateItems(List<TreeViewItem> items)
        {
            if (items == null || items.Count == 0) return;
            _allItems.RemoveAll(item => item.displayName.Equals(NO_CURRENT_ACTIONS_LOADED) 
                                       || item.displayName.Equals(NO_CURRENT_KINDS_AND_ACTIONS_SELECTED)
                                       || item.displayName.Equals(NO_RECORDS_SAVED)
                                       || item.displayName.Equals(NO_PATHS_ACCEPTED));
            _allItems = items;
            if (_allItems.Count == 0) return;
            SetupDepthsFromParentsAndChildren(_allItems[0]);
            Reload();
        }

        protected override void SingleClickedItem(int id)
        {
            base.SingleClickedItem(id);
            var clickedItem = FindItem(id, _root);
            if (clickedItem == null || clickedItem.depth > 0
                                    || clickedItem.displayName.Equals(NO_CURRENT_KINDS_AND_ACTIONS_SELECTED) 
                                    || clickedItem.displayName.Equals(NO_CURRENT_ACTIONS_LOADED)
                                    || clickedItem.displayName.Equals(NO_RECORDS_SAVED)
                                    || clickedItem.displayName.Equals(NO_SAVED_KINDS_AND_ACTIONS_SELECTED)
                                    || clickedItem.displayName.Equals(NO_PATHS_ACCEPTED)) return;
            switch (TreePurpose)
            {
                case TreePurpose.DRAW_CURRENT_NAMES:
                case TreePurpose.DRAW_SAVED_NAMES:
                    Storage.SetCurrentRecordName(clickedItem.displayName);
                    break;
                
                case TreePurpose.PATHS:
                case TreePurpose.DRAW_CURRENT_KINDS_AND_ACTIONS:
                case TreePurpose.DRAW_SAVED_KINDS_AND_ACTIONS:
                    break;
                
                case TreePurpose.NONE:
                    throw new System.ArgumentOutOfRangeException(string.Empty, "Tree purpose is NONE!");

                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }

        protected override void DoubleClickedItem(int id)
        {
            base.DoubleClickedItem(id);
            var clickedItem = FindItem(id, _root);
            if (clickedItem == null || clickedItem.depth > 0
                                    || clickedItem.displayName.Equals(NO_CURRENT_KINDS_AND_ACTIONS_SELECTED) 
                                    || clickedItem.displayName.Equals(NO_CURRENT_ACTIONS_LOADED)
                                    || clickedItem.displayName.Equals(NO_RECORDS_SAVED)
                                    || clickedItem.displayName.Equals(NO_PATHS_ACCEPTED)) return;
            switch (TreePurpose)
            {
                case TreePurpose.DRAW_CURRENT_NAMES:
                    KindsAndActionsTreeView?.UpdateItems(Storage.GetCurrentActions(clickedItem.displayName));
                    break;
                
                case TreePurpose.DRAW_SAVED_NAMES:
                    KindsAndActionsTreeView?.UpdateItems(Storage.GetSavedActions(clickedItem.displayName));
                    break;
                
                case TreePurpose.PATHS:
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
            var clickedItem = FindItem(id, _root);
            if (clickedItem == null || clickedItem.depth > 0
                                    || clickedItem.displayName.Equals(NO_CURRENT_KINDS_AND_ACTIONS_SELECTED) 
                                    || clickedItem.displayName.Equals(NO_CURRENT_ACTIONS_LOADED)
                                    || clickedItem.displayName.Equals(NO_RECORDS_SAVED)
                                    || clickedItem.displayName.Equals(NO_PATHS_ACCEPTED)) return;
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

                case TreePurpose.PATHS:
                    PathChanged?.Invoke(clickedItem.displayName, this);
                    Integrator.SetCurrentRecordName(clickedItem.displayName);
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
