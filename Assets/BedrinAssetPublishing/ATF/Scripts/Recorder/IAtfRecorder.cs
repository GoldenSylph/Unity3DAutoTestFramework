using ATF.Scripts.Helper;

namespace ATF.Scripts.Recorder
{
    public interface IAtfRecorder : IAtfGetSetRecordName
    {
        bool IsRecording();
        bool IsPlaying();

        bool IsRecordingPaused();
        bool IsPlayPaused();

        bool IsInputStopped();

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
        void SetInputStopped(bool value);

        void Record(FakeInput kind, object input, object fakeInputParameter);
        object GetLastInput(FakeInput kind, object fakeInputParameter);
        void SetLastInput(FakeInput kind, object realInput, object fakeInputParameter);
    }
}
