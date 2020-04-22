using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ATF.Scripts.Storage.Utils.Nito.Collections;
using UnityEngine;

namespace ATF.Scripts.Storage.Utils
{
    [Serializable]
    public class AtfActionRleQueue : Queue<AtfAction>
    {
        public Deque<int> rleCounts;
        public AtfAction last;

        public AtfActionRleQueue(AtfActionRleQueue initialItems) : base(initialItems)
        {
            rleCounts = new Deque<int>(initialItems.rleCounts);
            last = new AtfAction(initialItems.last.GetDeserialized());
        }

        public AtfActionRleQueue()
        {
            rleCounts = new Deque<int>();
        }

        public new int Count => rleCounts.Sum() + base.Count;

        public AtfActionRleQueue ConvertToDeserialized()
        {
            foreach (var action in this)
            {
                action.GetDeserialized();
            }
            return this;
        }

        public int GetRepetitions(int index)
        {
            return rleCounts[index];
        }

        public void EnqueueWithoutOptimization(AtfAction action)
        {
            base.Enqueue(action);
        }
        
        public new void Enqueue(AtfAction action)
        {
            if (Count > 0 && last.serializedContent.Equals(action.serializedContent))
            {
                var previousCount = rleCounts.RemoveFromBack();
                rleCounts.AddToBack(previousCount + 1);
            }
            else
            {
                rleCounts.AddToBack(0);
                last = action;
                base.Enqueue(action);
            }
        }
            
        public new AtfAction Dequeue()
        {
            var previousCount = rleCounts.RemoveFromFront();
            if (previousCount == 0)
            {
                var toRemove = base.Dequeue();
                return toRemove;
            }
            rleCounts.AddToFront(previousCount - 1);
            return Peek();
        }

    }
}