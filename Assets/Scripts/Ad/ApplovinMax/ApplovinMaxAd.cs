using GameAnalyticsSDK;
using GameCoreSDK.Ads;
using System;
using Tag.NutSort;
using UnityEngine;

namespace Tag.Ad
{
    public class ApplovinMaxAd : BaseAd
    {
        #region PUBLIC_VARS
        #endregion

        #region PRIVATE_VARS

        #endregion

        #region UNITY_CALLBACKS
        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                AdjustManager.Instance.Adjust_GamePauseEvent();
                AdsController.GetInstance().OnPauseGame();
            }
            else
            {
                AdsController.GetInstance().OnResumeGame();
            }
        }
        #endregion

        #region PUBLIC_FUNCTIONS

        public override void Init(Action actionToCallOnInitSuccess = null)
        {
            base.Init(actionToCallOnInitSuccess);
            AdsController.GetInstance().Initialize(TimeManager.Instance.InstallUnixTime, DevProfileHandler.Instance.CurrentDevelopmentProfile.isApplovinTstMode, () =>
            {
                MainThreadDispatcher.ExecuteOnMainThread(() =>
                {
                    Debug.Log($"Initialized Ads Controller with Install Time : {TimeManager.Instance.InstallUnixTime} Test Mode : {DevProfileHandler.Instance.CurrentDevelopmentProfile.isApplovinTstMode}");
                    OnApplovinMaxInitialized(true);
                    GameAnalyticsILRD.SubscribeMaxImpressions();
                });
            });
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        private void OnApplovinMaxInitialized(bool success, string error = "")
        {
            if (success)
            {
                Debug.Log("<APPLOVIN MAX> Initialized Successfully! ");
                baseRewardedAdHandler.Init();
                baseInterstitialAd.Init();
                baseBannerAd.Init();

                OnInitSuccess();
            }
            else
            {
                Debug.Log("<APPLOVIN MAX> Initialized failed! with error : " + error);
            }
        }
        #endregion

        #region CO-ROUTINES
        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       

        #endregion
    }
}
