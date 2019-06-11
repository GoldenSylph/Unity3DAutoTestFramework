using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using ATF.Helper;

namespace ATF.DI
{
    #region Attributes definition
    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public class ATFInject : System.Attribute {}

    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class ATFInjectable : System.Attribute { }
    #endregion

    public class ATFDependencyInjector : MonoSingleton<ATFDependencyInjector>
    {
        
        private Type[] GetTypesInATFNamespace()
        {
            return
                Assembly.GetExecutingAssembly().GetTypes()
                        .Where(t => t.Namespace.StartsWith("ATF"))
                        .ToArray();
        }

        private void Start()
        {
            print("Start injecting the dependencies");
            foreach(Type t in GetTypesInATFNamespace())
            {
                
            }
        }
    }
}
