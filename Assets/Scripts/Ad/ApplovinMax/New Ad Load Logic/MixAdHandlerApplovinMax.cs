using GameAnalyticsSDK;
using GameCoreSDK.Ads;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class MixAdHandlerApplovinMax : BaseRewardedAdHandler
    {
        #region PUBLIC_VARS

        #endregion

        #region PRIVATE_VARS

        [SerializeField] private string interstitialIdAndroid;
        [SerializeField] private string bannerAdIdAndroid;
        [SerializeField] private List<string> rewardedVideoIdsAndroid;
        [SerializeField] private List<string> rewardedInterstitialIdsAndroid;
        [SerializeField] private List<string> destoryRewardedVideoIdsAndroid;
        [SerializeField] private List<string> destoryInterstitialIdsAndroid;
        
        internal bool isRewardAdWatched = false;
        internal Action actionWatched;
        internal Action actionShowed;

        private bool isBannerAdLoaded;

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public override void Init()
        {
            InitializeRewardedAds();
            InitializeinterstitialAds();
        }

        public override void ShowAd(Action actionWatched, Action actionShowed = null, RewardAdShowCallType rewardAdShowCallType = RewardAdShowCallType.None)
        {
            this.actionShowed = actionShowed;
            this.actionWatched = () =>
            {
                Debug.Log("On Ad Task Completed");
                AdManager.Instance.OnRewardedAdShowed();
                actionWatched?.Invoke();
            };

            AdsController.GetInstance().ShowVideoAd();
        }

        public override bool IsAdLoaded()
        {
            return IsRewardedVideoAvailable();
        }

        public void LoadSimpleInterstitialAd()
        {
            if (AdManager.Instance.IsNoAdsPurchased()) return;
        }

        public void ShowSimpleInterstitialAd()
        {
            if (AdManager.Instance.IsNoAdsPurchased()) return;

            AdsController.GetInstance().ShowInterstitialAd();
        }

        public bool IsSimpleInterstitialAdLoaded()
        {
            return AdsController.GetInstance().IsInterstitialAdAvailable();
        }

        public void InitSimpleInterstitialAd()
        {
            LoadSimpleInterstitialAd();
        }

        public void InitBannerAd()
        {
            InitializeBannerAds();
        }

        public void HideBanner()
        {
            AdsController.GetInstance().HideBannerAd();
        }

        public Rect GetBannerAdRect()
        {
            return new Rect(0, 0, 0, 0);
        }

        public void ShowBanner()
        {
            AdsController.GetInstance().ShowBannerAd();
        }

        public bool IsBannerAdLoaded()
        {
            return isBannerAdLoaded;
        }

        public void OnBannerLoadSuccess()
        {
            isBannerAdLoaded = true;
        }
        #endregion

        #region PRIVATE_FUNCTIONS

        private void InitializeRewardedAds()
        {
            AdsController.GetInstance()._adsMediationCallbacks.OnVideoAdLoaded += OnRewardedAdLoadedEvent;
            AdsController.GetInstance()._adsMediationCallbacks.OnVideoAdClicked += OnRewardedAdClickedEvent;
            AdsController.GetInstance()._adsMediationCallbacks.OnVideoAdDisplayed += OnRewardedAdDisplayedEvent;
            AdsController.GetInstance()._adsMediationCallbacks.OnVideoAdDisplayFailed += OnRewardedAdFailedToDisplayEvent;
            AdsController.GetInstance()._adsMediationCallbacks.OnVideoAdHidden += OnRewardedAdDismissedEvent;
            AdsController.GetInstance()._adsMediationCallbacks.OnVideoAdGrantReward += OnRewardedAdReceivedRewardEvent;

            AdsController.GetInstance()._adsMediationCallbacks.OnAdRevenueReceived += OnRewardedAdRevenuePaidEvent;
        }

        private void InitializeinterstitialAds()
        {
            AdsController.GetInstance()._adsMediationCallbacks.OnInterstitialAdLoaded += OnInterstitialAdLoadedEvent;
            AdsController.GetInstance()._adsMediationCallbacks.OnInterstitialAdClicked += OnInterstitialAdClickedEvent;
            AdsController.GetInstance()._adsMediationCallbacks.OnInterstitialAdDisplayed += OnInterstitialAdDisplayedEvent;
            AdsController.GetInstance()._adsMediationCallbacks.OnInterstitialAdDisplayFailed += OnInterstitialAdFailedToDisplayEvent;
            AdsController.GetInstance()._adsMediationCallbacks.OnInterstitialAdHidden += OnInterstitialAdDismissedEvent;
        }

        private void InitializeBannerAds()
        {
            if (AdManager.Instance.IsNoAdsPurchased())
                return;
            AdsController.GetInstance()._adsMediationCallbacks.OnBannerAdClicked += OnBannerAdClickedEvent;
            AdsController.GetInstance()._adsMediationCallbacks.OnBannerAdLoaded += OnBannerAdLoadedEvent;
        }

        private bool IsRewardedVideoAvailable()
        {
            return AdsController.GetInstance().IsVideoAdAvailable();
        }
        #endregion

        #region CO-ROUTINES
        #endregion

        #region EVENT_HANDLERS
        private void OnRewardedAdLoadedEvent()
        {
            AnalyticsManager.Instance.LogEvent_AdGAEvent(GAAdAction.Loaded, GAAdType.RewardedVideo, AdManager.Instance.AdNameType);
        }

        private void OnInterstitialAdLoadedEvent()
        {
            AnalyticsManager.Instance.LogEvent_AdGAEvent(GAAdAction.Loaded, GAAdType.Interstitial, AdManager.Instance.AdNameType);
            Debug.Log("OnInterstitial AdLoadedEvent ");
        }

        private void OnRewardedAdFailedToDisplayEvent()
        {
            AnalyticsManager.Instance.LogEvent_AdGAEvent(GAAdAction.FailedShow, GAAdType.RewardedVideo, AdManager.Instance.AdNameType);
        }

        private void OnInterstitialAdFailedToDisplayEvent()
        {
            Debug.Log("OnInterstitial AdFailedToDisplayEvent ");
            LoadSimpleInterstitialAd();
            AnalyticsManager.Instance.LogEvent_AdGAEvent(GAAdAction.FailedShow, GAAdType.Interstitial, AdManager.Instance.AdNameType);
        }

        private void OnRewardedAdDisplayedEvent()
        {
            Debug.Log("Rewarded ad displayed");
            AnalyticsManager.Instance.LogEvent_AdGAEvent(GAAdAction.Show, GAAdType.RewardedVideo, AdManager.Instance.AdNameType);
        }

        private void OnInterstitialAdDisplayedEvent()
        {
            Debug.Log("OnInterstitial-Rewarded AdDisplayedEvent ");
            AnalyticsManager.Instance.LogEvent_AdGAEvent(GAAdAction.Show, GAAdType.RewardedVideo, AdManager.Instance.AdNameType);
        }

        private void OnRewardedAdClickedEvent()
        {
            Debug.Log("Rewarded ad clicked");
            AnalyticsManager.Instance.LogEvent_AdGAEvent(GAAdAction.Clicked, GAAdType.RewardedVideo, AdManager.Instance.AdNameType);
        }

        private void OnInterstitialAdClickedEvent()
        {
            Debug.Log("OnInterstitial-Rewarded AdClickedEvent ");
            AnalyticsManager.Instance.LogEvent_AdGAEvent(GAAdAction.Clicked, GAAdType.RewardedVideo, AdManager.Instance.AdNameType);
        }

        private void OnRewardedAdDismissedEvent()
        {
            Debug.Log("Rewarded ad dismissed");
            if (actionWatched != null && (isRewardAdWatched))
            {
                Debug.Log("Rewarded ad watched");
                actionWatched();
            }
            else if (actionShowed != null)
            {
                Debug.Log("Rewarded ad showed");
                actionShowed();
            }
            Debug.Log("OnRewarded AdDismissedEvent ");
            isRewardAdWatched = false;
        }

        private void OnInterstitialAdDismissedEvent()
        {
            Debug.Log("OnInterstitial AdDismissedEvent ");
        }


        private void OnRewardedAdReceivedRewardEvent()//string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("Rewarded ad received reward");
            isRewardAdWatched = true;
            AnalyticsManager.Instance.LogEvent_AdGAEvent(GAAdAction.RewardReceived, GAAdType.RewardedVideo, AdManager.Instance.AdNameType);
        }

        private void OnRewardedAdRevenuePaidEvent(string platform, string source, string format, string adUnitName, double value, string currency)//string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            AnalyticsManager.Instance.LogEvent_FirebaseAdRevenueAppLovin(platform, source, adUnitName, format, value, currency);

            double ecpmRewarded = value * (100000);
            SendFirebaseRevenueEvent("CPM_greaterthan_1000", 100000, ecpmRewarded);
            SendFirebaseRevenueEvent("CPM_greaterthan_500", 50000, ecpmRewarded);
            SendFirebaseRevenueEvent("CPM_greaterthan_100", 10000, ecpmRewarded);
            SendFirebaseRevenueEvent("CPM_greaterthan_10", 1000, ecpmRewarded);
        }

        private void OnBannerAdClickedEvent()
        {
            Debug.Log("Banner Ad Clicked");
            AnalyticsManager.Instance.LogEvent_AdGAEvent(GAAdAction.Clicked, GAAdType.Banner, "");
        }

        private void OnBannerAdLoadedEvent()
        {
            Debug.Log("Banner Ad Loaded ");
            AnalyticsManager.Instance.LogEvent_AdGAEvent(GAAdAction.Loaded, GAAdType.Banner, "");
            AnalyticsManager.Instance.LogEvent_AdGAEvent(GAAdAction.Show, GAAdType.Banner, "");
            OnBannerLoadSuccess();
        }
        private void SendFirebaseRevenueEvent(string key, double threshold, double ecpmValue)
        {
            AnalyticsManager.Instance.LogEvent_FirebaseAdRevanueEvent(key, threshold, ecpmValue);
        }
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS       

        #endregion
    }

}
