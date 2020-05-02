using ATF.Scripts.DI;
using ATF.Scripts.Helper;
using ATF.Scripts.Storage;
using ATF.Scripts.Storage.Interfaces;
using ATF.Scripts.Storage.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace ATF.Scripts.Recorder
{
    [Injectable]
    [AtfSystem]
    public class AtfQueueBasedRecorder : MonoSingleton<AtfQueueBasedRecorder>, IAtfRecorder
    {
        [Inject(typeof(AtfDictionaryBasedActionStorage))]
        // ReSharper disable once UnassignedReadonlyField
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once InconsistentNaming
        public static readonly IAtfActionStorage STORAGE;

        private const string LAST_INPUT_RECORD_NAME = "Last input";
        private const string DEFAULT_RECORD_NAME = "DefaultRecord";
        
        [Header("Debug Settings:")]
        
        [SerializeField]
        private bool recording;

        [SerializeField]
        private bool playing;

        [SerializeField]
        private bool recordingPaused;

        [SerializeField]
        private bool playPaused;

        [SerializeField]
        private bool inputStopped;
        
        [SerializeField]
        private string currentRecordingName;
        
        [MenuItem("Tools/ATF/Utils/Toggle Input %i")]
        public static void ChangeInputStopped()
        {
            if (EditorApplication.isPlaying)
            {
                Instance.SetInputStopped(!Instance.IsInputStopped());
                print($"Input Stopped: {Instance.IsInputStopped()}");
            }
            else
            {
                print("Please enter the Play mode first.");
            }
        }
        
        public string GetCurrentRecordName()
        {
            return currentRecordingName ?? (currentRecordingName = DEFAULT_RECORD_NAME);
        }

        public void SetCurrentRecordName(string value)
        {
            if (value.Equals(LAST_INPUT_RECORD_NAME))
            {
                Debug.LogError($"Please choose another name for your record because the name '{value}' is already present.");
            }
            else
            {
                currentRecordingName = value;
            }
        }

        public override void Initialize()
        {
            SetRecording(false);
            SetPlaying(false);
            SetRecordingPaused(false);
            SetPlayPaused(false);
            base.Initialize();
        }

        public bool IsPlaying()
        {
            return playing;
        }
        
        public bool IsRecording()
        {
            return recording;
        }

        public void SetRecording(bool value)
        {
            recording = value;
        }

        public void SetPlaying(bool value)
        {
            playing = value;
        }

        public void PauseRecord()
        {
            SetRecordingPaused(true);
        }

        public void ContinueRecord()
        {
            SetInputStopped(false);
            SetRecordingPaused(false);
        }

        public bool IsInputStopped()
        {
            return inputStopped;
        }

        public void PlayRecord()
        {
            SetInputStopped(false);
            if (!STORAGE.PrepareToPlayRecord(GetCurrentRecordName())) return;
            SetRecording(false);
            SetPlaying(true);
        }

        public void StartRecord()
        {
            SetInputStopped(false);
            SetRecording(true);
            SetPlaying(false);
        }

        public void SetInputStopped(bool value)
        {
            inputStopped = value;
        }

        public void Record(FakeInput kind, object input, object fakeInputParameter)
        {
            STORAGE.Enqueue(GetCurrentRecordName(), kind, fakeInputParameter, new AtfAction { Content = input });
        }

        public object GetLastInput(FakeInput kind, object fakeInputParameter)
        {
            return STORAGE.Peek(LAST_INPUT_RECORD_NAME, kind, fakeInputParameter)?.Content;
        }

        public void SetLastInput(FakeInput kind, object realInput, object fakeInputParameter)
        {
            if (GetLastInput(kind, fakeInputParameter) != null)
            {
                STORAGE.Dequeue(LAST_INPUT_RECORD_NAME, kind, fakeInputParameter);
            }
            STORAGE.Enqueue(LAST_INPUT_RECORD_NAME, kind, fakeInputParameter, new AtfAction {Content = realInput});   
        }

        public void StopRecord()
        {
            SetRecording(false);
            SetPlaying(false);
            SetRecordingPaused(false);
        }

        public void PausePlay()
        {
            SetPlayPaused(true);
        }

        public void ContinuePlay()
        {
            SetInputStopped(false);
            SetPlayPaused(false);
        }

        public void StopPlay()
        {
            SetPlaying(false);
            SetRecording(false);
            STORAGE.ClearPlayStorage();
            SetPlayPaused(false);
        }

        public bool IsRecordingPaused()
        {
            return recordingPaused;
        }

        public bool IsPlayPaused()
        {
            return playPaused;
        }

        public void SetRecordingPaused(bool value)
        {
            recordingPaused = value;
        }

        public void SetPlayPaused(bool value)
        {
            playPaused = value;
        }
    }
}
