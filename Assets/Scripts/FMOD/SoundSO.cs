using System;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using DG.Tweening.Core;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "SoundSO", menuName = Constant.GAME_NAME + "/FMOD/Sound")]
    public class SoundSO : ScriptableObject
    {
        #region PUBLIC_VARS

        public SoundType soundType;
        public SoundInstance SoundInstance => _soundInstance;

        #endregion

        #region PRIVATE_VARS

        [SerializeField] private EventReference _eventRefrence;
        private SoundInstance _soundInstance;

    	#endregion

    	#region UNITY_CALLBACKS

    	#endregion

    	#region PUBLIC_METHODS

        public void Init()
        {
            _soundInstance = new SoundInstance(_eventRefrence);
        }

        public void Play()
        {
            _soundInstance.Play();
        }

        public void Stop(bool isAllowFadeOut = false)
        {
            _soundInstance.Stop(isAllowFadeOut);
        }

        public SoundInstance PlayWithNewInstance()
        {
            return new SoundInstance(_eventRefrence, true);
        }
    	#endregion

    	#region PRIVATE_METHODS

    	#endregion

    	#region CO-ROUTINES

    	#endregion

    	#region EVENT_HANDLERS

    	#endregion

    	#region UI_CALLBACKS       

    	#endregion
    }

    public class SoundInstance
    {
        public EventInstance _eventInstance;
        private float defaultVolume;
        private EventReference eventReference;

        public SoundInstance()
        { 
        }

        public SoundInstance(EventReference eventReference, bool isPlay = false)
        {
            this.eventReference = eventReference;
            _eventInstance = RuntimeManager.CreateInstance(eventReference);
            _eventInstance.getVolume(out defaultVolume);
            if (isPlay)
                Play();
        }

        public void Play()
        {
            if (!_eventInstance.isValid())
                _eventInstance = RuntimeManager.CreateInstance(eventReference);

            _eventInstance.start();
        }

        public void Stop(bool isAllowFadeOut = false)
        {
            _eventInstance.stop(isAllowFadeOut ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
        }

        public void SetVolume(float targetVolume, bool isFade = false, float fadeTime = 0.5f)
        {
            if (targetVolume < 0)
                targetVolume = defaultVolume;

            if (isFade)
            {
                DOGetter<float> volumeGetter = delegate {
                    _eventInstance.getVolume(out float vol);
                    return vol;
                };

                DOSetter<float> volumeSetter = delegate (float vol) {
                    _eventInstance.setVolume(vol);
                };

                DOTween.To(volumeGetter, volumeSetter, targetVolume, fadeTime);
            }
            else
                _eventInstance.setVolume(targetVolume);
        }
    }
}
