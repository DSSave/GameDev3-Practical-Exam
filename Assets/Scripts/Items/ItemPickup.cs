using UnityEngine;
using UnityEngine.SceneManagement;

namespace Chisanuphong
{
    [RequireComponent(typeof(Collider))]
    public class ItemPickup : MonoBehaviour, IInteractable
    {
        [Header("Item Configuration")]
        [SerializeField] private ItemData itemData;
        [SerializeField] private bool requireInteraction = false;

        [Header("Animation")]
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private float bobSpeed = 3f;
        [SerializeField] private float bobHeight = 0.2f;

        private Vector3 startPosition;
        private bool isCollected = false;

        public ItemData Data => itemData;
        public bool RequireInteraction { get => requireInteraction; set => requireInteraction = value; }
        public bool IsCollected => isCollected;

        public string InteractionPrompt
        {
            get
            {
                if (itemData == null) return "[E] Collect Item";
                if (itemData.scoreModifier > 0) return $"[E] Pick up (Score +{itemData.scoreModifier})";
                if (itemData.scoreModifier < 0) return $"[E] Hazard (Score {itemData.scoreModifier})";
                if (itemData.timeModifier > 0) return $"[E] Pick up (Time +{itemData.timeModifier:0}s)";
                if (itemData.timeModifier < 0) return $"[E] Hazard (Time {itemData.timeModifier:0}s)";
                return $"[E] {itemData.itemName}";
            }
        }

        public bool CanInteract(GameObject interactor) => !isCollected;

        private void Start()
        {
            startPosition = transform.position;

            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void Update()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            transform.position = startPosition + new Vector3(0f, Mathf.Sin(Time.time * bobSpeed) * bobHeight, 0f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isCollected || requireInteraction) return;
            if (SceneManager.GetActiveScene().name == GameManager.SceneStage2) return;

            if (other.CompareTag("Player") || other.GetComponent<ItemCollector>() != null || other.GetComponentInParent<ItemCollector>() != null)
            {
                ItemCollector collector = other.GetComponent<ItemCollector>() ?? other.GetComponentInParent<ItemCollector>();
                if (collector != null)
                {
                    collector.CollectItem(this);
                }
                else
                {
                    ApplyDirect();
                    Collect();
                }
            }
        }

        public void Interact(GameObject interactor)
        {
            if (isCollected) return;

            ItemCollector collector = interactor != null 
                ? (interactor.GetComponent<ItemCollector>() ?? interactor.GetComponentInParent<ItemCollector>())
                : null;

            if (collector != null)
            {
                collector.CollectItem(this);
            }
            else
            {
                ApplyDirect();
                Collect();
            }
        }

        public void ApplyDirect()
        {
            if (itemData == null) return;

            if (itemData.scoreModifier != 0)
            {
                GameManager.AddScore(itemData.scoreModifier);
            }

            if (itemData.timeModifier != 0f)
            {
                GameManager.AddTime(itemData.timeModifier);
            }
        }

        public void Collect()
        {
            if (isCollected) return;
            isCollected = true;
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
            Destroy(gameObject);
        }
    }
}
