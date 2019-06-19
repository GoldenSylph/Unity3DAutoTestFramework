using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bedrin.DI;
using Bedrin.Helper;
using System;

namespace ATF.Storage
{
    public class ATFPlayerPrefsBasedActionStorageSaver : MonoSingleton<ATFPlayerPrefsBasedActionStorageSaver>, IATFActionStorageSaver
    {

        [Serializable]
        private class FirstSlotDTO
        {
            public Dictionary<string, Dictionary<FakeInput, Queue<Action>>> FirstSlot;
        }

        [Serializable]
        private class SecondSlotDTO
        {
            public string RecordName;
            public Dictionary<FakeInput, Queue<Action>> SecondSlot;
        }

        [Header("Debug Settings")]
        [SerializeField]
        private string AllRecordsCodeName;

        [SerializeField]
        private string CurrentRecordName;

        private FirstSlotDTO FirstSlotDTOToSerialize;
        private SecondSlotDTO SecondSlotDTOToSerialize;
        
        private T IfCurrentRecordNameEqualsToCodeAndNotNull<T>(Func<T> thenDo, Func<T> elseDo, Func<T> ifNullDo)
        {
            if (GetRecordName() != null)
            {
                if (GetRecordName().Equals(AllRecordsCodeName))
                {
                    return thenDo();
                }
                else
                {
                    return elseDo();
                }
            }
            else
            {
                return ifNullDo();
            }
        }

        public IEnumerable GetActions()
        {
            return IfCurrentRecordNameEqualsToCodeAndNotNull<IEnumerable>(
                () => {
                    LoadAll();
                    return FirstSlotDTOToSerialize.FirstSlot;
                },
                () => {
                    LoadRecord();
                    return SecondSlotDTOToSerialize.SecondSlot;
                },
                () => null
            );
        }

        public string GetRecordName()
        {
            return CurrentRecordName;
        }

        public void Initialize()
        {
            AllRecordsCodeName = "__ALL__";
        }

        public void LoadAll()
        {
            FirstSlotDTOToSerialize = JsonUtility.FromJson<FirstSlotDTO>(PlayerPrefs.GetString(AllRecordsCodeName));
        }

        public void LoadRecord()
        {
            SecondSlotDTOToSerialize = JsonUtility.FromJson<SecondSlotDTO>(PlayerPrefs.GetString(GetRecordName()));
        }

        public void SaveAll()
        {
            PlayerPrefs.SetString(AllRecordsCodeName, JsonUtility.ToJson(FirstSlotDTOToSerialize));
        }

        public void SaveRecord()
        {
            PlayerPrefs.SetString(GetRecordName(), JsonUtility.ToJson(SecondSlotDTOToSerialize));
        }

        public void SetActions(IEnumerable actionEnumerable)
        {
            IfCurrentRecordNameEqualsToCodeAndNotNull<object>(
                () => {
                    FirstSlotDTOToSerialize = new FirstSlotDTO()
                    {
                        FirstSlot = ATFDictionaryBasedActionStorage
                                .ReturnNewCopyOf(actionEnumerable as Dictionary<string, Dictionary<FakeInput, Queue<Action>>>)
                    };
                    SaveAll();
                    return null;
                },
                () => {
                    SecondSlotDTOToSerialize = new SecondSlotDTO()
                    {
                        SecondSlot = ATFDictionaryBasedActionStorage
                            .ReturnNewCopyOf((actionEnumerable as Dictionary<string, Dictionary<FakeInput, Queue<Action>>>)[GetRecordName()]),
                        RecordName = GetRecordName()
                    };
                    SaveRecord();
                    return null;
                },
                () => null
            );
        }

        public void SetRecordName(string recordName)
        {
            CurrentRecordName = recordName;
        }

        public void ScrapRecord()
        {
            PlayerPrefs.DeleteKey(GetRecordName());
        }

        public void ScrapAll()
        {
            PlayerPrefs.DeleteKey(AllRecordsCodeName);
        }

        public void ScrapSavedActions()
        {
            IfCurrentRecordNameEqualsToCodeAndNotNull<object>(
                () => {
                    ScrapAll();
                    return null;
                },
                () => {
                    ScrapRecord();
                    return null;
                }, 
                () => null
            );
        }
    }
}
