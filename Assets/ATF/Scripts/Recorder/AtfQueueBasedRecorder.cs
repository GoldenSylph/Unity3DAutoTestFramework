using ATF.Scripts.Storage;
using ATF.Scripts.Storage.Interfaces;
using Bedrin.DI;
using Bedrin.Helper;
using UnityEngine;
using UnityEngine.Serialization;

namespace ATF.Scripts.Recorder
{
    [Injectable]
    public class AtfQueueBasedRecorder : MonoSingleton<AtfQueueBasedRecorder>, IAtfRecorder
    {
        [Inject(typeof(AtfDictionaryBasedActionStorage))]
        // ReSharper disable once UnassignedReadonlyField
        // ReSharper disable once MemberCanBePrivate.Global
        public static readonly IAtfActionStorage Storage;

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
        private string currentRecordingName;

        public string GetCurrentRecordName()
        {
            return currentRecordingName ?? (currentRecordingName = "DefaultRecord");
        }

        public void SetCurrentRecordName(string value)
        {
            currentRecordingName = value;
        }

        public void Initialize()
        {
            SetRecording(false);
            SetPlaying(false);
            SetRecordingPaused(false);
            SetPlayPaused(false);
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
            SetRecordingPaused(false);
        }

        public void PlayRecord()
        {
            if (!Storage.PrepareToPlayRecord(GetCurrentRecordName())) return;
            SetRecording(false);
            SetPlaying(true);
        }

        public void StartRecord()
        {
            SetRecording(true);
            SetPlaying(false);
        }

        public void Record(FakeInput kind, object input, object fakeInputParameter)
        {
            Storage.Enqueue(GetCurrentRecordName(), kind, fakeInputParameter, new AtfAction { Content = input });
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
            SetPlayPaused(false);
        }

        public void StopPlay()
        {
            SetPlaying(false);
            SetRecording(false);
            Storage.ClearPlayStorage();
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
