using FMOD.Studio;
using FMODUnity;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class SoundHandler : Manager<SoundHandler>
    {
        #region PRIVATE_VARS
        [SerializeField] private SoundSO[] _soundSOs;
        [SerializeField] private SoundType _coreBackgroundSoundType;
        private Dictionary<SoundType, SoundSO> _soundDict = new Dictionary<SoundType, SoundSO>();
        private Coroutine busFadeCoroutine;
        private Bus _musicBus;
        private Bus _sFXBus;

        private const string MUSIC_BUS_PATH = "bus:/Music";
        private const string SFX_BUS_PATH = "bus:/SFX";
        private const string SFX_Prefs_key = "SFXPlayerPref";
        private const string Music_Prefs_key = "MusicPlayerPref";
        #endregion

        #region PUBLIC_VARS
        private bool SFX
        {
            get { return PlayerPrefbsHelper.GetInt(SFX_Prefs_key, 1) == 1; }
            set { PlayerPrefbsHelper.SetInt(SFX_Prefs_key, value ? 1 : 0); }
        }
        private bool Music
        {
            get { return PlayerPrefbsHelper.GetInt(Music_Prefs_key, 1) == 1; }
            set { PlayerPrefbsHelper.SetInt(Music_Prefs_key, value ? 1 : 0); }
        }
        public bool IsSFXOn
        {
            get => SFX;
            set => SetSFXBus(value, false);
        }

        public bool IsMusicOn
        {
            get => Music;
            set => SetMusicBus(value, true);
        }
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS

        public override void Awake()
        {
            base.Awake();
            Init();
            OnLoadingDone();
        }

        #endregion

        #region PUBLIC_METHODS
        public void Init()
        {
            for (int i = 0; i < _soundSOs.Length; i++)
            {
                _soundDict.Add(_soundSOs[i].soundType, _soundSOs[i]);
                _soundSOs[i].Init();
            }
            SetBusValue();
        }

        public void PlaySound(SoundType soundType, Action<bool> soundPlayResponse = null)
        {
            soundPlayResponse?.Invoke(_soundDict.ContainsKey(soundType));
            if (_soundDict.ContainsKey(soundType))
            {
                _soundDict[soundType].Play();
            }
        }

        public SoundInstance PlaySoundWithNewInstance(SoundType soundType)
        {
            if (_soundDict.ContainsKey(soundType))
            {
                return _soundDict[soundType].PlayWithNewInstance();
            }

            return null;
        }

        public void StopSound(SoundType soundType)
        {
            if (_soundDict.ContainsKey(soundType))
            {
                _soundDict[soundType].Stop();
            }
            else
            {
                throw new Exception("No Sound Type Found Please Create SO and Event For " + soundType);
            }
        }

        public void StopCoreMusic()
        {
            if (_soundDict.ContainsKey(_coreBackgroundSoundType))
            {
                _soundDict[_coreBackgroundSoundType].Stop(true);
            }
        }

        public void PlayCoreBackgrondMusic()
        {
            StartCoroutine(CoreSoundCoroutine());
        }

        private IEnumerator CoreSoundCoroutine()
        {
            yield return WaitForUtils.TenthSecond;
            if (_soundDict.ContainsKey(_coreBackgroundSoundType))
            {
                _soundDict[_coreBackgroundSoundType].Play();
            }
        }

        [Button]
        public void SetSFXBus(bool state, bool isAllowFadeOut = true)
        {
            SFX = state;

            if(!isAllowFadeOut)
                _sFXBus.setMute(!state);
            else
            {
                if (busFadeCoroutine != null)
                    StopCoroutine(busFadeCoroutine);

                busFadeCoroutine = StartCoroutine(FadeInOutBusVolume(_sFXBus, state ? 1f : 0f));
            }
        }

        [Button]
        public void SetMusicBus(bool state, bool isAllowFadeOut = true)
        {
            Music = state;

            if (!isAllowFadeOut)
                _musicBus.setMute(!state);
            else
            {
                if (busFadeCoroutine != null)
                    StopCoroutine(busFadeCoroutine);

                busFadeCoroutine = StartCoroutine(FadeInOutBusVolume(_musicBus, state ? 1f : 0f));
            }
        }
        #endregion

        #region PRIVATE_METHODS

        private void SetBusValue()
        {
            _musicBus = RuntimeManager.GetBus(MUSIC_BUS_PATH);
            _sFXBus = RuntimeManager.GetBus(SFX_BUS_PATH);
            SetMusicBus(IsMusicOn, false);
            SetSFXBus(IsSFXOn, false);
        }

        #endregion

        #region CO-ROUTINES
        IEnumerator FadeInOutBusVolume(Bus busToChange, float targetVolume, float fadeOutTime = 0.3f)
        {
            if (targetVolume > 0.99f)
                busToChange.setMute(false);

            float currentVolume = targetVolume > 0.99f ? 0f : 1f;
            float rate = 1 / fadeOutTime;
            float i = 0f;

            while (i <= 1f)
            {
                i += rate * Time.deltaTime;
                busToChange.setVolume(Mathf.Lerp(currentVolume, targetVolume, i));
                yield return null;
            }

            if (targetVolume < 0.01f)
                busToChange.setMute(true);
            busFadeCoroutine = null;
        }
        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       

        #endregion
    }
}
