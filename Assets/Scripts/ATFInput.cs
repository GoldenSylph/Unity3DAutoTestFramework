using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ATF {

    public class ATFInput : BaseInput
    {
        //     Controls enabling and disabling of IME input composition.
        public new static IMECompositionMode imeCompositionMode {
            get
            {
                return Input.imeCompositionMode;
            }

            set
            {
                Input.imeCompositionMode = value;
            }
        }

        //     The current IME composition string being typed by the user.
        public new static string compositionString {
            get
            {
                return Input.compositionString;
            }
        }

        //     Does the user have an IME keyboard input source selected?
        public static bool imeIsSelected
        {
            get
            {
                return Input.imeIsSelected;
            }
        }

        //     The current text input position used by IMEs to open windows.
        public new static Vector2 compositionCursorPos {
            get
            {
                return Input.compositionCursorPos;
            }

            set
            {
                Input.compositionCursorPos = value;
            }
        }

        //     Indicates if a mouse device is detected.
        public new static bool mousePresent
        {
            get
            {
                return Input.mousePresent;
            }
        }

        //     Number of touches. Guaranteed not to change throughout the frame. (Read Only)
        public new static int touchCount
        {
            get
            {
                return Input.touchCount;
            }
        }

        //     Bool value which let's users check if touch pressure is supported.
        public static bool touchPressureSupported
        {
            get
            {
                return Input.touchPressureSupported;
            }
        }

        //     Returns true when Stylus Touch is supported by a device or platform.
        public static bool stylusTouchSupported
        {
            get
            {
                return Input.stylusTouchSupported;
            }
        }

        //     The current mouse scroll delta. (Read Only)
        public new static Vector2 mouseScrollDelta
        {
            get
            {
                return Input.mouseScrollDelta;
            }
        }

        //     Returns whether the device on which application is currently running supports
        //     touch input.
        public new static bool touchSupported
        {
            get
            {
                return Input.touchSupported;
            }
        }

        //     Device physical orientation as reported by OS. (Read Only)
        public static DeviceOrientation deviceOrientation { get; }

        //     Last measured linear acceleration of a device in three-dimensional space. (Read
        //     Only)
        public static Vector3 acceleration
        {
            get
            {
                return Input.acceleration;
            }
        }

        //     This property controls if input sensors should be compensated for screen orientation.
        public static bool compensateSensors
        {
            get
            {
                return Input.compensateSensors;
            }

            set
            {
                Input.compensateSensors = value;
            }
        }

        //     Number of acceleration measurements which occurred during last frame.
        public static int accelerationEventCount
        {
            get
            {
                return Input.accelerationEventCount;
            }
        }

        //     Should Back button quit the application? Only usable on Android, Windows Phone
        //     or Windows Tablets.
        public static bool backButtonLeavesApp
        {
            get
            {
                return Input.backButtonLeavesApp;
            }

            set
            {
                Input.backButtonLeavesApp = value;
            }
        }

        //     Property for accessing device location (handheld devices only). (Read Only)
        public static LocationService location
        {
            get
            {
                return Input.location;
            }
        }

        //     Property for accessing compass (handheld devices only). (Read Only)
        public static Compass compass
        {
            get
            {
                return Input.compass;
            }
        }

        //     Returns default gyroscope.
        public static Gyroscope gyro
        {
            get
            {
                return Input.gyro;
            }
        }

        //     Property indicating whether the system handles multiple touches.
        public static bool multiTouchEnabled
        {
            get
            {
                return Input.multiTouchEnabled;
            }

            set
            {
                Input.multiTouchEnabled = value;
            }
        }

        //     The current mouse position in pixel coordinates. (Read Only)
        public new static Vector3 mousePosition
        {
            get
            {
                return Input.mousePosition;
            }
        }

        //     Returns the keyboard input entered this frame. (Read Only)
        public static string inputString
        {
            get
            {
                return Input.inputString;
            }
        }

        //     Returns true the first frame the user hits any key or mouse button. (Read Only)
        public static bool anyKeyDown
        {
            get
            {
                return Input.anyKeyDown;
            }
        }

        //     Returns list of objects representing status of all touches during last frame.
        //     (Read Only) (Allocates temporary variables).
        public static Touch[] touches
        {
            get
            {
                return Input.touches;
            }
        }

        //     Returns list of acceleration measurements which occurred during the last frame.
        //     (Read Only) (Allocates temporary variables).
        public static AccelerationEvent[] accelerationEvents
        {
            get
            {
                return Input.accelerationEvents;
            }
        }

        //     Is any key or mouse button currently held down? (Read Only)
        public static bool anyKey
        {
            get
            {
                return Input.anyKey;
            }
        }

        //     Enables/Disables mouse simulation with touches. By default this option is enabled.
        public static bool simulateMouseWithTouches
        {
            get
            {
                return Input.simulateMouseWithTouches;
            }
            set
            {
                Input.simulateMouseWithTouches = value;
            }
        }

        //     Returns specific acceleration measurement which occurred during last frame. (Does
        //     not allocate temporary variables).
        //
        // Параметры:
        //   index:
        public static AccelerationEvent GetAccelerationEvent(int index)
        {
            return Input.GetAccelerationEvent(index);
        }

        //     Returns the value of the virtual axis identified by axisName.
        //
        // Параметры:
        //   axisName:
        public static float GetAxis(string axisName)
        {
            return Input.GetAxis(axisName);
        }

        //     Returns the value of the virtual axis identified by axisName with no smoothing
        //     filtering applied.
        //
        // Параметры:
        //   axisName:
        public new static float GetAxisRaw(string axisName)
        {
            return Input.GetAxisRaw(axisName);
        }

        //     Returns true while the virtual button identified by buttonName is held down.
        //
        // Параметры:
        //   buttonName:
        //     The name of the button such as Jump.
        //
        // Возврат:
        //     True when an axis has been pressed and not released.
        public static bool GetButton(string buttonName)
        {
            return Input.GetButton(buttonName);
        }

        //     Returns true during the frame the user pressed down the virtual button identified
        //     by buttonName.
        //
        // Параметры:
        //   buttonName:
        public new static bool GetButtonDown(string buttonName)
        {
            return Input.GetButtonDown(buttonName);
        }

        //     Returns true the first frame the user releases the virtual button identified
        //     by buttonName.
        //
        // Параметры:
        //   buttonName:
        public static bool GetButtonUp(string buttonName)
        {
            return Input.GetButtonUp(buttonName);
        }

        //     Returns an array of strings describing the connected joysticks.
        public static string[] GetJoystickNames()
        {
            return Input.GetJoystickNames();
        }

        //     Returns true while the user holds down the key identified by name.
        //
        // Параметры:
        //   name:
        public static bool GetKey(string name)
        {
            return Input.GetKey(name);
        }

        //     Returns true while the user holds down the key identified by the key KeyCode
        //     enum parameter.
        //
        // Параметры:
        //   key:
        public static bool GetKey(KeyCode key)
        {
            return Input.GetKey(key);
        }

        //     Returns true during the frame the user starts pressing down the key identified
        //     by name.
        //
        // Параметры:
        //   name:
        public static bool GetKeyDown(string name)
        {
            return Input.GetKeyDown(name);
        }

        //     Returns true during the frame the user starts pressing down the key identified
        //     by the key KeyCode enum parameter.
        //
        // Параметры:
        //   key:
        public static bool GetKeyDown(KeyCode key)
        {
            return Input.GetKeyDown(key);
        }

        //     Returns true during the frame the user releases the key identified by the key
        //     KeyCode enum parameter.
        //
        // Параметры:
        //   key:
        public static bool GetKeyUp(KeyCode key)
        {
            return Input.GetKeyUp(key);
        }

        //     Returns true during the frame the user releases the key identified by name.
        //
        // Параметры:
        //   name:
        public static bool GetKeyUp(string name)
        {
            return Input.GetKeyUp(name);
        }

        //     Returns whether the given mouse button is held down.
        //
        // Параметры:
        //   button:
        public new static bool GetMouseButton(int button)
        {
            return Input.GetMouseButton(button);
        }

        //     Returns true during the frame the user pressed the given mouse button.
        //
        // Параметры:
        //   button:
        public new static bool GetMouseButtonDown(int button)
        {
            return Input.GetMouseButtonDown(button);
        }

        //     Returns true during the frame the user releases the given mouse button.
        //
        // Параметры:
        //   button:
        public new static bool GetMouseButtonUp(int button)
        {
            return Input.GetMouseButtonUp(button);
        }

        //     Call Input.GetTouch to obtain a Touch struct.
        //
        // Параметры:
        //   index:
        //     The touch input on the device screen.
        //
        // Возврат:
        //     Touch details in the struct.
        public new static Touch GetTouch(int index)
        {
            return Input.GetTouch(index);
        }

        //     Determine whether a particular joystick model has been preconfigured by Unity.
        //     (Linux-only).
        //
        // Параметры:
        //   joystickName:
        //     The name of the joystick to check (returned by Input.GetJoystickNames).
        //
        // Возврат:
        //     True if the joystick layout has been preconfigured; false otherwise.
        public static bool IsJoystickPreconfigured(string joystickName)
        {
            return Input.IsJoystickPreconfigured(joystickName);
        }

        //     Resets all input. After ResetInputAxes all axes return to 0 and all buttons return
        //     to 0 for one frame.
        public static void ResetInputAxes()
        {
            Input.ResetInputAxes();
        }
    }
}