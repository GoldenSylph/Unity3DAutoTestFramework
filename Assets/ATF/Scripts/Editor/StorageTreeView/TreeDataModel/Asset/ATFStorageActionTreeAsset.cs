using System.Collections.Generic;
using ATF.Scripts.Editor.StorageTreeView;
using UnityEngine;

namespace ATF.Scripts.Editor.StorageTreeView.TreeDataModel.Asset
{
    [CreateAssetMenu(fileName = "ActionStorageTreeAsset", menuName = "Action Storage Tree Asset", order = 1)]
    public class ATFStorageActionTreeAsset : ScriptableObject
    {
        public List<ATFStorageTreeElement> TreeElements { get; set; } = new List<ATFStorageTreeElement>();
    }
}
