using System;
using System.Collections.Generic;
using System.Linq;
using ATF.Scripts.DI;
using ATF.Scripts.Helper;
using ATF.Scripts.Storage.Interfaces;
using ATF.Scripts.Storage.Utils;
using UnityEngine;

namespace ATF.Scripts.Storage
{
    [AtfSystem]
    public class AtfGreedyPacker : MonoSingleton<AtfGreedyPacker>, IAtfPacker
    {
        public List<Record> Pack(Dictionary<string, Dictionary<FakeInput, Dictionary<object, AtfActionRleQueue>>> input) 
        {
            var result = new List<Record>();
            foreach (var recordName in input.Keys)
            {
                var newRecord = new Record
                {
                    recordName = recordName,
                    fakeInputsWithFipsAndActions = new List<FakeInputWithFipAndActions>()
                };
                foreach (var fakeInput in input[recordName].Keys)
                {
                    var newFakeInputWithFipAndActions = new FakeInputWithFipAndActions
                    {
                        fakeInput = fakeInput,
                        fipsAndActions = new List<FipAndActions>()
                    };
                    foreach (var fip in input[recordName][fakeInput].Keys)
                    {
                        var listedQueueDistinctElements = input[recordName][fakeInput][fip].ToList();
                        var metadata = input[recordName][fakeInput][fip].rleCounts.Select((t, i) 
                            => new Metadata {action = listedQueueDistinctElements[i], repetitions = t}).ToList();
                        newFakeInputWithFipAndActions.fipsAndActions.Add(new FipAndActions
                        {
                            fakeInputParameter = fip.ToString(),
                            metadata = metadata,
                            last = input[recordName][fakeInput][fip].last
                        });
                    }
                    newRecord.fakeInputsWithFipsAndActions.Add(newFakeInputWithFipAndActions);
                }
                result.Add(newRecord);
            }
            return result;
        }

        public Dictionary<string, Dictionary<FakeInput, Dictionary<object, AtfActionRleQueue>>> Unpack(Slot slot)
        {
            if (slot == null) return null;
            var result = new Dictionary<string, Dictionary<FakeInput, Dictionary<object, AtfActionRleQueue>>>();
            foreach (var record in slot.content)
            {
                result[record.recordName] = new Dictionary<FakeInput, Dictionary<object, AtfActionRleQueue>>();
                foreach (var fakeInputWithFipAndActions in record.fakeInputsWithFipsAndActions)
                {
                    result[record.recordName][fakeInputWithFipAndActions.fakeInput] = new Dictionary<object, AtfActionRleQueue>();
                    foreach (var fipAndActions in fakeInputWithFipAndActions.fipsAndActions)
                    {
                        var newRleQueue = new AtfActionRleQueue {last = fipAndActions.last};
                        foreach (var metadata in fipAndActions.metadata)
                        {
                            newRleQueue.EnqueueWithoutOptimization(metadata.action.GetDeserialized());
                            newRleQueue.rleCounts.AddToBack(metadata.repetitions);
                        }
                        result[record.recordName][fakeInputWithFipAndActions.fakeInput][ParseFip(fipAndActions.fakeInputParameter)] = newRleQueue;
                    }
                }
                print($"Record name {record.recordName}");
                foreach (var key in result[record.recordName].Keys)
                {
                    print(key);
                }
            }
            return result;
        }
    
        public string ValidatePacked(List<Record> packed)
        {
            throw new System.NotImplementedException();
        }
        
        private static object ParseFip(string fip)
        {
            if (bool.TryParse(fip, out var boolVariant))
            {
                return boolVariant;
            }
            if (float.TryParse(fip, out var floatVariant))
            {
                return floatVariant;
            }
            if (int.TryParse(fip, out var intVariant))
            {
                return intVariant;
            }
            if (Enum.TryParse<KeyCode>(fip, out var keyCodeVariant))
            {
                return keyCodeVariant;
            }
            return fip;
        }

    }
}
