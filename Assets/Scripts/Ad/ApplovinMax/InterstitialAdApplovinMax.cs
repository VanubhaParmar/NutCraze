//using GameAnalyticsSDK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.Ad
{
    public class InterstitialAdApplovinMax : BaseInterstitialAd
    {
        #region PUBLIC_VARS

        #endregion

        #region PRIVATE_VARS

#if UNITY_IOS
string adUnitId = "YOUR_IOS_AD_UNIT_ID";
#elif UNITY_ANDROID
        string adUnitId = "4abdbbd10b83b99d"; //dd0c5c4e9ab033ab
#endif
        int retryAttempt;

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public override void Init()
        {
            base.Init();

            // Attach callback
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;

            // Load the first interstitial
            LoadAd();
        }

        public override void LoadAd()
        {
            base.LoadAd();
            //AnalyticsManager.Instance.LogEvent_New_InterstitialAdRequested();
            MaxSdk.LoadInterstitial(adUnitId);
        }

        public override void ShowAd()
        {
            if (MaxSdk.IsInterstitialReady(adUnitId))
            {
                MaxSdk.ShowInterstitial(adUnitId);
            }
        }

        public override bool IsAdLoaded()
        {
            return MaxSdk.IsInterstitialReady(adUnitId);
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            retryAttempt = 0;
            //AnalyticsManager.Instance.LogEvent_New_InterstitialAdFilled();
            Debug.Log("Interstitial loaded");
        }

        private void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            //AnalyticsManager.Instance.LogEvent_New_InterstitialAdFailed();
            retryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, retryAttempt));

            Invoke("LoadAd", (float)retryDelay);

            Debug.Log("Interstitial failed to load with error code: " + errorInfo.Code);
        }

        private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            //DataManager.Instance.playerData.stats.totalInterstiAdWatched++;
            //DataManager.Instance.SavePlayerData();
            //AnalyticsManager.Instance.LogEvent_New_InterstitialAdShowed();
            //AnalyticsManager.Instance.LogEvent_INT_Watched();
            Debug.Log("Interstitial Displayed");
        }

        private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            LoadAd();
            Debug.Log("Interstitial failed to display with error code: " + errorInfo.Code);
        }

        private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("Interstitial clicked");
        }

        private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LoadAd();
            Debug.Log($"{adUnitId} was hidden");
        }

        #endregion

        #region UI_CALLBACKS       

        #endregion
    }
}
