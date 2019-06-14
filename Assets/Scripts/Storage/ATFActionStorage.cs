using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bedrin.Helper;
using Bedrin.DI;

namespace ATF.Storage
{
    public interface IATFActionStorage : IATFInitializable
    {
        void AddAction(string scenarioName, Action action);
        Action GetAction(string scenarioName, int actionIndex);
        void AddActions(string scenarioName, List<Action> actions);
        List<Action> GetActions(string scenarioName);
    }
}
