using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using Bedrin.Helper;
using System.Text.RegularExpressions;

namespace Bedrin.DI
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
    #endregion

    public class DependencyInjector : MonoSingleton<DependencyInjector>
    {

        public static bool DebugOn = true;

        private class PathValidationResult
        {
            public string[] Result;
            public string FullPath;
            public bool Valid;
        }

        public string CurrentNamespace { get; set; }

        public static void Print(object obj)
        {
            if (DebugOn)
            {
                print(obj);
            }
        }

        private static bool ContainsAnyAttributeOfType(object[] attributes, Type attributeType)
        {
            return attributes.Where(t => t.GetType() == attributeType).Any();
        }

        private static PathValidationResult GetHierarchyPathAndComponentName(string fullPath)
        {
            PathValidationResult result = new PathValidationResult()
            {
                Result = new string[2],
                FullPath = fullPath,
                Valid = true
            };
            if (!Regex.Match(fullPath, @"(?:/\w+)+(?:/\w+)").Success)
            {
                result.Valid = false;
            }
            string[] splitted = fullPath.Split('/');
            result.Result[1] = splitted[splitted.Length - 1];
            string[] path = new string[splitted.Length - 1];
            Array.Copy(splitted, 0, path, 0, splitted.Length - 1);
            result.Result[0] = string.Join("/", path);
            return result;
        }

        private static string GetGameObjectPath(GameObject obj)
        {
            if (!obj) return null;
            string path = "/" + obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }
            return path;
        }

        protected Type[] GetInjectableTypesInNamespace(string _namespace)
        {
            return
                Assembly.GetExecutingAssembly().GetTypes()
                        .Where(t => t.Namespace.StartsWith(_namespace) && ContainsAnyAttributeOfType(t.GetCustomAttributes(false), typeof(InjectableAttribute)))
                        .ToArray();
        }

        public void InjectType(Type t)
        {
            if (ContainsAnyAttributeOfType(t.GetCustomAttributes(false), typeof(InjectableAttribute)))
            {
                foreach (FieldInfo fi in t.GetFields())
                {
                    object[] fiAttributes = fi.GetCustomAttributes(true);
                    if (ContainsAnyAttributeOfType(fiAttributes, typeof(InjectAttribute)))
                    {
                        InjectAttribute temp = fiAttributes[0] as InjectAttribute;
                        bool isScenePathEmpty = temp.ScenePath == null || temp.ScenePath.Length == 0;
                        if (temp.ComponentType == null && isScenePathEmpty)
                        {
                            temp.ComponentType = fi.FieldType;
                        }

                        Print(string.Format("Injectable {0}: contains field ({1}), with custom attribute ({2}) of inject type ({3}) and path ({4}). Searching on scene...",
                            t, fi, temp, (temp.ComponentType != null) ? temp.ComponentType.ToString() : "None", isScenePathEmpty ? "None" : temp.ScenePath));

                        UnityEngine.Object objectToInject;
                        GameObject gameObjectContainingObjectToInject = null;
                        if (!isScenePathEmpty)
                        {
                            PathValidationResult hierarchyAndComponent = GetHierarchyPathAndComponentName(temp.ScenePath);
                            if (!hierarchyAndComponent.Valid)
                            {
                                Print(string.Format("Injectable ({2}): The path is not valid: {0} in injection ({1}). Moving on...", hierarchyAndComponent.FullPath, fi, t));
                                continue;
                            }
                            gameObjectContainingObjectToInject = GameObject.Find(hierarchyAndComponent.Result[0]);
                            if (!gameObjectContainingObjectToInject)
                            {
                                Print(string.Format("Injectable {0}: cannot find object to inject on scene. Moving on...", t));
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
                                    Print(string.Format("Injectable {0}: could not create the instance of injection {1}. Moving on...", t, temp.ComponentType));
                                    continue;
                                }
                                Print(string.Format("Injectable {0}: injection {1} created from type {2}.", t, fi, temp.ComponentType));
                            }
                        }

                        string pathOfGameObject = GetGameObjectPath(gameObjectContainingObjectToInject);
                        pathOfGameObject = (pathOfGameObject != null && pathOfGameObject.Length > 0) ? pathOfGameObject : "None";

                        if (!gameObjectContainingObjectToInject && objectToInject == null && temp.LookInScene)
                        {
                            Print(string.Format("Injectable {0}: cannot find object to inject on scene. Moving on...", t));
                            continue;
                        }

                        Print(string.Format("Injectable {0}: trying to inject found object {1} at path {2}.", t, objectToInject, pathOfGameObject));
                        dynamic typeToWhichInjected = FindObjectOfType(t.GetTypeInfo());
                        if (typeToWhichInjected == null)
                        {
                            Print(string.Format("Injectable {0}: cannot find injectable object in memory or on scene. Moving on...", t));
                            continue;
                        }
                        fi.SetValue(typeToWhichInjected, objectToInject);
                        Print(string.Format("Injectable {0}: injected {1} at path {2}.", t, objectToInject, pathOfGameObject));
                    }
                }
            }
        }

        public void Inject()
        {
            InjectScene(CurrentNamespace);
        }

        protected void ForEachTypeInTypesOfNamespace(string _namespace, Action<Type> action)
        {
            foreach (Type t in GetInjectableTypesInNamespace(_namespace))
            {
                action(t);
            }
        }

        protected void InjectScene(string _namespace)
        {
            ForEachTypeInTypesOfNamespace(_namespace, (t) => {
                InjectType(t);
            });
        }

        public void Initialize(string _namespace)
        {
            CurrentNamespace = _namespace;
        }
    }
}
