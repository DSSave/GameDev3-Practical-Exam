using UnityEngine;

namespace Chisanuphong
{
    public class ItemCollector : MonoBehaviour
    {
        [Header("Audio Feedback")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip defaultBonusSfx;
        [SerializeField] private AudioClip defaultPenaltySfx;

        private void Awake()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                    audioSource.spatialBlend = 0f; // 2D UI style
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            ItemPickup item = other.GetComponent<ItemPickup>();
            if (item == null)
            {
                item = other.GetComponentInParent<ItemPickup>();
            }

            if (item != null)
            {
                CollectItem(item);
            }
        }

        public void CollectItem(ItemPickup item)
        {
            if (item == null || item.Data == null) return;

            ItemData data = item.Data;

            // Apply score
            if (data.scoreModifier != 0)
            {
                GameManager.AddScore(data.scoreModifier);
            }

            // Apply time
            if (data.timeModifier != 0f)
            {
                GameManager.AddTime(data.timeModifier);
            }

            // Audio feedback
            AudioClip clipToPlay = data.sfxClip;
            if (clipToPlay == null)
            {
                clipToPlay = (data.scoreModifier > 0 || data.timeModifier > 0) ? defaultBonusSfx : defaultPenaltySfx;
            }

            if (audioSource != null && clipToPlay != null)
            {
                audioSource.PlayOneShot(clipToPlay);
            }

            // Destroy item
            item.Collect();
        }
    }
}
