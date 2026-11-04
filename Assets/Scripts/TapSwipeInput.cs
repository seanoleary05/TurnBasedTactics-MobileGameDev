using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class TapSwipeInput : MonoBehaviour
{
    public static event System.Action<Vector2> Tapped;          // screen position
    public static event System.Action<Vector2> Swiped;          // normalized direction

    public float swipeDp = 50f;     // distance in dp, not pixels
    public float tapMax = 0.3f;     // seconds
    public GameObject sampleShape;

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
#if UNITY_EDITOR
        TouchSimulation.Enable();   // mouse clicks act as touches in the editor
#endif
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        TouchSimulation.Disable();
#endif
        EnhancedTouchSupport.Disable();
    }

    void Update()
    {
        foreach (var t in Touch.activeTouches)
        {
            if (t.phase != TouchPhase.Ended) continue;
            float px = swipeDp * Mathf.Max(Screen.dpi, 160f) / 160f;
            Vector2 d = t.screenPosition - t.startScreenPosition;
            if (d.magnitude >= px)
            {
                Swiped?.Invoke(d.normalized);
            }
            else if (t.time - t.startTime < tapMax)
            {
                Tapped?.Invoke(t.screenPosition);
                Haptics.Pulse();
            }
        }
    }
}
