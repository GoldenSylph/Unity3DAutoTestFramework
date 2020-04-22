﻿using ATF.Scripts.DI;
using ATF.Scripts.Helper;
using ATF.Scripts.Storage;
using ATF.Scripts.Storage.Interfaces;
using ATF.Scripts.Storage.Utils;
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
            SetRecordingPaused(false);
        }

        public void PlayRecord()
        {
            if (!STORAGE.PrepareToPlayRecord(GetCurrentRecordName())) return;
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
            STORAGE.Enqueue(GetCurrentRecordName(), kind, fakeInputParameter, new AtfAction { Content = input });
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