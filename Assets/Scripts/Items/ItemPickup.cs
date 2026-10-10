using UnityEngine;

namespace Chisanuphong
{
    [RequireComponent(typeof(Collider))]
    public class ItemPickup : MonoBehaviour
    {
        [Header("Item Configuration")]
        [SerializeField] private ItemData itemData;

        [Header("Animation")]
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private float bobSpeed = 3f;
        [SerializeField] private float bobHeight = 0.2f;

        private Vector3 startPosition;
        private bool isCollected = false;

        public ItemData Data => itemData;

        private void Start()
        {
            startPosition = transform.position;

            // Ensure collider is a trigger
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void Update()
        {
            // Visual floating & spinning effect for clean UX
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            transform.position = startPosition + new Vector3(0f, Mathf.Sin(Time.time * bobSpeed) * bobHeight, 0f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isCollected) return;

            if (other.CompareTag("Player") || other.GetComponent<ItemCollector>() != null || other.GetComponentInParent<ItemCollector>() != null)
            {
                ItemCollector collector = other.GetComponent<ItemCollector>() ?? other.GetComponentInParent<ItemCollector>();
                if (collector != null)
                {
                    collector.CollectItem(this);
                }
                else
                {
                    // Fallback direct application
                    ApplyDirect();
                    Collect();
                }
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
            Destroy(gameObject);
        }
    }
}
