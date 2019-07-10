using System;
using System.Collections;
using System.Collections.Generic;
using ATF.Scripts.Storage.Interfaces;
using Bedrin.Helper;
using UnityEngine;
using UnityEngine.Serialization;

namespace ATF.Scripts.Storage
{
    public class AtfPlayerPrefsBasedActionStorageSaver : MonoSingleton<AtfPlayerPrefsBasedActionStorageSaver>, IAtfActionStorageSaver
    {

        [Serializable]
        private class FirstSlotDto
        {
            public Dictionary<string, Dictionary<FakeInput, Dictionary<object, Queue<AtfAction>>>> firstSlot;
        }

        [Serializable]
        private class SecondSlotDto
        {
            public string recordName;
            public Dictionary<FakeInput, Dictionary<object, Queue<AtfAction>>> secondSlot;
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
            if (GetCurrentRecordName() != null)
            {
                return GetCurrentRecordName().Equals(allRecordsCodeName) ? thenDo() : elseDo();
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

        public string GetCurrentRecordName()
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
            SecondSlotDtoToSerialize = JsonUtility.FromJson<SecondSlotDto>(PlayerPrefs.GetString(GetCurrentRecordName()));
        }

        public void SaveAll()
        {
            PlayerPrefs.SetString(allRecordsCodeName, JsonUtility.ToJson(FirstSlotDtoToSerialize));
        }

        public void SaveRecord()
        {
            PlayerPrefs.SetString(GetCurrentRecordName(), JsonUtility.ToJson(SecondSlotDtoToSerialize));
        }

        public void SetActions(IEnumerable actionEnumerable)
        {
            IfCurrentRecordNameEqualsToCodeAndNotNull<object>(
                () => {
                    FirstSlotDtoToSerialize = new FirstSlotDto()
                    {
                        firstSlot = AtfDictionaryBasedActionStorage
                                .ReturnNewCopyOf(actionEnumerable as Dictionary<string, Dictionary<FakeInput, Dictionary<object, Queue<AtfAction>>>>)
                    };
                    SaveAll();
                    return null;
                },
                () => {
                    SecondSlotDtoToSerialize = new SecondSlotDto()
                    {
                        secondSlot = AtfDictionaryBasedActionStorage
                            .ReturnNewCopyOf((actionEnumerable as Dictionary<string, Dictionary<FakeInput, Dictionary<object, Queue<AtfAction>>>>)?[GetCurrentRecordName()]),
                        recordName = GetCurrentRecordName()
                    };
                    SaveRecord();
                    return null;
                },
                () => null
            );
        }

        public void SetCurrentRecordName(string recordName)
        {
            currentRecordName = recordName;
        }

        public void ScrapRecord()
        {
            PlayerPrefs.DeleteKey(GetCurrentRecordName());
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
