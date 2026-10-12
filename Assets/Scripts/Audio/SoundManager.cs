using UnityEngine;
using UnityEngine.UI;

namespace Chisanuphong
{
    public class SoundManager : MonoBehaviour
    {
        [SerializeField] protected SoundSettings m_SoundSettings;

        public Slider m_SliderMasterVolume;
        public Slider m_SliderMusicVolume;
        public Slider m_SliderMasterSFXVolume;
        public Slider m_SliderSFXVolume;
        public Slider m_SliderUIVolume;

        public SoundSettings SoundSettings => m_SoundSettings;

        private void Start()
        {
            InitialiseVolumes();
        }

        public void InitialiseVolumes()
        {
            if (m_SoundSettings == null) return;

            SetMasterVolume(m_SoundSettings.MasterVolume);
            SetMusicVolume(m_SoundSettings.MusicVolume);
            SetMasterSFXVolume(m_SoundSettings.MasterSFXVolume);
            SetSFXVolume(m_SoundSettings.SFXVolume);
            SetUIVolume(m_SoundSettings.UIVolume);
        }

        public void SetMasterVolume(float vol)
        {
            if (m_SoundSettings == null || m_SoundSettings.AudioMixer == null) return;
            m_SoundSettings.AudioMixer.SetFloat(m_SoundSettings.MasterVolumeName, vol);
            m_SoundSettings.MasterVolume = vol;
            if (m_SliderMasterVolume != null && !Mathf.Approximately(m_SliderMasterVolume.value, vol))
            {
                m_SliderMasterVolume.value = vol;
            }
        }

        public void SetMusicVolume(float vol)
        {
            if (m_SoundSettings == null || m_SoundSettings.AudioMixer == null) return;
            m_SoundSettings.AudioMixer.SetFloat(m_SoundSettings.MusicVolumeName, vol);
            m_SoundSettings.MusicVolume = vol;
            if (m_SliderMusicVolume != null && !Mathf.Approximately(m_SliderMusicVolume.value, vol))
            {
                m_SliderMusicVolume.value = vol;
            }
        }

        public void SetMasterSFXVolume(float vol)
        {
            if (m_SoundSettings == null || m_SoundSettings.AudioMixer == null) return;
            m_SoundSettings.AudioMixer.SetFloat(m_SoundSettings.MasterSFXVolumeName, vol);
            m_SoundSettings.MasterSFXVolume = vol;
            if (m_SliderMasterSFXVolume != null && !Mathf.Approximately(m_SliderMasterSFXVolume.value, vol))
            {
                m_SliderMasterSFXVolume.value = vol;
            }
        }

        public void SetSFXVolume(float vol)
        {
            if (m_SoundSettings == null || m_SoundSettings.AudioMixer == null) return;
            m_SoundSettings.AudioMixer.SetFloat(m_SoundSettings.SFXVolumeName, vol);
            m_SoundSettings.SFXVolume = vol;
            if (m_SliderSFXVolume != null && !Mathf.Approximately(m_SliderSFXVolume.value, vol))
            {
                m_SliderSFXVolume.value = vol;
            }
        }

        public void SetUIVolume(float vol)
        {
            if (m_SoundSettings == null || m_SoundSettings.AudioMixer == null) return;
            m_SoundSettings.AudioMixer.SetFloat(m_SoundSettings.UIVolumeName, vol);
            m_SoundSettings.UIVolume = vol;
            if (m_SliderUIVolume != null && !Mathf.Approximately(m_SliderUIVolume.value, vol))
            {
                m_SliderUIVolume.value = vol;
            }
        }
    }
}
