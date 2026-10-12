using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace Chisanuphong
{
    public class ItemCollector : MonoBehaviour
    {
        [Header("Audio Feedback")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip defaultBonusSfx;
        [SerializeField] private AudioClip defaultPenaltySfx;
        [SerializeField] private AudioMixerGroup sfxMixerGroup;

        [Header("Collection Mode")]
        [SerializeField] private bool allowTriggerPickup = false;

        public bool AllowTriggerPickup { get => allowTriggerPickup; set => allowTriggerPickup = value; }

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

            if (sfxMixerGroup != null && audioSource.outputAudioMixerGroup == null)
            {
                audioSource.outputAudioMixerGroup = sfxMixerGroup;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!allowTriggerPickup) return;
            if (SceneManager.GetActiveScene().name == GameManager.SceneStage2) return;

            ItemPickup item = other.GetComponent<ItemPickup>() ?? other.GetComponentInParent<ItemPickup>();
            if (item != null && !item.RequireInteraction && !item.IsCollected)
            {
                CollectItem(item);
            }
        }

        public void CollectItem(ItemPickup item)
        {
            if (item == null || item.Data == null || item.IsCollected) return;

            item.Collect();

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
        }
    }
}
