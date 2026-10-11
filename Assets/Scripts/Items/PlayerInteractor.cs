using UnityEngine;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Chisanuphong
{
    public class PlayerInteractor : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private float interactionRadius = 2.5f;
        [SerializeField] private Vector3 sphereCenterOffset = new Vector3(0f, 1f, 0.5f);
        [SerializeField] private LayerMask interactableLayer = ~0;

        [Header("HUD Feedback")]
        [SerializeField] private GameObject interactionPromptRoot;
        [SerializeField] private TextMeshProUGUI interactionPromptText;

        private IInteractable currentInteractable;

        public float InteractionRadius => interactionRadius;
        public IInteractable CurrentInteractable => currentInteractable;

        private void Update()
        {
            DetectInteractables();
            HandleInput();
            UpdatePromptUI();
        }

        private void DetectInteractables()
        {
            Vector3 center = transform.position + transform.TransformDirection(sphereCenterOffset);
            Collider[] hits = Physics.OverlapSphere(center, interactionRadius, interactableLayer, QueryTriggerInteraction.Collide);

            IInteractable bestTarget = null;
            float closestDist = float.MaxValue;

            for (int i = 0; i < hits.Length; i++)
            {
                Collider col = hits[i];
                if (col.gameObject == gameObject || col.transform.IsChildOf(transform)) continue;

                IInteractable interactable = col.GetComponent<IInteractable>() 
                    ?? col.GetComponentInParent<IInteractable>();

                if (interactable != null && interactable.CanInteract(gameObject))
                {
                    float dist = Vector3.Distance(transform.position, col.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        bestTarget = interactable;
                    }
                }
            }

            currentInteractable = bestTarget;
        }

        private void HandleInput()
        {
            if (currentInteractable == null) return;

            bool pressed = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                pressed = true;
#endif
            try
            {
                if (Input.GetKeyDown(KeyCode.E))
                    pressed = true;
            }
            catch { }

            if (pressed)
            {
                currentInteractable.Interact(gameObject);
                DetectInteractables();
            }
        }

        private void UpdatePromptUI()
        {
            if (interactionPromptText == null && interactionPromptRoot == null) return;

            bool hasTarget = currentInteractable != null;

            if (interactionPromptRoot != null)
            {
                if (interactionPromptRoot.activeSelf != hasTarget)
                    interactionPromptRoot.SetActive(hasTarget);
            }
            else if (interactionPromptText != null)
            {
                if (interactionPromptText.gameObject.activeSelf != hasTarget)
                    interactionPromptText.gameObject.SetActive(hasTarget);
            }

            if (hasTarget && interactionPromptText != null)
            {
                interactionPromptText.text = currentInteractable.InteractionPrompt;
            }
        }

        public void SetPromptUI(GameObject promptRoot, TextMeshProUGUI promptText)
        {
            interactionPromptRoot = promptRoot;
            interactionPromptText = promptText;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = currentInteractable != null ? Color.green : Color.cyan;
            Vector3 center = transform.position + transform.TransformDirection(sphereCenterOffset);
            Gizmos.DrawWireSphere(center, interactionRadius);
        }
    }
}
