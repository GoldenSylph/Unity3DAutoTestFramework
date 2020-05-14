using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using ATF.Scripts.Helper;
using ATF.Scripts.Integration;
using ATF.Scripts.Integration.Interfaces;
using ATF.Scripts.Recorder;
using ATF.Scripts.Storage.Interfaces;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Serialization;

namespace ATF.Scripts.Editor
{
    public class AtfIntegratorWindow : EditorWindow
    {
        public IAtfIntegrator integrator;

        private string _currentPath = "";
        private HashSet<string> _pathsToSendIntoIntegrator;

        [SerializeField]
        private TreeViewState treeViewStateForPaths;
        private AtfStorageTreeView _treeViewForPaths;
        private SearchField _searchFieldForPaths;
        
        private void OnFocus()
        {
            if (!EditorApplication.isPlaying) return;
            integrator = FindObjectOfType<AtfFileSystemBasedIntegrator>();
            AtfWindow.InitTreeViewOf(ref _treeViewForPaths, ref _searchFieldForPaths, ref treeViewStateForPaths, TreePurpose.PATHS, integrator);
            _treeViewForPaths.PathChanged += (s, context) => _currentPath = s;
        }

        public static string CheckPath(string path, bool isFileInAssetsFolder, string fileFormat)
        {
            var message = new StringBuilder();
            if (!Regex.IsMatch(path, $@"^(?:\w:/)?(?:\w+/)*(?:\w+/\w+.{fileFormat})$"))
            {
                message.Append(isFileInAssetsFolder
                    ? "Path is not valid. It must be a relational file directory, where root is Assets folder.\n"
                    : "Path is not valid. It must be a full absolute path to the file.");
            }
            if (isFileInAssetsFolder && !File.Exists($"{Application.dataPath}{Path.DirectorySeparatorChar}{path}"))
            {
                message.Append("This file does not exist.\n");
            }

            if (message.Length == 0)
            {
                message.Append(0);
            }
            return message.ToString();
        }
        
        private string CheckCurrentPath()
        {
            return CheckPath(_currentPath, true, "cs");
        }

        private void UpdateTree()
        {
            integrator.SetUris(_pathsToSendIntoIntegrator);
            if (_pathsToSendIntoIntegrator.Count == 0)
            {
                // ReSharper disable once InconsistentNaming
                const string NO_PATHS_ACCEPTED = "No paths accepted";
                _treeViewForPaths.UpdateItems(new List<TreeViewItem>
                {
                    new TreeViewItem
                    {
                        id = DictionaryBasedIdGenerator.GetNewId(NO_PATHS_ACCEPTED),
                        displayName = NO_PATHS_ACCEPTED
                    }
                });    
            }
            else
            {
                _treeViewForPaths.UpdateItems(_pathsToSendIntoIntegrator.Select(s => new TreeViewItem
                {
                    id = DictionaryBasedIdGenerator.GetNewId(s),
                    displayName = s
                }).ToList());
            }
        }
        
        private void UpdatePathsButton(string buttonText, Action ifPathValid)
        {
            if (!GUILayout.Button(buttonText)) return;
            if (_pathsToSendIntoIntegrator == null) _pathsToSendIntoIntegrator = new HashSet<string>();
            var pathValidationResult = CheckCurrentPath();
            int _;
            if (int.TryParse(pathValidationResult, out _))
            {
                ifPathValid();
                UpdateTree();
            }
            else
            {
                Debug.Log($"Path is invalid. Cannot add or remove the path in integration list. Reason is: {pathValidationResult}");
            }
        }
        
        private void OnGUI()
        {
            var integratorLoaded = integrator != null;
            if (EditorApplication.isPlaying)
            {
                GUILayout.Label("Integrator Control Panel", EditorStyles.boldLabel);
                GUILayout.Label(
                    integratorLoaded
                        ? $"Integrator current realisation: {integrator.GetType().Name}"
                        : "Integrator current realisation: Waiting to focus...", EditorStyles.label);
                if (!integratorLoaded) return;
                
                GUILayout.Label("Adding files to integrate", EditorStyles.boldLabel);
                _currentPath = EditorGUILayout.TextField(".cs file path", _currentPath);
                
                EditorGUILayout.BeginHorizontal();
                UpdatePathsButton("Add path", () => _pathsToSendIntoIntegrator.Add(_currentPath));
                UpdatePathsButton("Remove path", () => _pathsToSendIntoIntegrator.Remove(_currentPath));
                EditorGUILayout.EndHorizontal();
                
                GUILayout.Label("Paths chosen", EditorStyles.boldLabel);
                AtfWindow.DoToolbarFor(_treeViewForPaths, _searchFieldForPaths);
                AtfWindow.DoTreeViewFor(_treeViewForPaths);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Integrate"))
                {
                    integrator.Integrate();
                }
                if (GUILayout.Button("Integrate and replace"))
                {
                    integrator.IntegrateAndReplace();
                }
                EditorGUILayout.EndHorizontal();
                if (GUILayout.Button("Integrate All"))
                {
                    integrator.IntegrateAll();
                }
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Save paths"))
                {
                    integrator.SaveUris();
                }
                if (GUILayout.Button("Load paths"))
                {
                    _pathsToSendIntoIntegrator = new HashSet<string>();
                    var rawPaths = integrator.LoadUris();
                    foreach (var path in rawPaths)
                    {
                        _pathsToSendIntoIntegrator.Add(path);
                    }
                    UpdateTree();
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label("Waiting for Play Mode...", EditorStyles.boldLabel);
            }
        }
    }
}
