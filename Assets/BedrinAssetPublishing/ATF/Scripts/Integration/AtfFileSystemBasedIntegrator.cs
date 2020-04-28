using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using ATF.Scripts.DI;
using ATF.Scripts.Helper;
using ATF.Scripts.Integration.Interfaces;
using UnityEditor;
using UnityEngine;

namespace ATF.Scripts.Integration
{
    [AtfSystem]
    [Injectable]
    public class AtfFileSystemBasedIntegrator : MonoSingleton<AtfFileSystemBasedIntegrator>,  IAtfIntegrator
    {
        [Serializable]
        public class SerializedPaths
        {
            public List<string> paths;
        }
        
        private const string SAVE_KEY = "FSBI_URIS";

        private List<string> _paths;
        private string _currentRecordName;

        public override void Initialize()
        {
            _paths = new List<string>();
            base.Initialize();
        }

        public void SetUris(IEnumerable<string> filePaths)
        {
            _paths.AddRange(filePaths);
        }

        public void Integrate()
        {
            foreach (var script in _paths)
            {
                PerformIntegrationForPath(script, false, false);
            }
        }

        public void IntegrateAndReplace()
        {
            foreach (var script in _paths)
            {
                PerformIntegrationForPath(script, true, false);
            }
        }

        public void IntegrateAll()
        {
            Debug.LogWarning("Starting full integration...");
            var sourceCodeFileNames = Directory.GetFiles(GetRoot(), "*.cs", SearchOption.AllDirectories)
                .Where(filename => !filename.Contains("ATF"));
            foreach (var sourceFileName in sourceCodeFileNames)
            {
                print($"Integrating in {sourceFileName}...");
                PerformIntegrationForPath(sourceFileName, true, true);
            }
            Debug.LogWarning("Full integration complete...");
        }

        public void SaveUris()
        {
            var serializedPaths = new SerializedPaths {paths = new List<string>()};
            if (_paths != null && _paths.Count > 0)
            {
                _paths.ForEach(e => serializedPaths.paths.Add(e));
                PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(serializedPaths));
                print($"All paths are saved in PlayerPrefs under the key {SAVE_KEY}");
            }
            else
            {
                Debug.LogWarning("Paths are not saved because the paths list is empty.");
            }
        }

        public IEnumerable<string> LoadUris()
        {
            var serializedPaths = JsonUtility.FromJson<SerializedPaths>(PlayerPrefs.GetString(SAVE_KEY));
            _paths = new List<string>();
            serializedPaths.paths.ForEach(e => _paths.Add(e));
            print($"All paths are loaded from PlayerPrefs under the key {SAVE_KEY}");
            return _paths;
        }
        
        public string GetCurrentRecordName()
        {
            return _currentRecordName;
        }

        public void SetCurrentRecordName(string recordName)
        {
            _currentRecordName = recordName;
        }

        private static string GetFilePathAccordingToMode(string filePath, bool isReplacing)
        {
            return isReplacing ? filePath : filePath.Insert(filePath.Length - 3, "ATF");
        }

        private static string ClassNameMatchEvaluator(Match match)
        {
            var groups = match.Groups;
            return $"{groups["prefix"]}AtfInput.Instance{groups["postfix"]}";
        }
        
        private static void PerformIntegrationForPath(string filePath, bool isReplacing, bool isFoolFilePath)
        {
            try
            {
                string scriptSource;
                var fullPath = isFoolFilePath ? filePath : $"{GetRoot()}{filePath}";
                using (var sr = new StreamReader(fullPath))
                {
                    scriptSource = sr.ReadToEnd();
                }
                
                var matchEvaluator = new MatchEvaluator(ClassNameMatchEvaluator);
                var originalSource = scriptSource;
                scriptSource = Regex.Replace(scriptSource, @"(?<prefix>[(\[(\s,=\+\-\*/\?&|])Input(?<postfix>\.)", matchEvaluator);
                if (scriptSource.Equals(originalSource))
                {
                    print($"{filePath} is already integrated...");
                    return;
                }
                // var user = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                // print($"Integrating as user: {user}");
                // AddDirectorySecurity(fullPath, user, FileSystemRights.Write, AccessControlType.Allow);
                using (var writer = new StreamWriter(GetFilePathAccordingToMode(fullPath, isReplacing)))  
                {  
                    writer.Write(scriptSource);  
                }
                // RemoveDirectorySecurity(fullPath, user, FileSystemRights.Write, AccessControlType.Allow);
                AssetDatabase.Refresh();
                print($"Performed integration for {filePath}, replacing mode: {isReplacing}");
                if (!isReplacing)
                {
                    Debug.LogWarning("After exiting player mode, please, change the class name of the generated file as you want.");
                }
            }
            catch (Exception e)
            {
                print($"{e.GetType()} {e.Message}");
            }
        }

        private static string GetRoot()
        {
            return $"{Application.dataPath}{Path.DirectorySeparatorChar}";
        }
        
        // Adds an ACL entry on the specified directory for the specified account.
        private static void AddDirectorySecurity(string fileName, string account, FileSystemRights rights, AccessControlType controlType)
        {
            // Create a new DirectoryInfo object.
            var fInfo = new FileInfo(fileName);

            // Get a DirectorySecurity object that represents the 
            // current security settings.
            var fSecurity = fInfo.GetAccessControl();

            // Add the FileSystemAccessRule to the security settings. 
            fSecurity.AddAccessRule(new FileSystemAccessRule(account,
                rights,
                controlType));

            // Set the new access settings.
            fInfo.SetAccessControl(fSecurity);
        }

        // Removes an ACL entry on the specified directory for the specified account.
        private static void RemoveDirectorySecurity(string fileName, string account, FileSystemRights rights, AccessControlType controlType)
        {
            // Create a new DirectoryInfo object.
            var fInfo = new FileInfo(fileName);

            // Get a DirectorySecurity object that represents the 
            // current security settings.
            var fSecurity = fInfo.GetAccessControl();

            // Add the FileSystemAccessRule to the security settings. 
            fSecurity.RemoveAccessRule(new FileSystemAccessRule(account,
                rights,
                controlType));

            // Set the new access settings.
            fInfo.SetAccessControl(fSecurity);
        }
    }
}
