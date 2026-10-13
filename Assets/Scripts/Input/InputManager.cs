using UnityEngine;

namespace LagosLifestyle.Input
{
    /// <summary>
    /// Centralized Cross-Platform Input Manager for Lagos Life-style.
    /// Unifies Keyboard/Mouse, Gamepad, and Mobile On-Screen Touch Controls.
    /// Both Character Controller and Vehicle Physics read from this single source.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        [Header("Debug / Fallback Mode")]
        [Tooltip("If true, simulates mobile touch controls even while testing inside Unity Editor")]
        [SerializeField] private bool forceMobileUI = false;

        [Header("Current Input States (Read-Only)")]
        [SerializeField] private Vector2 moveInput;
        [SerializeField] private Vector2 lookInput;
        [SerializeField] private bool isSprinting;
        [SerializeField] private bool jumpTriggered;
        [SerializeField] private bool interactTriggered;
        [SerializeField] private bool handbrakeTriggered;
        [SerializeField] private bool hornTriggered;

        // Virtual Touch Overrides
        private Vector2 virtualMoveInput;
        private Vector2 virtualLookInput;
        private bool virtualSprint;
        private bool virtualJump;
        private bool virtualInteract;
        private bool virtualHandbrake;
        private bool virtualHorn;

        // Public Accessors
        public Vector2 MoveInput => moveInput;
        public Vector2 LookInput => lookInput;
        public bool IsSprinting => isSprinting;
        public bool JumpTriggered => jumpTriggered;
        public bool InteractTriggered => interactTriggered;
        public bool HandbrakeTriggered => handbrakeTriggered;
        public bool HornTriggered => hornTriggered;
        public bool IsMobileUIActive => forceMobileUI || Application.isMobilePlatform;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            PollHardwareInputs();
            CombineInputs();
        }

        private void LateUpdate()
        {
            // Reset frame triggers after all controllers have had a chance to read them
            if (jumpTriggered) virtualJump = false;
            if (interactTriggered) virtualInteract = false;
            if (hornTriggered) virtualHorn = false;
        }

        /// <summary>
        /// Reads keyboard, mouse, and standard gamepads.
        /// Works out of the box with Unity's standard inputs.
        /// </summary>
        private void PollHardwareInputs()
        {
            float horizontal = UnityEngine.Input.GetAxisRaw("Horizontal");
            float vertical = UnityEngine.Input.GetAxisRaw("Vertical");
            Vector2 rawKeyboardMove = new Vector2(horizontal, vertical).normalized;

            float mouseX = UnityEngine.Input.GetAxis("Mouse X");
            float mouseY = UnityEngine.Input.GetAxis("Mouse Y");
            Vector2 rawMouseLook = new Vector2(mouseX, mouseY);

            bool rawSprint = UnityEngine.Input.GetKey(KeyCode.LeftShift) || UnityEngine.Input.GetKey(KeyCode.RightShift);
            bool rawJump = UnityEngine.Input.GetKeyDown(KeyCode.Space);
            bool rawInteract = UnityEngine.Input.GetKeyDown(KeyCode.E) || UnityEngine.Input.GetKeyDown(KeyCode.F);
            bool rawHandbrake = UnityEngine.Input.GetKey(KeyCode.Space);
            bool rawHorn = UnityEngine.Input.GetKeyDown(KeyCode.H);

            // Combine Hardware + Virtual Inputs
            moveInput = (virtualMoveInput.sqrMagnitude > 0.01f) ? virtualMoveInput : rawKeyboardMove;
            lookInput = (virtualLookInput.sqrMagnitude > 0.01f) ? virtualLookInput : rawMouseLook;

            isSprinting = rawSprint || virtualSprint;
            jumpTriggered = rawJump || virtualJump;
            interactTriggered = rawInteract || virtualInteract;
            handbrakeTriggered = rawHandbrake || virtualHandbrake;
            hornTriggered = rawHorn || virtualHorn;
        }

        private void CombineInputs()
        {
            // Clamp Move Input magnitude to 1.0f
            if (moveInput.magnitude > 1f)
            {
                moveInput.Normalize();
            }
        }

        #region Virtual UI Setters (Called by Touch Joystick & Buttons)

        public void SetVirtualMove(Vector2 vector)
        {
            virtualMoveInput = vector;
        }

        public void SetVirtualLook(Vector2 vector)
        {
            virtualLookInput = vector;
        }

        public void SetVirtualSprint(bool active)
        {
            virtualSprint = active;
        }

        public void TriggerVirtualJump()
        {
            virtualJump = true;
        }

        public void TriggerVirtualInteract()
        {
            virtualInteract = true;
        }

        public void SetVirtualHandbrake(bool active)
        {
            virtualHandbrake = active;
        }

        public void TriggerVirtualHorn()
        {
            virtualHorn = true;
        }

        #endregion
    }
}
