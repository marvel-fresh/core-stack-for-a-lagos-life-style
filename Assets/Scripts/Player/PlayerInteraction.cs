using UnityEngine;
using LagosLifestyle.Input;

namespace LagosLifestyle.Player
{
    /// <summary>
    /// Detects nearby Lagos interactive objects (Danfo buses, market vendors, kiosks, Okada bikes).
    /// Listens for Interact input (E/F key or mobile Interact button) to trigger action.
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private float interactionRadius = 2.5f;
        [SerializeField] private LayerMask interactableLayer;

        [Header("UI Prompt")]
        [Tooltip("Optional reference to floating world UI or canvas prompt (e.g. 'Press E or Tap to Enter Danfo')")]
        [SerializeField] private GameObject interactionPromptUI;

        private Collider[] hitColliders = new Collider[5];
        private IInteractable currentInteractable;

        private void Update()
        {
            DetectInteractables();

            if (currentInteractable != null && InputManager.Instance != null && InputManager.Instance.InteractTriggered)
            {
                currentInteractable.OnInteract(gameObject);
            }
        }

        private void DetectInteractables()
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, interactionRadius, hitColliders, interactableLayer);
            IInteractable closestInteractable = null;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                if (hitColliders[i].TryGetComponent<IInteractable>(out var interactable))
                {
                    float dist = Vector3.Distance(transform.position, hitColliders[i].transform.position);
                    if (dist < closestDistance)
                    {
                        closestDistance = dist;
                        closestInteractable = interactable;
                    }
                }
            }

            currentInteractable = closestInteractable;

            if (interactionPromptUI != null)
            {
                interactionPromptUI.SetActive(currentInteractable != null);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }

    /// <summary>
    /// Common interface for any interactable entity in Lagos (vehicles, vendors, street doors).
    /// </summary>
    public interface IInteractable
    {
        string GetPromptMessage();
        void OnInteract(GameObject player);
    }
}
