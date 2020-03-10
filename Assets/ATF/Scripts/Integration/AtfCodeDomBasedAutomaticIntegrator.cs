using System;
using System.CodeDom;
using System.Windows;
using System.IO;
using System.Text;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using ATF.Scripts.DI;
using ATF.Scripts.Helper;
using ATF.Scripts.Integration.Interfaces;
using UnityEngine;
using Microsoft.CSharp;

namespace ATF.Scripts.Integration
{
    [AtfSystem]
    public class AtfCodeDomBasedAutomaticIntegrator : MonoSingleton<AtfCodeDomBasedAutomaticIntegrator>, IAtfAutomaticIntegrator
    {
        private List<StreamReader> _sourceCodeFiles;
        private CodeDomProvider _codeDomProvider;
        
        public override void Initialize()
        {
            _sourceCodeFiles = new List<StreamReader>();
            _codeDomProvider = CodeDomProvider.CreateProvider("CSharp");
            base.Initialize();
        }
        
        public void IntegrateAll()
        {
            // //Find all files
            // CollectAllSourceFiles();
            // Debug.Log("Collecting all source files...");
            // foreach (var dom in _sourceCodeFiles.Select(streamReader => _codeDomProvider.Parse(streamReader)))
            // {
            //     foreach (CodeTypeDeclaration type in dom.Namespaces[0].Types)
            //     {
            //         foreach (var memberObj in type.Members)
            //         {
            //             if (memberObj is CodeMemberMethod memberMethod)
            //             {
            //                 var inputExpression = memberMethod.Statements.IndexOf(new CodeMethodInvokeExpression(
            //                     new CodeTypeReferenceExpression(typeof(Input)),
            //                     "ReadAllData", new CodePrimitiveExpression(@"C:\File.exe")));
            //             }
            //
            //             if (!(memberObj is CodeMemberProperty memberProperty)) continue;
            //             foreach (CodeStatement statement in  memberProperty.GetStatements)
            //             {
            //                     
            //             }
            //         }
            //     }
            // }
            Debug.Log("Integrate ALL from auto integrator");
        }
        
        // find all files
        private void CollectAllSourceFiles()
        {
            var root = $"{Application.dataPath}{Path.DirectorySeparatorChar}";
            _sourceCodeFiles = Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories)
                .Select(e => new StreamReader(e)).ToList();
        }
    }
}
