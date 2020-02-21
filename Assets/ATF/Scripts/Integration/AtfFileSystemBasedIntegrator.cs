using Bedrin.Helper;
using System.IO;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ATF.Scripts.Integration
{
    public class AtfFileSystemBasedIntegrator : MonoSingleton<AtfFileSystemBasedIntegrator>,  IAtfIntegrator
    {
        private List<string> _paths;
        private string _currentRecordName;
        
        public void Initialize()
        {
            _paths = new List<string>();
        }

        public void SetUrls(IEnumerable<string> filePaths)
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
