using System;
using ATF.Scripts.Editor.StorageTreeView.TreeDataModel;

namespace ATF.Scripts.Editor.StorageTreeView
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
