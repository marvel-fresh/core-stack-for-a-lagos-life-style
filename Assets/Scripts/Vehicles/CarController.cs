using System;
using UnityEngine;
using LagosLifestyle.Input;

namespace LagosLifestyle.Vehicles
{
    [Serializable]
    public struct WheelSlot
    {
        public WheelCollider collider;
        public Transform meshTransform;
        public bool isMotor;
        public bool isSteer;
    }

    /// <summary>
    /// Vehicle Physics Controller tailored for Lagos open-world traffic (Danfo minibuses, sedans, trucks).
    /// Uses Unity WheelColliders, anti-roll stabilization, motor torque distribution, and handbrake.
    /// Reads steering and acceleration from InputManager.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class CarController : MonoBehaviour
    {
        [Header("Wheels Configuration")]
        [SerializeField] private WheelSlot frontLeft;
        [SerializeField] private WheelSlot frontRight;
        [SerializeField] private WheelSlot rearLeft;
        [SerializeField] private WheelSlot rearRight;

        [Header("Engine & Handling")]
        [Tooltip("Max motor torque per drive wheel in N*m")]
        [SerializeField] private float motorTorque = 1600f;

        [Tooltip("Max braking torque")]
        [SerializeField] private float brakeTorque = 3000f;

        [Tooltip("Handbrake torque applied to rear wheels")]
        [SerializeField] private float handbrakeTorque = 6000f;

        [Tooltip("Maximum steering angle in degrees")]
        [SerializeField] private float maxSteerAngle = 35f;

        [Header("Stability Tuning (Danfo Center of Mass)")]
        [Tooltip("Lower center of mass prevents tall Danfo minibuses from tipping over during sharp Lagos turns")]
        [SerializeField] private Vector3 centerOfMassOffset = new Vector3(0f, -0.6f, 0f);

        [Header("Audio & Effects")]
        [SerializeField] private AudioSource engineAudio;
        [SerializeField] private AudioSource hornAudio;
        [SerializeField] private float minPitch = 0.8f;
        [SerializeField] private float maxPitch = 2.2f;

        private Rigidbody rb;
        private bool isPlayerControlled = false;
        private float currentSteerAngle;
        private float currentSpeedKmH;

        public float SpeedKmH => currentSpeedKmH;
        public bool IsPlayerControlled => isPlayerControlled;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.centerOfMass += centerOfMassOffset;
        }

        private void FixedUpdate()
        {
            currentSpeedKmH = rb.linearVelocity.magnitude * 3.6f;

            if (isPlayerControlled && InputManager.Instance != null)
            {
                HandleDriverInput();
            }
            else
            {
                ApplyDeceleration();
            }

            UpdateWheelVisuals();
            UpdateEngineAudio();
        }

        private void HandleDriverInput()
        {
            Vector2 move = InputManager.Instance.MoveInput;
            float throttle = move.y;
            float steer = move.x;
            bool handbrake = InputManager.Instance.HandbrakeTriggered;

            // Steering
            currentSteerAngle = steer * maxSteerAngle;
            ApplySteering(frontLeft, currentSteerAngle);
            ApplySteering(frontRight, currentSteerAngle);

            // Acceleration & Braking
            if (handbrake)
            {
                ApplyHandbrake();
            }
            else
            {
                // Normal driving
                ApplyTorque(frontLeft, throttle);
                ApplyTorque(frontRight, throttle);
                ApplyTorque(rearLeft, throttle);
                ApplyTorque(rearRight, throttle);
            }

            // Lagos Horn
            if (InputManager.Instance.HornTriggered && hornAudio != null && !hornAudio.isPlaying)
            {
                hornAudio.Play();
            }
        }

        private void ApplyTorque(WheelSlot slot, float throttle)
        {
            if (slot.collider == null) return;

            // Reset brake
            slot.collider.brakeTorque = 0f;

            if (slot.isMotor)
            {
                slot.collider.motorTorque = throttle * motorTorque;
            }
        }

        private void ApplySteering(WheelSlot slot, float steerAngle)
        {
            if (slot.collider != null && slot.isSteer)
            {
                slot.collider.steerAngle = steerAngle;
            }
        }

        private void ApplyHandbrake()
        {
            if (rearLeft.collider != null) rearLeft.collider.brakeTorque = handbrakeTorque;
            if (rearRight.collider != null) rearRight.collider.brakeTorque = handbrakeTorque;
            if (frontLeft.collider != null) frontLeft.collider.motorTorque = 0f;
            if (frontRight.collider != null) frontRight.collider.motorTorque = 0f;
        }

        private void ApplyDeceleration()
        {
            float idleBrake = 50f;
            if (frontLeft.collider != null) { frontLeft.collider.motorTorque = 0; frontLeft.collider.brakeTorque = idleBrake; }
            if (frontRight.collider != null) { frontRight.collider.motorTorque = 0; frontRight.collider.brakeTorque = idleBrake; }
            if (rearLeft.collider != null) { rearLeft.collider.motorTorque = 0; rearLeft.collider.brakeTorque = idleBrake; }
            if (rearRight.collider != null) { rearRight.collider.motorTorque = 0; rearRight.collider.brakeTorque = idleBrake; }
        }

        private void UpdateWheelVisuals()
        {
            SyncWheelMesh(frontLeft);
            SyncWheelMesh(frontRight);
            SyncWheelMesh(rearLeft);
            SyncWheelMesh(rearRight);
        }

        private void SyncWheelMesh(WheelSlot slot)
        {
            if (slot.collider == null || slot.meshTransform == null) return;

            slot.collider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            slot.meshTransform.position = pos;
            slot.meshTransform.rotation = rot;
        }

        private void UpdateEngineAudio()
        {
            if (engineAudio == null) return;

            float pitchRatio = Mathf.Clamp01(currentSpeedKmH / 120f);
            engineAudio.pitch = Mathf.Lerp(minPitch, maxPitch, pitchRatio);
        }

        public void SetDriverControl(bool active)
        {
            isPlayerControlled = active;
            if (!active)
            {
                ApplyDeceleration();
            }
        }
    }
}
