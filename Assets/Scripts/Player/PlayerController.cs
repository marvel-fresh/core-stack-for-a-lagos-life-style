using UnityEngine;
using LagosLifestyle.Input;

namespace LagosLifestyle.Player
{
    /// <summary>
    /// Third-Person Lagos Street Character Controller.
    /// Reads unified inputs from InputManager (works seamlessly with Keyboard, Gamepad, and Mobile Touch).
    /// Features camera-relative locomotion, smooth slerp rotation, gravity, and sprint mechanics.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Speeds (m/s)")]
        [Tooltip("Standard walking speed through Lagos streets and markets")]
        [SerializeField] private float walkSpeed = 3.5f;

        [Tooltip("Sprinting speed")]
        [SerializeField] private float sprintSpeed = 7.0f;

        [Tooltip("Rotation smoothing speed")]
        [SerializeField] private float rotationSpeed = 12f;

        [Header("Jump & Physics")]
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float gravity = -18f;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundDistance = 0.25f;
        [SerializeField] private LayerMask groundMask;

        [Header("Camera Reference")]
        [Tooltip("Reference to the main camera transform to ensure movement is relative to camera view")]
        [SerializeField] private Transform cameraTransform;

        [Header("Optional Animator")]
        [SerializeField] private Animator animator;

        // Internal State
        private CharacterController controller;
        private Vector3 verticalVelocity;
        private bool isGrounded;

        // Animator Parameter Hashes
        private readonly int hashSpeed = Animator.StringToHash("Speed");
        private readonly int hashIsGrounded = Animator.StringToHash("IsGrounded");
        private readonly int hashJump = Animator.StringToHash("Jump");

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        private void Update()
        {
            CheckGround();
            HandleMovement();
            HandleJumpAndGravity();
            UpdateAnimation();
        }

        private void CheckGround()
        {
            if (groundCheck != null)
            {
                isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            }
            else
            {
                isGrounded = controller.isGrounded;
            }

            if (isGrounded && verticalVelocity.y < 0)
            {
                verticalVelocity.y = -2f; // Slight downward snap to keep character grounded on slopes
            }
        }

        private void HandleMovement()
        {
            if (InputManager.Instance == null) return;

            Vector2 input = InputManager.Instance.MoveInput;
            bool isSprinting = InputManager.Instance.IsSprinting;

            // Camera-relative direction vector
            Vector3 camForward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
            Vector3 camRight = cameraTransform != null ? cameraTransform.right : Vector3.right;

            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDirection = (camForward * input.y + camRight * input.x).normalized;

            if (moveDirection.sqrMagnitude > 0.001f)
            {
                // Smoothly rotate character towards movement direction
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
                controller.Move(moveDirection * (currentSpeed * Time.deltaTime));
            }
        }

        private void HandleJumpAndGravity()
        {
            if (InputManager.Instance == null) return;

            if (isGrounded && InputManager.Instance.JumpTriggered)
            {
                verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                if (animator != null)
                {
                    animator.SetTrigger(hashJump);
                }
            }

            // Apply gravity
            verticalVelocity.y += gravity * Time.deltaTime;
            controller.Move(verticalVelocity * Time.deltaTime);
        }

        private void UpdateAnimation()
        {
            if (animator == null || InputManager.Instance == null) return;

            Vector2 input = InputManager.Instance.MoveInput;
            float targetSpeedNorm = input.magnitude;

            if (InputManager.Instance.IsSprinting && targetSpeedNorm > 0.1f)
            {
                targetSpeedNorm = 2.0f; // Sprint tier
            }

            animator.SetFloat(hashSpeed, targetSpeedNorm, 0.1f, Time.deltaTime);
            animator.SetBool(hashIsGrounded, isGrounded);
        }

        /// <summary>
        /// Enable or disable character movement (e.g., when entering a Danfo bus or during cutscenes).
        /// </summary>
        public void SetControlActive(bool active)
        {
            enabled = active;
            controller.enabled = active;
            if (!active)
            {
                verticalVelocity = Vector3.zero;
            }
        }
    }
}
