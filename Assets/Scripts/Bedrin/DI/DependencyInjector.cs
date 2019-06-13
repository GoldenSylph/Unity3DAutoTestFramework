using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using Bedrin.Helper;
namespace Bedrin.DI
{
    #region Attributes definition
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class InjectAttribute : Attribute
    {
        public Type ComponentType { get; set; }
        public string ScenePath { get; set; }
        public bool IsAutoBinding { get; set; }

        public InjectAttribute()
        {
            IsAutoBinding = true;
        }

        public void SetAutoBind(Type self)
        {
            ComponentType = self;
        }

        public InjectAttribute(Type componentType)
        {
            ComponentType = componentType;
        }

        public InjectAttribute(string scenePath)
        {
            ScenePath = scenePath;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class InjectableAttribute : Attribute { }
    #endregion

    public class DependencyInjector : MonoSingleton<DependencyInjector>
    {

        public string CurrentNamespace { get; set; }

        public static bool ContainsAnyAttributeOfType(object[] attributes, Type attributeType)
        {
            return attributes.Where(t => t.GetType() == attributeType).Any();
        }

        protected Type[] GetInjectableTypesInNamespace(string _namespace)
        {
            return
                Assembly.GetExecutingAssembly().GetTypes()
                        .Where(t => t.Namespace.StartsWith(_namespace) && ContainsAnyAttributeOfType(t.GetCustomAttributes(false), typeof(InjectableAttribute)))
                        .ToArray();
        }

        public void Inject()
        {
            Inject(CurrentNamespace);
        }

        protected void Inject(string _namespace)
        {
            print("ATF.DI: Start injecting the dependencies");
            foreach (Type t in GetInjectableTypesInNamespace(_namespace))
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
                        print(string.Format("Injectable {0}: contains field ({1}), with custom attribute ({2}) of inject type ({3}) and path ({4}). Searching on scene...",
                            t, fi, temp, temp.ComponentType, (isScenePathEmpty ? "None" : temp.ScenePath)));
                        //check existance of the gameobject on scene
                        //get the component
                    }
                }
            }
        }

        public void Initialize(string _namespace)
        {
            CurrentNamespace = _namespace;
        }
    }
}
