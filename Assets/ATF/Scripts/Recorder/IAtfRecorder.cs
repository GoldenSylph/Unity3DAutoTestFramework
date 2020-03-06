using ATF.Scripts.Helper;

namespace ATF.Scripts.Recorder
{
    public interface IAtfRecorder : IAtfGetSetRecordName
    {
        bool IsRecording();
        bool IsPlaying();

        bool IsRecordingPaused();
        bool IsPlayPaused();

        void PlayRecord();
        void PausePlay();
        void ContinuePlay();
        void StopPlay();

        void StartRecord();
        void PauseRecord();
        void ContinueRecord();
        void StopRecord();

        void SetRecording(bool value);
        void SetPlaying(bool value);
        void SetRecordingPaused(bool value);
        void SetPlayPaused(bool value);

        void Record(FakeInput kind, object input, object fakeInputParameter);
    }
}
