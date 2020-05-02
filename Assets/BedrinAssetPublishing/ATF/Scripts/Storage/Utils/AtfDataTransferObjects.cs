using System;
using System.Collections.Generic;
using System.Linq;

namespace ATF.Scripts.Storage.Utils
{

    [Serializable]
    public class Metadata
    {
        public AtfAction action;
        public int repetitions;
    }
    
    [Serializable]
    public class FipAndActions
    {
        public string fakeInputParameter;
        public List<Metadata> metadata;
        public AtfAction last;
    }

    [Serializable]
    public class FakeInputWithFipAndActions
    {
        public FakeInput fakeInput;
        public List<FipAndActions> fipsAndActions;

        public FipAndActions FindFipAndActionsByFip(object fip)
        {
            return fipsAndActions.Find((e) => e.fakeInputParameter.Equals(fip));
        }

        public IEnumerable<object> GetAllFips()
        {
            return fipsAndActions?.ConvertAll((e) => e.fakeInputParameter);
        }

    }

    [Serializable]
    public class Record
    {
        public string recordName;
        public List<FakeInputWithFipAndActions> fakeInputsWithFipsAndActions;

        public FakeInputWithFipAndActions FindFakeInputWithFipAndActionsByKind(FakeInput kind)
        {
            return fakeInputsWithFipsAndActions?.Find((element) => element.fakeInput.Equals(kind));
        }

        public IEnumerable<FakeInput> GetAllFakeInputs()
        {
            return fakeInputsWithFipsAndActions?.ConvertAll((e) => e.fakeInput);
        }

        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            return recordName.GetHashCode();
        }

#pragma warning disable 659
        public override bool Equals(object obj)
#pragma warning restore 659
        {
            var temp = obj as Record;
            if (temp != null)
            {
                return temp.recordName.Equals(recordName);
            }
            return false;
            // return obj is Record record && record.recordName.Equals(recordName);
        }
    }

    [Serializable]
    public class Slot
    {
        public List<Record> content;

        public Record FindRecordByName(string name)
        {
            return content?.Find((r) => r.recordName.Equals(name));
        }

        public IEnumerable<string> GetAllRecordNames()
        {
            return content?.ConvertAll((record) => record.recordName);
        }

        public static Slot Merge(Slot a, Slot b)
        {
            return new Slot {content = a.content.Union(b.content).ToList()};
        }
    }
}