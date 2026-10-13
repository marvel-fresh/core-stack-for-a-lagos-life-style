using UnityEngine;
using LagosLifestyle.Player;
using LagosLifestyle.Input;

namespace LagosLifestyle.Vehicles
{
    /// <summary>
    /// Handles entering and exiting Lagos vehicles (Danfo bus, Okada, car).
    /// Manages player disabling/enabling, camera switching, and UI control toggle.
    /// Implements IInteractable so player interaction system can trigger it automatically.
    /// </summary>
    [RequireComponent(typeof(CarController))]
    public class VehicleEntryExit : MonoBehaviour, IInteractable
    {
        [Header("Entry / Exit References")]
        [Tooltip("Transform position where player spawns when exiting the vehicle")]
        [SerializeField] private Transform exitPoint;

        [Tooltip("Optional seat transform inside the vehicle where player can be seated")]
        [SerializeField] private Transform driverSeat;

        [Tooltip("Vehicle Follow Camera GameObject to enable while driving")]
        [SerializeField] private GameObject vehicleCamera;

        [Tooltip("Player Follow Camera GameObject to disable while driving")]
        [SerializeField] private GameObject playerCamera;

        [Header("Mobile UI Handler")]
        [SerializeField] private MobileControlsToggle mobileToggle;

        private CarController carController;
        private GameObject activePlayer;
        private bool isOccupied = false;

        private void Awake()
        {
            carController = GetComponent<CarController>();

            if (exitPoint == null)
            {
                // Fallback to left side of car
                GameObject fallbackExit = new GameObject("DefaultExitPoint");
                fallbackExit.transform.SetParent(transform);
                fallbackExit.transform.localPosition = new Vector3(-2.0f, 0f, 0f);
                exitPoint = fallbackExit.transform;
            }

            if (mobileToggle == null)
            {
                mobileToggle = FindFirstObjectByType<MobileControlsToggle>();
            }
        }

        private void Update()
        {
            // If player is currently driving, allow them to exit using Interact button
            if (isOccupied && InputManager.Instance != null && InputManager.Instance.InteractTriggered)
            {
                ExitVehicle();
            }
        }

        #region IInteractable Implementation

        public string GetPromptMessage()
        {
            return isOccupied ? "Exit Vehicle" : "Enter Danfo / Vehicle";
        }

        public void OnInteract(GameObject player)
        {
            if (!isOccupied)
            {
                EnterVehicle(player);
            }
        }

        #endregion

        public void EnterVehicle(GameObject player)
        {
            activePlayer = player;
            isOccupied = true;

            // 1. Disable player control and visuals
            if (player.TryGetComponent<PlayerController>(out var playerCtrl))
            {
                playerCtrl.SetControlActive(false);
            }

            // Hide or parent player
            player.SetActive(false);

            // 2. Enable car physics & control
            carController.SetDriverControl(true);

            // 3. Switch camera
            if (playerCamera != null) playerCamera.SetActive(false);
            if (vehicleCamera != null) vehicleCamera.SetActive(true);

            // 4. Switch Mobile UI to Driving controls
            if (mobileToggle != null)
            {
                mobileToggle.SetMode(false);
            }

            Debug.Log($"[Vehicle] Player entered {gameObject.name}");
        }

        public void ExitVehicle()
        {
            if (activePlayer == null) return;

            // 1. Disable car driver control
            carController.SetDriverControl(false);

            // 2. Position player at driver door exit point
            activePlayer.transform.position = exitPoint.position;
            activePlayer.transform.rotation = Quaternion.LookRotation(transform.forward);
            activePlayer.SetActive(true);

            if (activePlayer.TryGetComponent<PlayerController>(out var playerCtrl))
            {
                playerCtrl.SetControlActive(true);
            }

            // 3. Switch cameras back
            if (vehicleCamera != null) vehicleCamera.SetActive(false);
            if (playerCamera != null) playerCamera.SetActive(true);

            // 4. Switch Mobile UI back to On-Foot
            if (mobileToggle != null)
            {
                mobileToggle.SetMode(true);
            }

            isOccupied = false;
            activePlayer = null;

            Debug.Log($"[Vehicle] Player exited {gameObject.name}");
        }
    }
}
