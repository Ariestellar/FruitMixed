using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.Infrastructure.Services
{
    public class SoundService : IService
    {
        private AudioSource _audioSourceBackground;
        private Toggle _muteSoundToggle;
        private SessionData _sessionData;
        private AudioSource _audioSourceMerge;
        
        public void SetSoundSceneComponents(SoundSceneComponents soundSceneComponents, SessionData sessionData) {
            _sessionData = sessionData;
            _audioSourceMerge = soundSceneComponents.AudioSourceMerge;
            _audioSourceBackground = soundSceneComponents.AudioSourceBackground;
            _muteSoundToggle = soundSceneComponents.MuteSoundToggle;
            _muteSoundToggle.onValueChanged.AddListener(SwitchMute);
        }
        
        public void SwitchMute(bool muteValue) {
            _sessionData.isOffSound = muteValue;
            AudioListener.pause = muteValue;
        }

        public void PlaySoundBackground() => PlayBackgroundSound(_audioSourceBackground);
        public void PlaySoundMerge() => PlayBackgroundSound(_audioSourceMerge);

        private void PlayBackgroundSound(AudioSource audioSource) {
            if(audioSource == null) Debug.LogError("Компоненты для звуков на сцене не были переданы");
            audioSource.Play();
        } 
    } 
}