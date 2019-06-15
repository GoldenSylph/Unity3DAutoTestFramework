using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using ATF.Storage;
using ATF.Recorder;
using Bedrin.DI;
using System;

namespace ATF {

    public enum FakeInput {
        ANY_KEY_DOWN, ANY_KEY, GET_AXIS, GET_AXIS_RAW, GET_BUTTON,
        GET_BUTTON_DOWN, GET_BUTTON_UP, GET_KEY, GET_KEY_DOWN, GET_KEY_UP,
        GET_MOUSE_BUTTON, GET_MOUSE_BUTTON_DOWN, GET_MOUSE_BUTTON_UP
    }

    [Serializable]
    [Injectable]
    public class ATFInput : BaseInput
    {
        [Inject(typeof(ATFCoroutineBasedRecorder))]
        public static readonly IATFRecorder RECORDER;

        [Inject(typeof(ATFDictionaryBasedActionStorage))]
        public static readonly IATFActionStorage STORAGE;

        private static object RealOrFakeInputOrRecord(object realInput, object fakeInput)
        {
            if (RECORDER.IsPlaying() && !RECORDER.IsRecording())
            {
                return fakeInput;
            }
            else if (!RECORDER.IsPlaying() && RECORDER.IsRecording())
            {
                RECORDER.Record(realInput);
                return realInput;
            }
            else if (!RECORDER.IsPlaying() && !RECORDER.IsRecording())
            {
                return realInput;
            } else
            {
                return null;
            }
        }

        private static T IfExceptionReturnDefault<T>(Func<T> function, T defaultValue)
        {
            try
            {
                return function();
            } catch (Exception)
            {
                return defaultValue;
            }
        }

        private static object GetCurrentFakeInput(FakeInput inputKind)
        {
            return STORAGE.GetContentOfRecordingAndType(RECORDER.GetCurrentRecordingName(), inputKind);
        }

        private static T Intercept<T>(object realInput, FakeInput fakeInputKind, T defaultValue)
        {
            return IfExceptionReturnDefault<T>(() => (T) RealOrFakeInputOrRecord(realInput, GetCurrentFakeInput(fakeInputKind)), defaultValue);
        }

        public static bool anyKeyDown
        {
            get
            {
                return Intercept(Input.anyKeyDown, FakeInput.ANY_KEY_DOWN, false);
            }
        }

        public static bool anyKey
        {
            get
            {
                return Intercept(Input.anyKey, FakeInput.ANY_KEY, false);
            }
        }

        public static float GetAxis(string axisName)
        {
            return Intercept(Input.GetAxis(axisName), FakeInput.GET_AXIS, 0f);
        }

        public new static float GetAxisRaw(string axisName)
        {
            return Intercept(Input.GetAxisRaw(axisName), FakeInput.GET_AXIS_RAW, 0f);
        }

        public static bool GetButton(string buttonName)
        {
            return Intercept(Input.GetButton(buttonName), FakeInput.GET_BUTTON, false);
        }

        public new static bool GetButtonDown(string buttonName)
        {
            return Intercept(Input.GetButtonDown(buttonName), FakeInput.GET_BUTTON_DOWN, false);
        }

        public static bool GetButtonUp(string buttonName)
        {
            return Intercept(Input.GetButtonUp(buttonName), FakeInput.GET_BUTTON_UP, false);
        }

        public static bool GetKey(string name)
        {
            return Intercept(Input.GetKey(name), FakeInput.GET_KEY, false);
        }

        public static bool GetKey(KeyCode key)
        {
            return Intercept(Input.GetKey(key), FakeInput.GET_KEY, false);
        }

        public static bool GetKeyDown(string name)
        {
            return Intercept(Input.GetKeyDown(name), FakeInput.GET_KEY_DOWN, false);
        }

        public static bool GetKeyDown(KeyCode key)
        {
            return Intercept(Input.GetKeyDown(key), FakeInput.GET_KEY_DOWN, false);
        }

        public static bool GetKeyUp(KeyCode key)
        {
            return Intercept(Input.GetKeyUp(key), FakeInput.GET_KEY_UP, false);
        }

        public static bool GetKeyUp(string name)
        {
            return Intercept(Input.GetKeyUp(name), FakeInput.GET_KEY_UP, false);
        }

        public new static bool GetMouseButton(int button)
        {
            return Intercept(Input.GetMouseButton(button), FakeInput.GET_MOUSE_BUTTON, false);
        }

        public new static bool GetMouseButtonDown(int button)
        {
            return Intercept(Input.GetMouseButtonDown(button), FakeInput.GET_MOUSE_BUTTON_DOWN, false);
        }

        public new static bool GetMouseButtonUp(int button)
        {
            return Intercept(Input.GetMouseButtonUp(button), FakeInput.GET_MOUSE_BUTTON_UP, false);
        }

  
    }
}