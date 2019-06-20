using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Serialization;

namespace ATF.Storage
{
    [Serializable]
    public class ATFStorageTreeElement : TreeElement
    {
        public string recordName;
        public object inputContent;
        public FakeInput kindOfInput;
        public bool enabled = true;
    }
}
