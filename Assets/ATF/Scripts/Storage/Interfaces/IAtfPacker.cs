using System.Collections.Generic;
using ATF.Scripts.Storage.Utils;
using UnityEngine;

namespace ATF.Scripts.Storage.Interfaces
{
    public interface IAtfPacker
    {
        List<Record> Pack(Dictionary<string, Dictionary<FakeInput, Dictionary<object, AtfActionRleQueue>>> input);
        Dictionary<string, Dictionary<FakeInput, Dictionary<object, AtfActionRleQueue>>> Unpack(Slot slot);
        string ValidatePacked(List<Record> packed);
    }
}
