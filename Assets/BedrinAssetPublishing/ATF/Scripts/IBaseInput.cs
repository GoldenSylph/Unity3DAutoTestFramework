using UnityEngine;

namespace ATF.Scripts
{
	public interface IBaseInput {
	    string compositionString { get; }

	    /// <summary>
	    ///   <para>Interface to Input.imeCompositionMode. Can be overridden to provide custom input instead of using the Input class.</para>
	    /// </summary>
	    IMECompositionMode imeCompositionMode { get; set; }

	    /// <summary>
	    ///   <para>Interface to Input.compositionCursorPos. Can be overridden to provide custom input instead of using the Input class.</para>
	    /// </summary>
	    Vector2 compositionCursorPos { get; set; }

	    /// <summary>
	    ///   <para>Interface to Input.mousePresent. Can be overridden to provide custom input instead of using the Input class.</para>
	    /// </summary>
	    bool mousePresent { get; }

	    /// <summary>
	    ///   <para>Interface to Input.GetMouseButtonDown. Can be overridden to provide custom input instead of using the Input class.</para>
	    /// </summary>
	    /// <param name="button"></param>
	    bool GetMouseButtonDown(int button);

	    /// <summary>
	    ///   <para>Interface to Input.GetMouseButtonUp. Can be overridden to provide custom input instead of using the Input class.</para>
	    /// </summary>
	    /// <param name="button"></param>
	    bool GetMouseButtonUp(int button);

	    /// <summary>
	    ///   <para>Interface to Input.GetMouseButton. Can be overridden to provide custom input instead of using the Input class.</para>
	    /// </summary>
	    /// <param name="button"></param>
	    bool GetMouseButton(int button);

	    /// <summary>
	    ///   <para>Interface to Input.mousePosition. Can be overridden to provide custom input instead of using the Input class.</para>
	    /// </summary>
	    Vector3 mousePosition { get; }

	    /// <summary>
	    ///   <para>Interface to Input.mouseScrollDelta. Can be overridden to provide custom input instead of using the Input class.</para>
	    /// </summary>
	    Vector2 mouseScrollDelta { get; }

	    /// <summary>
	    ///   <para>Interface to Input.touchSupported. Can be overridden to provide custom input instead of using the Input class.</para>
	    /// </summary>
	    bool touchSupported { get; }

	    /// <summary>
	    ///   <para>Interface to Input.touchCount. Can be overridden to provide custom input instead of using the Input class.</para>
	    /// </summary>
	    int touchCount { get; }

	    /// <summary>
	    ///   <para>Interface to Input.GetTouch. Can be overridden to provide custom input instead of using the Input class.</para>
	    /// </summary>
	    /// <param name="index"></param>
	    Touch GetTouch(int index);

	    /// <summary>
	    ///   <para>Interface to Input.GetAxisRaw. Can be overridden to provide custom input instead of using the Input class.</para>
	    /// </summary>
	    /// <param name="axisName"></param>
	    float GetAxisRaw(string axisName);

	    /// <summary>
	    ///   <para>Interface to Input.GetButtonDown. Can be overridden to provide custom input instead of using the Input class.</para>
	    /// </summary>
	    /// <param name="buttonName"></param>
	    bool GetButtonDown(string buttonName);
	}
}
