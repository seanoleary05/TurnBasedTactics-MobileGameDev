using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class TapSwipeInput : MonoBehaviour
{
    public float swipeDp = 50f;     // distance in dp, not pixels
    public float tapMax = 0.3f;     // seconds
    public GameObject sampleShape;

    void OnEnable() => EnhancedTouchSupport.Enable();
    void OnDisable() => EnhancedTouchSupport.Disable();

    void Update()
    {
        foreach (var t in Touch.activeTouches)
        {
            if (t.phase != TouchPhase.Ended) continue;
            float px = swipeDp * Mathf.Max(Screen.dpi, 160f) / 160f;
            Vector2 d = t.screenPosition - t.startScreenPosition;
            if (d.magnitude >= px) Debug.Log("Swipe " + d.normalized);
            else if (t.time - t.startTime < tapMax) {
                Debug.Log("Tap: Selected @ " + t.screenPosition);
                Instantiate(sampleShape).transform.position = Camera.main.ScreenToWorldPoint(new Vector3(t.screenPosition.x, t.screenPosition.y, 10f));
            }
        }
    }
}