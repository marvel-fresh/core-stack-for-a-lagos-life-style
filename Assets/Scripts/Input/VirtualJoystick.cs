using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LagosLifestyle.Input
{
    /// <summary>
    /// Smooth on-screen touch joystick for mobile gameplay in Lagos Life-style.
    /// Handles drag, deadzone, visual knob offset, and feeds direction directly into InputManager.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("UI References")]
        [Tooltip("Background container of the joystick")]
        [SerializeField] private RectTransform background;

        [Tooltip("Movable thumb/handle stick")]
        [SerializeField] private RectTransform handle;

        [Header("Joystick Settings")]
        [Tooltip("Max pixel radius the handle can move from background center")]
        [SerializeField] private float handleRange = 64f;

        [Tooltip("Ignore tiny accidental finger twitches below this radius percentage")]
        [Range(0f, 0.5f)]
        [SerializeField] private float deadZone = 0.1f;

        [Tooltip("If true, snaps joystick center to first finger touch point in touch zone")]
        [SerializeField] private bool dynamicPosition = false;

        private Vector2 inputVector = Vector2.zero;
        private Canvas canvas;
        private Camera targetCamera;
        private Vector2 defaultAnchoredPosition;

        public Vector2 Direction => inputVector;

        private void Start()
        {
            canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("[VirtualJoystick] Missing parent Canvas!");
            }

            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                targetCamera = canvas.worldCamera;
            }

            if (background == null)
            {
                background = GetComponent<RectTransform>();
            }

            defaultAnchoredPosition = background.anchoredPosition;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (dynamicPosition)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    (RectTransform)transform.parent,
                    eventData.position,
                    targetCamera,
                    out Vector2 localPoint
                );
                background.anchoredPosition = localPoint;
            }

            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background,
                eventData.position,
                targetCamera,
                out Vector2 position
            ))
            {
                // Normalize position relative to handleRange
                position.x = (position.x / background.sizeDelta.x);
                position.y = (position.y / background.sizeDelta.y);

                inputVector = new Vector2(position.x * 2f, position.y * 2f);

                // Check deadzone
                if (inputVector.magnitude > 1f)
                {
                    inputVector.Normalize();
                }
                else if (inputVector.magnitude < deadZone)
                {
                    inputVector = Vector2.zero;
                }

                // Move visual knob
                if (handle != null)
                {
                    handle.anchoredPosition = inputVector * handleRange;
                }

                // Send to InputManager
                if (InputManager.Instance != null)
                {
                    InputManager.Instance.SetVirtualMove(inputVector);
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            inputVector = Vector2.zero;

            if (handle != null)
            {
                handle.anchoredPosition = Vector2.zero;
            }

            if (dynamicPosition)
            {
                background.anchoredPosition = defaultAnchoredPosition;
            }

            if (InputManager.Instance != null)
            {
                InputManager.Instance.SetVirtualMove(Vector2.zero);
            }
        }
    }
}
