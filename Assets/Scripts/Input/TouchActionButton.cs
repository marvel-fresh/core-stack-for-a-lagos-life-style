using UnityEngine;
using UnityEngine.EventSystems;

namespace LagosLifestyle.Input
{
    public enum TouchButtonAction
    {
        Jump,
        Sprint,
        Interact,
        Handbrake,
        Horn
    }

    /// <summary>
    /// Touch button component for UI action buttons (Jump, Sprint, Enter/Exit Danfo, Handbrake).
    /// Supports both single-tap triggers and hold states.
    /// </summary>
    public class TouchActionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Button Configuration")]
        [SerializeField] private TouchButtonAction actionType;

        [Tooltip("True for hold-to-activate (like Sprint or Handbrake). False for tap triggers (like Jump or Interact).")]
        [SerializeField] private bool isHoldAction = false;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (InputManager.Instance == null) return;

            switch (actionType)
            {
                case TouchButtonAction.Jump:
                    InputManager.Instance.TriggerVirtualJump();
                    break;
                case TouchButtonAction.Sprint:
                    InputManager.Instance.SetVirtualSprint(true);
                    break;
                case TouchButtonAction.Interact:
                    InputManager.Instance.TriggerVirtualInteract();
                    break;
                case TouchButtonAction.Handbrake:
                    InputManager.Instance.SetVirtualHandbrake(true);
                    break;
                case TouchButtonAction.Horn:
                    InputManager.Instance.TriggerVirtualHorn();
                    break;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (InputManager.Instance == null) return;

            if (isHoldAction)
            {
                switch (actionType)
                {
                    case TouchButtonAction.Sprint:
                        InputManager.Instance.SetVirtualSprint(false);
                        break;
                    case TouchButtonAction.Handbrake:
                        InputManager.Instance.SetVirtualHandbrake(false);
                        break;
                }
            }
        }
    }
}
