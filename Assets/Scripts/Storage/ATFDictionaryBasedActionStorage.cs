using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bedrin.Helper;
using ATF.InputTest;
using Bedrin.DI;

namespace ATF.Storage
{
    [Injectable]
    public class ATFDictionaryBasedActionStorage : MonoSingleton<ATFDictionaryBasedActionStorage>, IATFActionStorage
    {
        [Inject]
        public Mover mover;

        private Dictionary<string, List<Action>> Storage;

        public void Initialize()
        {
            Storage = new Dictionary<string, List<Action>>();
        }

        public void AddAction(string scenarioName, Action action)
        {
            if (Storage[scenarioName] == null)
            {
                Storage[scenarioName] = new List<Action>();
            }
            Storage[scenarioName].Add(action);
        }

        public Action GetAction(string scenarioName, int actionIndex)
        {
            return Storage[scenarioName][actionIndex];
        }

        public void AddActions(string scenarioName, List<Action> actions)
        {
            Storage[scenarioName] = actions;
        }

        public List<Action> GetActions(string scenarioName)
        {
            return Storage[scenarioName];
        }

    }
}