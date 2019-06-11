using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ATF.Helper;

namespace ATF.Storage
{
    public class ATFDictionaryBasedActionStorage : ATFActionStorage
    {

        private readonly Dictionary<string, List<Action>> Storage = new Dictionary<string, List<Action>>();

        public override void AddAction(string scenarioName, Action action)
        {
            if (Storage[scenarioName] == null)
            {
                Storage[scenarioName] = new List<Action>();
            }
            Storage[scenarioName].Add(action);
        }

        public override Action GetAction(string scenarioName, int actionIndex)
        {
            return Storage[scenarioName][actionIndex];
        }

        public override void AddActions(string scenarioName, List<Action> actions)
        {
            Storage[scenarioName] = actions;
        }

        public override List<Action> GetActions(string scenarioName)
        {
            return Storage[scenarioName];
        }

    }
}