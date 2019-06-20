using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ATF.Storage
{
    [CreateAssetMenu(fileName = "ActionStorageTreeAsset", menuName = "Action Storage Tree Asset", order = 1)]
    public class ATFStorageActionTreeAsset : ScriptableObject
    {
        public List<ATFStorageTreeElement> TreeElements { get; set; } = new List<ATFStorageTreeElement>();
    }
}
