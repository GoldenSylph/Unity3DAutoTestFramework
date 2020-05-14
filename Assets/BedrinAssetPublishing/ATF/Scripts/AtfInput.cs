using System;
using ATF.Scripts.DI;
using ATF.Scripts.Helper;
using ATF.Scripts.Recorder;
using ATF.Scripts.Storage;
using ATF.Scripts.Storage.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ATF.Scripts {

    public enum FakeInput {
        NONE,
        ANY_KEY_DOWN,
        ANY_KEY,
        GET_AXIS,
        GET_AXIS_RAW,
        GET_BUTTON,
        GET_BUTTON_DOWN,
        GET_BUTTON_UP,
        GET_KEY,
        GET_KEY_DOWN,
        GET_KEY_UP,
        GET_MOUSE_BUTTON,
        GET_MOUSE_BUTTON_DOWN,
        GET_MOUSE_BUTTON_UP,
        GET_TOUCH,
        MOUSE_POSITION,
        TOUCH_COUNT,
        MOUSE_SCROLL_DELTA,
        TOUCH_SUPPORTED,
        COMPOSITION_STRING,
        IME_COMPOSITION_MODE,
        COMPOSITION_CURSOR_POS,
        MOUSE_PRESENT,
        SIMULATE_MOUSE_WITH_TOUCHES
    }

    [AtfSystem]
    [Serializable]
    [Injectable]
    public class AtfInput : MonoSingleton<AtfInput>, IBaseInput
    {
        [Inject(typeof(AtfQueueBasedRecorder))]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnassignedReadonlyField
        // ReSharper disable once InconsistentNaming
        public static readonly IAtfRecorder RECORDER;

        [Inject(typeof(AtfDictionaryBasedActionStorage))]
        // ReSharper disable once UnassignedReadonlyField
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once InconsistentNaming
        public static readonly IAtfActionStorage STORAGE;

        [Inject(typeof(BaseInput))]
        // ReSharper disable once UnassignedReadonlyField
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once InconsistentNaming
        public static readonly BaseInput BASE_INPUT;

        private static object RealOrFakeInputOrRecord(object realInput, object fakeInputParameter, FakeInput kind)
        {

            if (RECORDER.IsPlaying())
            {
                if (RECORDER.IsInputStopped())
                {
                    print("Input now cannot be stopped. Please uncheck the option.");
                }
                if (!RECORDER.IsPlayPaused())
                {
                    try
                    {
                        realInput = GetCurrentFakeInput(kind, fakeInputParameter);
                    }
                    catch (Exception e)
                    {
                        if (AtfInitializer.Instance.isDebugPrintOn) print(e);
                    }
                    return realInput;
                }
                else
                {
                    return realInput;
                }
            }
            else
            {
                if (RECORDER.IsRecording())
                {
                    if (RECORDER.IsInputStopped())
                    {
                        realInput = RECORDER.GetLastInput(kind, fakeInputParameter);
                    }
                    if (!RECORDER.IsRecordingPaused())
                    {
                        RECORDER.Record(kind, realInput, fakeInputParameter);
                    }
                    return realInput;
                }
                else
                {
                    if (RECORDER.IsInputStopped())
                    {
                        return RECORDER.GetLastInput(kind, fakeInputParameter);
                    }
                    else
                    {
                        RECORDER.SetLastInput(kind, realInput, fakeInputParameter);
                        return realInput;
                    }
                }
            }
        }

        private static object GetCurrentFakeInput(FakeInput inputKind, object fakeInputParameter)
        {
            return STORAGE.GetPartOfRecord(inputKind, fakeInputParameter);
        }

        private static T Intercept<T>(T realInput, FakeInput fakeInputKind, object fakeInputParameter = null)
        {
            if (fakeInputParameter == null)
            {
                fakeInputParameter = "EMPTY FAKE INPUT PARAMETER";
            }
            dynamic result = RealOrFakeInputOrRecord(realInput, fakeInputParameter, fakeInputKind);
            if (result is string)
            {
                Debug.Log(result);
            }
            return result;
        }
        
        public static bool simulateMouseWithTouches
        {
            get { return Intercept(Input.simulateMouseWithTouches, FakeInput.SIMULATE_MOUSE_WITH_TOUCHES, "Simulate mouse with touches"); }
            set { Input.simulateMouseWithTouches = value; }
        }
        
        public static bool AnyKeyDown => Intercept(Input.anyKeyDown, FakeInput.ANY_KEY_DOWN);

        public static bool AnyKey => Intercept(Input.anyKey, FakeInput.ANY_KEY);

        public static float GetAxis(string axisName)
        {
            return Intercept(Input.GetAxis(axisName), FakeInput.GET_AXIS, axisName);
        }

        Touch IBaseInput.GetTouch(int index)
        {
            return GetTouch(index);
        }

        float IBaseInput.GetAxisRaw(string axisName)
        {
            return GetAxisRaw(axisName);
        }

        bool IBaseInput.GetButtonDown(string buttonName)
        {
            return GetButtonDown(buttonName);
        }

        public new static float GetAxisRaw(string axisName)
        {
            return Intercept(Input.GetAxisRaw(axisName), FakeInput.GET_AXIS_RAW, axisName);
        }

        public static bool GetButton(string buttonName)
        {
            return Intercept(Input.GetButton(buttonName), FakeInput.GET_BUTTON, buttonName);
        }

        public new static bool GetButtonDown(string buttonName)
        {
            return Intercept(Input.GetButtonDown(buttonName), FakeInput.GET_BUTTON_DOWN, buttonName);
        }

        public static bool GetButtonUp(string buttonName)
        {
            return Intercept(Input.GetButtonUp(buttonName), FakeInput.GET_BUTTON_UP, buttonName);
        }

        public static bool GetKey(string name)
        {
            return Intercept(Input.GetKey(name), FakeInput.GET_KEY, name);
        }

        public static bool GetKey(KeyCode key)
        {
            return Intercept(Input.GetKey(key), FakeInput.GET_KEY, key);
        }

        public static bool GetKeyDown(string name)
        {
            return Intercept(Input.GetKeyDown(name), FakeInput.GET_KEY_DOWN, name);
        }

        public static bool GetKeyDown(KeyCode key)
        {
            return Intercept(Input.GetKeyDown(key), FakeInput.GET_KEY_DOWN, key);
        }

        public static bool GetKeyUp(KeyCode key)
        {
            return Intercept(Input.GetKeyUp(key), FakeInput.GET_KEY_UP, key);
        }

        public static bool GetKeyUp(string name)
        {
            return Intercept(Input.GetKeyUp(name), FakeInput.GET_KEY_UP, name);
        }

        bool IBaseInput.GetMouseButtonUp(int button)
        {
            return GetMouseButtonUp(button);
        }

        bool IBaseInput.GetMouseButton(int button)
        {
            return GetMouseButton(button);
        }

        public Vector3 mousePosition
        {
            get
            {
                var intercepted = Intercept(BASE_INPUT.mousePosition, FakeInput.MOUSE_POSITION, "Mouse");
                return new Vector3(intercepted.x, intercepted.y, 0);
            }
        }

        public Vector2 mouseScrollDelta
        {
            get
            {
                return Intercept(BASE_INPUT.mouseScrollDelta, FakeInput.MOUSE_SCROLL_DELTA, "Mouse scroll delta");
            }
        }

        public bool touchSupported
        {
            get { return Intercept(BASE_INPUT.touchSupported, FakeInput.TOUCH_SUPPORTED, "Touch supported"); }
        }

        public int touchCount
        {
            get { return Intercept(BASE_INPUT.touchCount, FakeInput.TOUCH_COUNT, "Touch count"); }
        }

        public static bool GetMouseButton(int button)
        {
            return Intercept(Input.GetMouseButton(button), FakeInput.GET_MOUSE_BUTTON, button);
        }

        public string compositionString
        {
            get { return Intercept(BASE_INPUT.compositionString, FakeInput.COMPOSITION_STRING, "Composition string"); }
        }

        public IMECompositionMode imeCompositionMode
        {
            get { return Intercept(BASE_INPUT.imeCompositionMode, FakeInput.IME_COMPOSITION_MODE, "IME Composition mode"); }
            set { BASE_INPUT.imeCompositionMode = value; }
        }

        public Vector2 compositionCursorPos
        {
            get { return Intercept(BASE_INPUT.compositionCursorPos, FakeInput.COMPOSITION_CURSOR_POS, "Composition Cursor Pos"); }
            set { BASE_INPUT.compositionCursorPos = value; }
        }

        public bool mousePresent
        {
            get { return Intercept(BASE_INPUT.mousePresent, FakeInput.MOUSE_PRESENT, "Mouse present"); }
        }

        public static bool GetMouseButtonDown(int button)
        {
            return Intercept(Input.GetMouseButtonDown(button), FakeInput.GET_MOUSE_BUTTON_DOWN, button);
        }

        bool IBaseInput.GetMouseButtonDown(int button)
        {
            return GetMouseButtonDown(button);
        }

        public static bool GetMouseButtonUp(int button)
        {
            return Intercept(Input.GetMouseButtonUp(button), FakeInput.GET_MOUSE_BUTTON_UP, button);
        }

        public static Touch GetTouch(int index)
        {
            return Intercept(Input.GetTouch(index), FakeInput.GET_TOUCH, index);
        }
    }
}