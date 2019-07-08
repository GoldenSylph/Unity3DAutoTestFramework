using System;
using System.Collections;
using System.Collections.Generic;
using ATF.Scripts.Storage.Interfaces;
using Bedrin.Helper;
using UnityEngine;
using UnityEngine.Serialization;
using Action = ATF.Scripts.Storage.Action;

namespace ATF.Scripts.Storage
{
    public class ATFPlayerPrefsBasedActionStorageSaver : MonoSingleton<ATFPlayerPrefsBasedActionStorageSaver>, IATFActionStorageSaver
    {

        [Serializable]
        private class FirstSlotDto
        {
            public Dictionary<string, Dictionary<FakeInput, Dictionary<object, Queue<Action>>>> firstSlot;
        }

        [Serializable]
        private class SecondSlotDto
        {
            public string recordName;
            public Dictionary<FakeInput, Dictionary<object, Queue<Action>>> secondSlot;
        }

        [Header("Debug Settings")]
        [SerializeField]
        private string allRecordsCodeName;

        [SerializeField]
        private string currentRecordName;

        private FirstSlotDto FirstSlotDtoToSerialize;
        private SecondSlotDto SecondSlotDtoToSerialize;
        
        private T IfCurrentRecordNameEqualsToCodeAndNotNull<T>(Func<T> thenDo, Func<T> elseDo, Func<T> ifNullDo)
        {
            if (GetRecordName() != null)
            {
                return GetRecordName().Equals(allRecordsCodeName) ? thenDo() : elseDo();
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
                    return FirstSlotDtoToSerialize?.firstSlot;
                },
                () => {
                    LoadRecord();
                    return SecondSlotDtoToSerialize?.secondSlot;
                },
                () => null
            );
        }

        public string GetRecordName()
        {
            return currentRecordName;
        }

        public void Initialize()
        {
            allRecordsCodeName = "__ALL__";
        }

        public void LoadAll()
        {
            FirstSlotDtoToSerialize = JsonUtility.FromJson<FirstSlotDto>(PlayerPrefs.GetString(allRecordsCodeName));
        }

        public void LoadRecord()
        {
            SecondSlotDtoToSerialize = JsonUtility.FromJson<SecondSlotDto>(PlayerPrefs.GetString(GetRecordName()));
        }

        public void SaveAll()
        {
            PlayerPrefs.SetString(allRecordsCodeName, JsonUtility.ToJson(FirstSlotDtoToSerialize));
        }

        public void SaveRecord()
        {
            PlayerPrefs.SetString(GetRecordName(), JsonUtility.ToJson(SecondSlotDtoToSerialize));
        }

        public void SetActions(IEnumerable actionEnumerable)
        {
            IfCurrentRecordNameEqualsToCodeAndNotNull<object>(
                () => {
                    FirstSlotDtoToSerialize = new FirstSlotDto()
                    {
                        firstSlot = ATFDictionaryBasedActionStorage
                                .ReturnNewCopyOf(actionEnumerable as Dictionary<string, Dictionary<FakeInput, Dictionary<object, Queue<Action>>>>)
                    };
                    SaveAll();
                    return null;
                },
                () => {
                    SecondSlotDtoToSerialize = new SecondSlotDto()
                    {
                        secondSlot = ATFDictionaryBasedActionStorage
                            .ReturnNewCopyOf((actionEnumerable as Dictionary<string, Dictionary<FakeInput, Dictionary<object, Queue<Action>>>>)?[GetRecordName()]),
                        recordName = GetRecordName()
                    };
                    SaveRecord();
                    return null;
                },
                () => null
            );
        }

        public void SetRecordName(string recordName)
        {
            currentRecordName = recordName;
        }

        public void ScrapRecord()
        {
            PlayerPrefs.DeleteKey(GetRecordName());
        }

        public void ScrapAll()
        {
            PlayerPrefs.DeleteKey(allRecordsCodeName);
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
