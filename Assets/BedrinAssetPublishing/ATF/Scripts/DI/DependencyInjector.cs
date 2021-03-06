﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ATF.Scripts.Helper;
using UnityEngine;

namespace ATF.Scripts.DI
{
    #region Attributes definition
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class InjectAttribute : Attribute
    {
        public Type ComponentType { get; set; }
        public string ScenePath { get; set; }
        public bool IsAutoBinding { get; set; }
        public bool LookInScene { get; set; }

        public InjectAttribute(bool lookInScene=true)
        {
            LookInScene = lookInScene;
        }


        public InjectAttribute(Type componentType, bool lookInScene = true)
        {
            ComponentType = componentType;
            LookInScene = lookInScene;
        }

        public InjectAttribute(string scenePath, bool lookInScene = true)
        {
            ScenePath = scenePath;
            LookInScene = lookInScene;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class InjectableAttribute : Attribute { }
    
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class AtfSystemAttribute : Attribute { }
    #endregion

    [AtfSystem]
    public class DependencyInjector : MonoSingleton<DependencyInjector>
    {
        private class PathValidationResult
        {
            public string[] Result;
            public string FullPath;
            public bool Valid;
        }

        private string CurrentNamespace { get; set; }

        private static void Print(object obj)
        {
            if (AtfInitializer.Instance.isDebugPrintOn)
            {
                print(obj);
            }
        }

        private static bool ContainsAnyAttributeOfType(object[] attributes, Type attributeType)
        {
            return attributes != null && attributeType != null && attributes.Any(t => t.GetType() == attributeType);
        }

        private static PathValidationResult GetHierarchyPathAndComponentName(string fullPath)
        {
            var result = new PathValidationResult()
            {
                Result = new string[2],
                FullPath = fullPath,
                Valid = true
            };
            if (!Regex.Match(fullPath, @"(?:/\w+)+(?:/\w+)").Success)
            {
                result.Valid = false;
            }
            var split = fullPath.Split('/');
            result.Result[1] = split[split.Length - 1];
            var path = new string[split.Length - 1];
            Array.Copy(split, 0, path, 0, split.Length - 1);
            result.Result[0] = string.Join("/", path);
            return result;
        }

        private static string GetGameObjectPath(GameObject obj)
        {
            if (!obj) return null;
            var path = "/" + obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }
            return path;
        }

        public static IEnumerable<Type> GetAttributeTypesInNamespace(string @namespace, Type attributeType)
        {
            return
                Assembly
                    .GetExecutingAssembly()
                    .GetTypes()
                    .Where(
                        t => t.Namespace != null 
                             && t.Namespace.StartsWith(@namespace) 
                             && ContainsAnyAttributeOfType(t.GetCustomAttributes(false), attributeType)
                    )
                    .ToArray();
        }
        
        private static IEnumerable<Type> GetInjectableTypesInNamespace(string @namespace)
        {
            return GetAttributeTypesInNamespace(@namespace, typeof(InjectableAttribute));
        }

        public static void InjectType(Type t)
        {
            if (!ContainsAnyAttributeOfType(t.GetCustomAttributes(false), typeof(InjectableAttribute))) return;
            foreach (var fi in t.GetFields())
            {
                var fiAttributes = fi.GetCustomAttributes(true);
                if (!ContainsAnyAttributeOfType(fiAttributes, typeof(InjectAttribute))) continue;
                if (!(fiAttributes[0] is InjectAttribute)) continue;
                var temp = (InjectAttribute) fiAttributes[0];
                var isScenePathEmpty = string.IsNullOrEmpty(temp.ScenePath);
                if (temp.ComponentType == null && isScenePathEmpty)
                {
                    temp.ComponentType = fi.FieldType;
                }

                Print(
                    $"Injectable {t}: contains field ({fi}), with custom attribute ({temp}) of" +
                    $" inject type ({(temp.ComponentType != null ? temp.ComponentType.ToString() : "None")}) " +
                    $"and path ({(isScenePathEmpty ? "None" : temp.ScenePath)}). Searching on scene..."
                    );

                UnityEngine.Object objectToInject;
                GameObject gameObjectContainingObjectToInject = null;
                if (!isScenePathEmpty)
                {
                    var hierarchyAndComponent = GetHierarchyPathAndComponentName(temp.ScenePath);
                    if (!hierarchyAndComponent.Valid)
                    {
                        Print(
                            $"Injectable ({t}): The path is not valid: {hierarchyAndComponent.FullPath} in injection ({fi}). Moving on...");
                        continue;
                    }
                    gameObjectContainingObjectToInject = GameObject.Find(hierarchyAndComponent.Result[0]);
                    if (!gameObjectContainingObjectToInject)
                    {
                        Print($"Injectable {t}: cannot find object to inject on scene. Moving on...");
                        continue;
                    }
                    objectToInject = gameObjectContainingObjectToInject.GetComponent(hierarchyAndComponent.Result[1]);
                }
                else
                {
                    objectToInject = FindObjectOfType(temp.ComponentType);
                    if (objectToInject)
                    {
                        gameObjectContainingObjectToInject = GameObject.Find(objectToInject.name);
                    }
                    if (!gameObjectContainingObjectToInject && !temp.LookInScene)
                    {
                        objectToInject = Activator.CreateInstance(temp.ComponentType) as UnityEngine.Object;
                        if (!objectToInject)
                        {
                            Print(
                                $"Injectable {t}: could not create the instance of injection {temp.ComponentType}. Moving on...");
                            continue;
                        }
                        Print($"Injectable {t}: injection {fi} created from type {temp.ComponentType}.");
                    }
                }

                var pathOfGameObject = GetGameObjectPath(gameObjectContainingObjectToInject);
                pathOfGameObject = !string.IsNullOrEmpty(pathOfGameObject) ? pathOfGameObject : "None";

                if (!gameObjectContainingObjectToInject && objectToInject == null && temp.LookInScene)
                {
                    Print($"Injectable {t}: cannot find object to inject on scene. Moving on...");
                    continue;
                }

                Print($"Injectable {t}: trying to inject found object {objectToInject} at path {pathOfGameObject}.");
                dynamic typeToWhichInjected = FindObjectOfType(t.GetTypeInfo());
                if (typeToWhichInjected == null)
                {
                    Print($"Injectable {t}: cannot find injectable object in memory or on scene. Moving on...");
                    continue;
                }
                fi.SetValue(typeToWhichInjected, objectToInject);
                Print($"Injectable {t}: injected {objectToInject} at path {pathOfGameObject}.");
            }
        }

        public void Inject()
        {
            InjectScene(CurrentNamespace);
        }

        private static void ForEachTypeInTypesOfNamespace(string @namespace, Action<Type> action)
        {
            foreach (var t in GetInjectableTypesInNamespace(@namespace))
            {
                action(t);
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public static void InjectScene(string @namespace)
        {
            ForEachTypeInTypesOfNamespace(@namespace, InjectType);
        }

        public void Initialize(string @namespace)
        {
            CurrentNamespace = @namespace;
        }

        public override void Initialize()
        {
            Initialize(AtfInitializer.ATF_NAMESPACE_NAME);
            base.Initialize();
        }
    }
}
