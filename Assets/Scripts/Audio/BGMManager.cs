using UnityEngine;
using UnityEngine.Audio;

namespace Chisanuphong
{
    public class BGMManager : MonoBehaviour
    {
        public static BGMManager Instance { get; private set; }

        [Header("Audio Configuration")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip bgmClip;
        [SerializeField] private AudioMixerGroup musicMixerGroup;
        [SerializeField] private SoundSettings soundSettings;

        public AudioSource AudioSource => audioSource;
        public AudioClip BGMClip { get => bgmClip; set => bgmClip = value; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitAudioSource();
        }

        private void Start()
        {
            PlayBGM();
        }

        private void InitAudioSource()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D Stereo sound

            if (musicMixerGroup == null && soundSettings != null && soundSettings.AudioMixer != null)
            {
                var groups = soundSettings.AudioMixer.FindMatchingGroups("Music");
                if (groups != null && groups.Length > 0)
                {
                    musicMixerGroup = groups[0];
                }
            }

            if (musicMixerGroup != null)
            {
                audioSource.outputAudioMixerGroup = musicMixerGroup;
            }

            if (soundSettings != null && soundSettings.AudioMixer != null)
            {
                soundSettings.AudioMixer.SetFloat(soundSettings.MusicVolumeName, soundSettings.MusicVolume);
                soundSettings.AudioMixer.SetFloat(soundSettings.MasterVolumeName, soundSettings.MasterVolume);
            }
        }

        public void PlayBGM(AudioClip clip = null)
        {
            if (clip != null)
            {
                bgmClip = clip;
            }

            if (audioSource == null) InitAudioSource();

            if (bgmClip != null)
            {
                if (audioSource.clip != bgmClip)
                {
                    audioSource.clip = bgmClip;
                    audioSource.Play();
                }
                else if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                }
            }
        }

        public void StopBGM()
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }

        public void PauseBGM()
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Pause();
            }
        }

        public void ResumeBGM()
        {
            if (audioSource != null && !audioSource.isPlaying)
            {
                audioSource.UnPause();
            }
        }
    }
}
