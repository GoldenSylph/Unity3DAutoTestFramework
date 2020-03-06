using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text.RegularExpressions;
using ATF.Scripts.DI;
using ATF.Scripts.Helper;
using UnityEditor;
using UnityEngine;

namespace ATF.Scripts.Integration
{
    [AtfSystem]
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
                PerformIntegrationForPath(script, false);
            }
        }

        public void IntegrateAndReplace()
        {
            foreach (var script in _paths)
            {
                PerformIntegrationForPath(script, true);
            }
        }

        public void IntegrateAll()
        {
            print("Integrate All");
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

        private static string GetFilePathAccordingToMode(string filePath, bool isReplacing)
        {
            return isReplacing ? filePath : filePath.Insert(filePath.Length - 3, "ATF");
        }
        
        private static void PerformIntegrationForPath(string filePath, bool isReplacing)
        {
            try
            {
                string scriptSource;
                var fullPath = $"{Application.dataPath}{Path.DirectorySeparatorChar}{filePath}";
                using (var sr = new StreamReader(fullPath))
                {
                    scriptSource = sr.ReadToEnd();
                }
                scriptSource = scriptSource.Replace("Input.", "AtfInput.");
                using (var writer = new StreamWriter(GetFilePathAccordingToMode(fullPath, isReplacing)))  
                {  
                    writer.Write(scriptSource);  
                }
                AssetDatabase.Refresh();
                print($"Performed integration for {filePath}, replacing mode: {isReplacing}");
                if (!isReplacing)
                {
                    Debug.LogWarning("After exiting player mode, please, change the class name of the generated file as you want.");
                }
            }
            catch (Exception e)
            {
                print(e.Message);
            }
        }

        public string GetCurrentRecordName()
        {
            return _currentRecordName;
        }

        public void SetCurrentRecordName(string recordName)
        {
            _currentRecordName = recordName;
        }
    }
}
