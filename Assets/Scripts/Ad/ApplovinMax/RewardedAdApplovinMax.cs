using System;

namespace Tag.NutSort {
    public class RewardedAdApplovinMax : BaseRewardedAd
    {
        #region PUBLIC_VARS

        #endregion

        #region PRIVATE_VARS

#if UNITY_IOS
string adUnitId = "YOUR_IOS_AD_UNIT_ID";
#elif UNITY_ANDROID
        public string adUnitId = "d2ed3c086971e206"; //2039b2cb121d0567
#endif
        int retryAttempt;

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public override void Init()
        {
            // Attach callback
            // MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            // MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
            // MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            // MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
            // MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;
            // MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
            // MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
            // MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;

            // Load the first rewarded ad
            base.Init();
        }

        public override void LoadAd()
        {
            base.LoadAd();
            // MaxSdk.LoadRewardedAd(adUnitId);
        }

        public override void ShowAd(Action actionWatched, Action actionShowed = null)
        {
            base.ShowAd(actionWatched, actionShowed);
            // MaxSdk.ShowRewardedAd(adUnitId);
        }

        public override bool IsAdLoaded()
        {
            return false;
            // return MaxSdk.IsRewardedAdReady(adUnitId);
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        // private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        // {
        //     retryAttempt = 0;
        //     OnVideoReady();
        // }

        // private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        // {
        //     retryAttempt++;
        //     double retryDelay = Math.Pow(2, Math.Min(6, retryAttempt));
        //     OnVideoFail();
        // }

        // private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        // {

        // }

        // private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        // {
        //     OnVideoFailToShow();
        // }

        // private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        // {

        // }

        // private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        // {
        //     OnVideoAdDismiss();
        // }

        // private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
        // {
        //     isRewardAdWatched = true;
        // }

        // private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        // {
        //     // Ad revenue paid. Use this callback to track user revenue.
        //     //AdjustManager.Instance.TrackAdRevenue(adInfo);
        // }

        #endregion

        #region UI_CALLBACKS       

        #endregion
    }
}
