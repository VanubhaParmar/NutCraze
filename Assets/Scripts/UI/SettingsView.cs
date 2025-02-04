using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort
{
    public class SettingsView : BaseView
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private SettingToggle soundToggle;
        [SerializeField] private SettingToggle vibrationToggle;

        [Space]
        [SerializeField] private Text buildVersionAndCodeText;
        private const string BuildVersionCodeFormat = "v {0} ({1})";
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public override void Show(Action action = null, bool isForceShow = false)
        {
            GameStatsCollector.Instance.OnPopUpTriggered();

            base.Show(action, isForceShow);
            MainSceneUIManager.Instance.GetView<BannerAdsView>().Hide();

            SetView();
        }

        public override void Hide()
        {
            MainSceneUIManager.Instance.GetView<BannerAdsView>().Show(true);
            base.Hide();
        }
        #endregion

        #region PRIVATE_METHODS
        private void InitToggles()
        {
            soundToggle.InitView(GetSoundToggleValue, SetSoundToggleValue);
            vibrationToggle.InitView(GetVibrationToggleValue, SetVibrationToggleValue);
        }

        private bool GetSoundToggleValue()
        {
            return SoundHandler.Instance.IsSFXOn;
        }

        private void SetSoundToggleValue(bool state)
        {
            SoundHandler.Instance.IsSFXOn = state;
            SoundHandler.Instance.IsMusicOn = state;
        }

        private bool GetVibrationToggleValue()
        {
            return Vibrator.IsVibrateOn;
        }

        private void SetVibrationToggleValue(bool state)
        {
            Vibrator.IsVibrateOn = state;
        }

        private void SetView()
        {
            InitToggles();
            buildVersionAndCodeText.text = Constant.BuildVersionCodeString;
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        public void OnButtonClick_Terms()
        {
            Application.OpenURL(GameManager.Instance.GameMainDataSO.termsLink);
        }

        public void OnButtonClick_PrivacyPolicy()
        {
            Application.OpenURL(GameManager.Instance.GameMainDataSO.privacyPolicyLink);
        }
        #endregion
    }
}