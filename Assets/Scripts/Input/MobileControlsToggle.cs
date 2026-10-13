using UnityEngine;

namespace LagosLifestyle.Input
{
    /// <summary>
    /// Automatically manages mobile UI visibility.
    /// Also switches control layouts between On-Foot Mode and Vehicle Mode.
    /// </summary>
    public class MobileControlsToggle : MonoBehaviour
    {
        [Header("Canvas Groups / Panels")]
        [Tooltip("Root canvas holding all mobile on-screen controls")]
        [SerializeField] private GameObject mobileUIRoot;

        [Tooltip("Buttons visible when walking on foot (Joystick, Jump, Sprint, Interact)")]
        [SerializeField] private GameObject onFootControls;

        [Tooltip("Buttons visible when driving Danfo/Car (Steer, Gas, Brake, Handbrake, Horn, Exit)")]
        [SerializeField] private GameObject vehicleControls;

        [Header("Overrides")]
        [SerializeField] private bool forceShowInEditor = true;

        private void Start()
        {
            ApplyPlatformVisibility();
            SetMode(true); // Default to on-foot mode
        }

        private void ApplyPlatformVisibility()
        {
            bool shouldBeVisible = Application.isMobilePlatform || (Application.isEditor && forceShowInEditor);

            if (mobileUIRoot != null)
            {
                mobileUIRoot.SetActive(shouldBeVisible);
            }
        }

        /// <summary>
        /// Switch between On-Foot controls and Driving controls on mobile.
        /// </summary>
        /// <param name="isOnFoot">True if character is walking, False if in vehicle</param>
        public void SetMode(bool isOnFoot)
        {
            if (onFootControls != null)
            {
                onFootControls.SetActive(isOnFoot);
            }

            if (vehicleControls != null)
            {
                vehicleControls.SetActive(!isOnFoot);
            }
        }
    }
}
