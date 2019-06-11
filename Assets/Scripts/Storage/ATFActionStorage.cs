using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ATF.Helper;
using ATF.DI;

namespace ATF.Storage
{
    [ATFInjectable]
    public abstract class ATFActionStorage : MonoSingleton<ATFActionStorage>
    {
        public abstract void AddAction(string scenarioName, Action action);
        public abstract Action GetAction(string scenarioName, int actionIndex);
        public abstract void AddActions(string scenarioName, List<Action> actions);
        public abstract List<Action> GetActions(string scenarioName);
    }
}
