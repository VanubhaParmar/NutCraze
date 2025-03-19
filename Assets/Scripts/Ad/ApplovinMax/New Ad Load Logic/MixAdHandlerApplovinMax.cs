//using GameAnalyticsSDK;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tag.NutSort;
using GameAnalyticsSDK;
using GameCoreSDK.Ads;
using Mediation.Runtime.Scripts;

namespace Tag.Ad
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

        [ShowInInspector, ReadOnly] private static Dictionary<string, double> revenuMapping = new Dictionary<string, double>();

        internal bool isRewardAdWatched = false;
        internal Action actionWatched;
        internal Action actionShowed;
        private DateTime adShowTime = DateTime.MinValue;
        private string adInfo;

        private float tryInternetCheckWait = 5f;

        private bool isBannerAdLoaded;

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public override void Init()
        {
            this.adInfo = "None";
            InitializeRewardedAds();
            InitializeinterstitialAds();
        }

        public override void ShowAd(Action actionWatched, Action actionShowed = null, RewardAdShowCallType rewardAdShowCallType = RewardAdShowCallType.None)
        {
            this.adInfo = rewardAdShowCallType.ToString();
            this.actionShowed = actionShowed;
            this.actionWatched = () => 
            {
                AdManager.Instance.OnRewardedAdShowed();
                actionWatched?.Invoke();
            };

            AdsController.GetInstance().ShowVideoAd();

            //AdType adType = GetHighestCMPAdType();
            //if (adType == AdType.RewardedAd)
            //{
            //    ShowRewardedVideo();
            //    //AnalyticsManager.Instance.LogEvent_RV_Watched();
            //}
            //else if (adType == AdType.InterstitialAd)
            //{
            //    ShowInterstitial();
            //}
            //AnalyticsManager.Instance.AdGAEvent(GAAdAction.Show, GAAdType.RewardedVideo, adInfo);
            //DataManager.Instance.playerData.stats.totalVideoAdWatched++;
            //DataManager.Instance.SavePlayerData();
            //AnalyticsManager.Instance.LogEvent_New_RewardedAdInfo(rewardAdShowCallType.ToString());
        }

        public override bool IsAdLoaded()
        {
            return IsRewardedVideoAvailable();// || IsInterstitialAvailable();
        }

        public override void LoadAd()
        {
            //LoadRewardedVideo();
            //LoadInterstitial();
        }

        //Logic For Simple Inter Ad.

        public void LoadSimpleInterstitialAd()
        {
            if (AdManager.Instance.IsNoAdsPurchased()) return;

            // MaxSdk.LoadInterstitial(interstitialIdAndroid);
        }

        public void ShowSimpleInterstitialAd()
        {
            if (AdManager.Instance.IsNoAdsPurchased()) return;

            // MaxSdk.ShowInterstitial(interstitialIdAndroid);
            AdsController.GetInstance().ShowInterstitialAd();
        }

        public bool IsSimpleInterstitialAdLoaded()
        {
            //return false;
            // return MaxSdk.IsInterstitialReady(interstitialIdAndroid);
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

        public void LoadBanner()
        {
            try
            {
                if (!isBannerAdLoaded)
                {
                    ForceStopBannerAds();
                    CreateBannerAd();
                }
                else {}
                    // MaxSdk.LoadBanner(bannerAdIdAndroid);
            }
            catch (Exception e)
            {
                Debug.LogError("LoadBanner Exception " + e);
            }
        }

        public void HideBanner()
        {
            // MaxSdk.HideBanner(bannerAdIdAndroid);
            AdsController.GetInstance().HideBannerAd();
        }

        public void StartBannerAdsAutoRefresh()
        {
            // MaxSdk.StartBannerAutoRefresh(bannerAdIdAndroid);
        }
        public void ForceStopBannerAds()
        {
            // MaxSdk.DestroyBanner(bannerAdIdAndroid);
            // MaxSdk.StopBannerAutoRefresh(bannerAdIdAndroid);
        }

        public Rect GetBannerAdRect()
        {
            return new Rect(0, 0, 0, 0);
            // return MaxSdk.GetBannerLayout(bannerAdIdAndroid);
        }

        public void ShowBanner()
        {
            // MaxSdk.ShowBanner(bannerAdIdAndroid);
            AdsController.GetInstance().ShowBannerAd();
        }

        public bool IsBannerAdLoaded()
        {
            return isBannerAdLoaded;
        }

        public void OnBannerLoadSuccess()
        {
            isBannerAdLoaded = true;
            //AdManager.Instance.ShowBannerAd();
        }

        public void OnBannerLoadFail()
        {
            LoadBanner();
        }
        #endregion

        #region PRIVATE_FUNCTIONS

        private void InitializeRewardedAds()
        {
            AdsNativeCallbacks.Instance.OnVideoAdLoaded += OnRewardedAdLoadedEvent;
            AdsNativeCallbacks.Instance.OnVideoAdClicked += OnRewardedAdClickedEvent;
            AdsNativeCallbacks.Instance.OnVideoAdDisplayed += OnRewardedAdDisplayedEvent;
            AdsNativeCallbacks.Instance.OnVideoAdDisplayFailed += OnRewardedAdFailedToDisplayEvent;
            AdsNativeCallbacks.Instance.OnVideoAdHidden += OnRewardedAdDismissedEvent;
            AdsNativeCallbacks.Instance.OnVideoAdGrantReward += OnRewardedAdReceivedRewardEvent;

            AdsNativeCallbacks.Instance.OnAdRevenueReceived += OnRewardedAdRevenuePaidEvent;

            // MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            // MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdFailedEvent;
            // MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
            // MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            // MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
            // MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdDismissedEvent;
            // MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
            // MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;
            //LoadRewardedVideo();
        }

        private void InitializeinterstitialAds()
        {
            // TODO : Return if no ads pack purchased

            AdsNativeCallbacks.Instance.OnInterstitialAdLoaded += OnInterstitialAdLoadedEvent;
            AdsNativeCallbacks.Instance.OnInterstitialAdClicked += OnInterstitialAdClickedEvent;
            AdsNativeCallbacks.Instance.OnInterstitialAdDisplayed += OnInterstitialAdDisplayedEvent;
            AdsNativeCallbacks.Instance.OnInterstitialAdDisplayFailed += OnInterstitialAdFailedToDisplayEvent;
            AdsNativeCallbacks.Instance.OnInterstitialAdHidden += OnInterstitialAdDismissedEvent;

            // MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialAdLoadedEvent;
            // MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialAdFailedEvent;
            // MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;
            // MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialAdDisplayedEvent;
            // MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialAdClickedEvent;
            // MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialAdDismissedEvent;
            // MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnInterstitialAdRevenuePaidEvent;
            //LoadInterstitial();
        }

        private void InitializeBannerAds()
        {
            if (AdManager.Instance.IsNoAdsPurchased()) return;

            // TODO : Return if no ads pack purchased
            CreateBannerAd();

            AdsNativeCallbacks.Instance.OnBannerAdClicked += OnBannerAdClickedEvent;
            AdsNativeCallbacks.Instance.OnBannerAdLoaded += OnBannerAdLoadedEvent;

            // MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
            // MaxSdkCallbacks.Banner.OnAdCollapsedEvent += OnBannerAdCollapsedEvent;
            // MaxSdkCallbacks.Banner.OnAdExpandedEvent += OnAdExpandedEvent;
            // MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
            // MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdLoadFailedEvent;
            // MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
            //LoadBanner();
        }

        private void CreateBannerAd()
        {
            // MaxSdk.CreateBanner(bannerAdIdAndroid, MaxSdkBase.BannerPosition.BottomCenter);
            // MaxSdk.SetBannerBackgroundColor(bannerAdIdAndroid, Color.clear);
            // MaxSdk.SetBannerExtraParameter(bannerAdIdAndroid, "adaptive_banner", "true");

            // MaxSdk.StartBannerAutoRefresh(bannerAdIdAndroid);
        }

        //private void LoadRewardedVideo()
        //{
        //    StartCoroutine(LoadRewardAdCO());
        //}

        //private void LoadInterstitial()
        //{
        //    StartCoroutine(LoadInterstitialCO());
        //}

        //private bool IsInterstitialAvailable()
        //{
        //    string id = GetHighestCPMInterstitialId();
        //    if (revenuMapping.ContainsKey(id))
        //    {
        //        // if (MaxSdk.IsInterstitialReady(id))
        //        //     return true;
        //    }
        //    return false;
        //}

        private bool IsRewardedVideoAvailable()
        {
            return AdsController.GetInstance().IsVideoAdAvailable();
            //string id = GetHighestCPMRewardedId();
            //if (revenuMapping.ContainsKey(id))
            //{
            //    // if (MaxSdk.IsRewardedAdReady(id))
            //    //     return true;
            //}
            //return false;
        }

        //private void ShowRewardedVideo()
        //{
        //    string id = GetHighestCPMRewardedId();
        //    adShowTime = DateTime.Now;
        //    // MaxSdk.ShowRewardedAd(id);
        //    Debug.Log("Highest Ecpm Rewarded Ad :- " + id);
        //}

        //private void ShowInterstitial()
        //{
        //    string id = GetHighestCPMInterstitialId();
        //    // if (MaxSdk.IsInterstitialReady(id))
        //    // {
        //    //     Debug.Log("Highest Ecpm Interstitial Ad :- " + id);
        //    //     MaxSdk.ShowInterstitial(id);
        //    // }
        //    // else
        //    // {
        //    //     LoadInterstitial();
        //    // }
        //}

        //private string GetHighestCPMRewardedId()
        //{
        //    double highestCPM = 0;
        //    string id = null;
        //    foreach (var item in revenuMapping)
        //    {
        //        if (rewardedVideoIdsAndroid.Contains(item.Key))
        //        {
        //            if (highestCPM < item.Value)
        //            {
        //                highestCPM = item.Value;
        //                id = item.Key;
        //            }
        //        }
        //    }
        //    if (string.IsNullOrEmpty(id))
        //    {
        //        return rewardedVideoIdsAndroid[0];
        //    }
        //    return id;
        //}

        //private string GetHighestCPMInterstitialId()
        //{
        //    double highestCPM = 0;
        //    string id = null;

        //    foreach (var item in revenuMapping)
        //    {
        //        if (rewardedInterstitialIdsAndroid.Contains(item.Key))
        //        {
        //            if (highestCPM < item.Value)
        //            {
        //                highestCPM = item.Value;
        //                id = item.Key;
        //            }
        //        }
        //    }
        //    if (string.IsNullOrEmpty(id))
        //    {
        //        return rewardedInterstitialIdsAndroid[0];
        //    }
        //    return id;
        //}

        //private List<string> GetLoadRewardedAdId()
        //{
        //    List<string> ids = new List<string>();
        //    if (revenuMapping.Count <= 0)
        //    {
        //        ids.AddRange(rewardedVideoIdsAndroid);
        //    }
        //    else
        //    {
        //        for (int i = 0; i < rewardedVideoIdsAndroid.Count; i++)
        //        {
        //            if (!revenuMapping.ContainsKey(rewardedVideoIdsAndroid[i]))
        //                ids.Add(rewardedVideoIdsAndroid[i]);
        //        }
        //    }
        //    if (ids.Count <= 0)
        //        ids.Add(rewardedVideoIdsAndroid[0]);
        //    return ids;
        //}

        //private List<string> GetLoadInterstitialAdId()
        //{
        //    List<string> ids = new List<string>();
        //    if (revenuMapping.Count <= 0)
        //    {
        //        ids.AddRange(rewardedInterstitialIdsAndroid);
        //    }
        //    else
        //    {
        //        for (int i = 0; i < rewardedInterstitialIdsAndroid.Count; i++)
        //        {
        //            if (!revenuMapping.ContainsKey(rewardedInterstitialIdsAndroid[i]))
        //                ids.Add(rewardedInterstitialIdsAndroid[i]);
        //        }
        //    }
        //    if (ids.Count <= 0)
        //        ids.Add(rewardedInterstitialIdsAndroid[0]);
        //    return ids;
        //}

        //private AdType GetHighestCMPAdType()
        //{
        //    foreach (var item in revenuMapping)
        //    {
        //        Debug.Log("revenuMapping " + item.Key + "____" + item.Value);
        //    }

        //    string highestCPMRewardedId = GetHighestCPMRewardedId();
        //    string highestCPMInterstitialId = GetHighestCPMInterstitialId();
        //    if (revenuMapping.ContainsKey(highestCPMRewardedId) && revenuMapping.ContainsKey(highestCPMInterstitialId))
        //    {
        //        Debug.Log("revenuMapping.ContainsKey(highestCPMRewardedId) " + highestCPMRewardedId);
        //        Debug.Log("revenuMapping.ContainsKey(highestCPMInterstitialId) " + highestCPMInterstitialId);

        //        Debug.Log("revenuMapping[highestCPMRewardedId] " + revenuMapping[highestCPMRewardedId]);
        //        Debug.Log("revenuMapping[highestCPMInterstitialId] " + revenuMapping[highestCPMInterstitialId]);

        //        if (revenuMapping[highestCPMRewardedId] < revenuMapping[highestCPMInterstitialId])
        //        {
        //            return AdType.InterstitialAd;
        //        }
        //    }
        //    return AdType.RewardedAd;
        //}

        #endregion

        #region CO-ROUTINES

        //IEnumerator LoadRewardAdCO()
        //{
        //    List<string> ids = GetLoadRewardedAdId();
        //    for (int i = 0; i < ids.Count; i++)
        //    {
        //        //AnalyticsManager.Instance.LogEvent_New_RewardedAdRequested();
        //        //AnalyticsManager.Instance.AdGAEvent(GAAdAction.Request, GAAdType.RewardedVideo, this.adInfo);
        //        // MaxSdk.LoadRewardedAd(ids[i]);
        //        yield return new WaitForSeconds(2);
        //    }
        //}

        //IEnumerator LoadInterstitialCO()
        //{
        //    List<string> ids = GetLoadInterstitialAdId();
        //    for (int i = 0; i < ids.Count; i++)
        //    {
        //        // MaxSdk.LoadInterstitial(ids[i]);
        //        yield return new WaitForSeconds(2);
        //    }
        //}

        #endregion

        #region EVENT_HANDLERS

        private void OnRewardedVideoLoadFail()
        {
            //AnalyticsManager.Instance.LogEvent_New_RewardedAdFailed();
            //LoadRewardedVideo();
        }

        private void OnInterstitialLoadFail()
        {
            //LoadInterstitial();
        }

        private void OnRewardedAdLoadedEvent()//string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            //if (!revenuMapping.ContainsKey(adUnitId))
            //    revenuMapping.Add(adUnitId, adInfo.Revenue);
            //else
            //    revenuMapping[adUnitId] = adInfo.Revenue;
            //AnalyticsManager.Instance.LogEvent_New_RewardedAdFilled();
            AnalyticsManager.Instance.LogEvent_AdGAEvent(GAAdAction.Loaded, GAAdType.RewardedVideo, AdManager.Instance.AdNameType);
        }

         private void OnInterstitialAdLoadedEvent()//string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            //if (interstitialIdAndroid == adUnitId)
            //{
                AnalyticsManager.Instance.LogEvent_AdGAEvent(GAAdAction.Loaded, GAAdType.Interstitial, AdManager.Instance.AdNameType);
            Debug.Log("OnInterstitial AdLoadedEvent ");// + adUnitId);
                //AnalyticsManager.Instance.LogEvent_New_InterstitialAdFilled();
            //    return;
            //}

            //AnalyticsManager.Instance.LogEvent_AdGAEvent(GAAdAction.Loaded, GAAdType.RewardedVideo, AdManager.Instance.AdNameType);
            //AnalyticsManager.Instance.LogEvent_New_RewardedAdFilled();
            //Debug.Log("OnInterstitial-Rewarded AdLoadedEvent ");// + adUnitId);
            //if (!revenuMapping.ContainsKey(adUnitId))
            //    revenuMapping.Add(adUnitId, adInfo.Revenue);
            //else
            //    revenuMapping[adUnitId] = adInfo.Revenue;
        }

        // private void OnRewardedAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        // {
        //     if (revenuMapping.ContainsKey(adUnitId))
        //     {
        //         revenuMapping.Remove(adUnitId);
        //     }

        //     //OnRewardedVideoLoadFail();
        //     //AnalyticsManager.Instance.AdGAEvent(GAAdAction.FailedShow, GAAdType.RewardedVideo, adInfo);
        //     this.adInfo = "None";

        //     StartCoroutine(CheckInternetConnectionAndRetryLoadAdCall(OnRewardedVideoLoadFail));
        // }

        // private void OnInterstitialAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        // {
        //     if (interstitialIdAndroid == adUnitId)
        //     {
        //         Debug.Log("OnInterstitial AdFailedEvent " + adUnitId);
        //         //AnalyticsManager.Instance.LogEvent_New_InterstitialAdFailed();
        //         //AnalyticsManager.Instance.AdGAEvent(GAAdAction.FailedShow, GAAdType.Interstitial, "None");
        //         //LoadSimpleInterstitialAd();
        //         StartCoroutine(CheckInternetConnectionAndRetryLoadAdCall(LoadSimpleInterstitialAd));
        //         return;
        //     }

        //     Debug.Log("OnInterstitial-Rewarded AdFailedEvent " + adUnitId);
        //     if (revenuMapping.ContainsKey(adUnitId))
        //     {
        //         revenuMapping.Remove(adUnitId);
        //     }
        //     //OnInterstitialLoadFail();

        //     StartCoroutine(CheckInternetConnectionAndRetryLoadAdCall(OnInterstitialLoadFail));
        // }

        private void OnRewardedAdFailedToDisplayEvent()//string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            //if (revenuMapping.ContainsKey(adUnitId))
            //{
            //    revenuMapping.Remove(adUnitId);
            //}
            //OnRewardedVideoLoadFail();
            this.adInfo = "None";
            AnalyticsManager.Instance.LogEvent_AdGAEvent(GAAdAction.FailedShow, GAAdType.RewardedVideo, AdManager.Instance.AdNameType);
        }

         private void OnInterstitialAdFailedToDisplayEvent()//string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            //if (interstitialIdAndroid == adUnitId)
            //{
            Debug.Log("OnInterstitial AdFailedToDisplayEvent ");// + adUnitId);
            LoadSimpleInterstitialAd();
            AnalyticsManager.Instance.LogEvent_AdGAEvent(GAAdAction.FailedShow, GAAdType.Interstitial, AdManager.Instance.AdNameType);

            //    return;
            //}

            //AnalyticsManager.Instance.LogEvent_AdGAEvent(GAAdAction.FailedShow, GAAdType.RewardedVideo, AdManager.Instance.AdNameType);
            //Debug.Log("OnInterstitial-Rewarded AdFailedToDisplayEvent " + adUnitId);
            //if (revenuMapping.ContainsKey(adUnitId))
            //{
            //    revenuMapping.Remove(adUnitId);
            //}
            //OnInterstitialLoadFail();
        }

        private void OnRewardedAdDisplayedEvent()//string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("Rewarded ad displayed");
            AnalyticsManager.Instance.LogEvent_AdGAEvent(GAAdAction.Show, GAAdType.RewardedVideo, AdManager.Instance.AdNameType);
        }

         private void OnInterstitialAdDisplayedEvent()//string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            //if (interstitialIdAndroid == adUnitId)
            //{
            //    Debug.Log("OnInterstitial AdDisplayedEvent " + adUnitId);
            //    AnalyticsManager.Instance.LogEvent_AdGAEvent(GAAdAction.Show, GAAdType.Interstitial, AdManager.Instance.AdNameType);
            //    //DataManager.Instance.playerData.stats.totalInterstiAdWatched++;
            //    //DataManager.Instance.SavePlayerData();
            //    //AnalyticsManager.Instance.LogEvent_New_InterstitialAdShowed();
            //    //AnalyticsManager.Instance.LogEvent_INT_Watched();
            //    return;
            //}

            Debug.Log("OnInterstitial-Rewarded AdDisplayedEvent ");// + adUnitId);
            AnalyticsManager.Instance.LogEvent_AdGAEvent(GAAdAction.Show, GAAdType.RewardedVideo, AdManager.Instance.AdNameType);
        }

        private void OnRewardedAdClickedEvent()//string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("Rewarded ad clicked");
            AnalyticsManager.Instance.LogEvent_AdGAEvent(GAAdAction.Clicked, GAAdType.RewardedVideo, AdManager.Instance.AdNameType);
        }

        private void OnInterstitialAdClickedEvent()//string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            //if (interstitialIdAndroid == adUnitId)
            //{
            //    AnalyticsManager.Instance.LogEvent_AdGAEvent(GAAdAction.Clicked, GAAdType.Interstitial, AdManager.Instance.AdNameType);
            //    Debug.Log("OnInterstitial AdClickedEvent " + adUnitId);
            //    return;
            //}

            Debug.Log("OnInterstitial-Rewarded AdClickedEvent ");// + adUnitId);
            AnalyticsManager.Instance.LogEvent_AdGAEvent(GAAdAction.Clicked, GAAdType.RewardedVideo, AdManager.Instance.AdNameType);
        }

        private void OnRewardedAdDismissedEvent()//string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            //RemoveRewardedAdIdsFromMapping(adUnitId);
            //double seconds = 100;
            //if (adShowTime != DateTime.MinValue)
            //    seconds = DateTime.Now.Subtract(adShowTime).TotalSeconds;
            if (actionWatched != null && (isRewardAdWatched))// || seconds > 15))
            {
                actionWatched();
            }
            else if (actionShowed != null)
            {
                actionShowed();
            }
            //LoadRewardedVideo();
            isRewardAdWatched = false;
        }

        // private void RemoveRewardedAdIdsFromMapping(string adUnitId)
        // {
        //     if (!destoryRewardedVideoIdsAndroid.Contains(adUnitId))
        //     {
        //         for (int i = 0; i < destoryRewardedVideoIdsAndroid.Count; i++)
        //         {
        //             if (revenuMapping.ContainsKey(destoryRewardedVideoIdsAndroid[i]) && MaxSdk.IsRewardedAdReady(destoryRewardedVideoIdsAndroid[i]))
        //             {
        //                 revenuMapping.Remove(destoryRewardedVideoIdsAndroid[i]);
        //                 Debug.Log("RemoveRewardedAdIdsFromMapping  " + destoryRewardedVideoIdsAndroid[i]);
        //             }
        //         }
        //     }
        //     if (revenuMapping.ContainsKey(adUnitId))
        //     {
        //         revenuMapping.Remove(adUnitId);
        //     }
        // }

        private void OnInterstitialAdDismissedEvent()// string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            //if (interstitialIdAndroid == adUnitId)
            //{
            Debug.Log("OnInterstitial AdDismissedEvent ");// + adUnitId);
            //    LoadSimpleInterstitialAd();
            //    return;
            //}

            //AnalyticsManager.Instance.LogEvent_AdGAEvent(GAAdAction.RewardReceived, GAAdType.Interstitial, AdManager.Instance.AdNameType);

            //Debug.Log("OnInterstitial-Rewarded AdDismissedEvent " + adUnitId);
            //RemoveInterstitialAdIdsFromMapping(adUnitId);
            //LoadInterstitial();
            //if (actionWatched != null && (isRewardAdWatched))
            //{
            //    actionWatched();
            //}
            //else if (actionShowed != null)
            //{
            //    actionShowed();
            //}
            //isRewardAdWatched = false;
        }

        // private void RemoveInterstitialAdIdsFromMapping(string adUnitId)
        // {
        //     if (!destoryInterstitialIdsAndroid.Contains(adUnitId))
        //     {
        //         for (int i = 0; i < destoryInterstitialIdsAndroid.Count; i++)
        //         {
        //             if (revenuMapping.ContainsKey(destoryInterstitialIdsAndroid[i]) && MaxSdk.IsRewardedAdReady(destoryInterstitialIdsAndroid[i]))
        //             {
        //                 revenuMapping.Remove(destoryInterstitialIdsAndroid[i]);
        //                 Debug.Log("RemoveInterstitialAdIdsFromMapping  " + destoryInterstitialIdsAndroid[i]);
        //             }
        //         }
        //     }
        //     if (revenuMapping.ContainsKey(adUnitId))
        //     {
        //         revenuMapping.Remove(adUnitId);
        //     }
        // }

        private void OnRewardedAdReceivedRewardEvent()//string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("Rewarded ad received reward");
            isRewardAdWatched = true;
            AnalyticsManager.Instance.LogEvent_AdGAEvent(GAAdAction.RewardReceived, GAAdType.RewardedVideo, AdManager.Instance.AdNameType);
            this.adInfo = "None";
        }

        // private void OnInterstitialAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        // {
        //     AnalyticsManager.Instance.LogEvent_FirebaseAdRevenueAppLovin(adInfo.NetworkName, adInfo.AdUnitIdentifier, adInfo.AdFormat, adInfo.Revenue);
        //     AdjustManager.Instance.TrackAdRevenue(adInfo);

        //     double ecpmRewarded = adInfo.Revenue * (100000);
        //     SendFirebaseRevenueEvent("CPM_greaterthan_1000", 100000, ecpmRewarded);
        //     SendFirebaseRevenueEvent("CPM_greaterthan_500", 50000, ecpmRewarded);
        //     SendFirebaseRevenueEvent("CPM_greaterthan_100", 10000, ecpmRewarded);
        //     SendFirebaseRevenueEvent("CPM_greaterthan_10", 1000, ecpmRewarded);

        //     if (interstitialIdAndroid == adUnitId)
        //     {
        //         Debug.Log("OnInterstitial AdRevenuePaidEvent " + adUnitId);
        //         return;
        //     }

        //     Debug.Log("OnInterstitial-Rewarded AdRevenuePaidEvent " + adUnitId);
        //     isRewardAdWatched = true;
        // }

        private void OnRewardedAdRevenuePaidEvent(string platform, string source, string format, string adUnitName, double value, string currency)//string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            AnalyticsManager.Instance.LogEvent_FirebaseAdRevenueAppLovin(platform, source, adUnitName, format, value, currency);

            double ecpmRewarded = value * (100000);
            SendFirebaseRevenueEvent("CPM_greaterthan_1000", 100000, ecpmRewarded);
            SendFirebaseRevenueEvent("CPM_greaterthan_500", 50000, ecpmRewarded);
            SendFirebaseRevenueEvent("CPM_greaterthan_100", 10000, ecpmRewarded);
            SendFirebaseRevenueEvent("CPM_greaterthan_10", 1000, ecpmRewarded);

            //AdjustManager.Instance.TrackAdRevenue(adInfo);
        }

        private void OnBannerAdClickedEvent()//string arg1, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("Banner Ad Clicked");
            AnalyticsManager.Instance.LogEvent_AdGAEvent(GAAdAction.Clicked, GAAdType.Banner, "");
        }

        // private void OnBannerAdCollapsedEvent(string arg1, MaxSdkBase.AdInfo adInfo)
        // {
        //     Debug.Log("Banner Ad Collapsed");
        // }

        // private void OnAdExpandedEvent(string arg1, MaxSdkBase.AdInfo adInfo)
        // {
        //     Debug.Log("Banner Ad Expanded");
        // }

        private void OnBannerAdLoadedEvent()//string arg1, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("Banner Ad Loaded ");
            AnalyticsManager.Instance.LogEvent_AdGAEvent(GAAdAction.Loaded, GAAdType.Banner, "");
            AnalyticsManager.Instance.LogEvent_AdGAEvent(GAAdAction.Show, GAAdType.Banner, "");
            OnBannerLoadSuccess();
        }

        // private void OnBannerAdLoadFailedEvent(string arg1, MaxSdkBase.ErrorInfo errorInfo)
        // {
        //     Debug.Log("Banner Ad Load Failed");
        //     //OnBannerLoadFail();
        //     AnalyticsManager.Instance.LogEvent_AdGAEvent(GAAdAction.FailedShow, GAAdType.Banner, "");

        //     StartCoroutine(CheckInternetConnectionAndRetryLoadAdCall(OnBannerLoadFail));
        // }

        // private void OnBannerAdRevenuePaidEvent(string arg1, MaxSdkBase.AdInfo adInfo)
        // {
        //     if (string.IsNullOrEmpty(bannerAdIdAndroid))
        //     {
        //         Debug.Log("OnBannerAdRevenuePaidEvent adUnitId IsNullOrEmpty");
        //         return;
        //     }
        //     Debug.Log("Banner ad revenue paid " + adInfo.Revenue);

        //     AnalyticsManager.Instance.LogEvent_FirebaseAdRevenueAppLovin(adInfo.NetworkName, adInfo.AdUnitIdentifier, adInfo.AdFormat, adInfo.Revenue);
        //     AdjustManager.Instance.TrackAdRevenue(adInfo);
        //     AnalyticsManager.Instance.LogEvent_AdGAEvent(GAAdAction.RewardReceived, GAAdType.Banner, "");

        //     double ecpmRewarded = adInfo.Revenue * (1000 * 100);

        //     SendFirebaseRevenueEvent("CPM_greaterthan_1000", 100000, ecpmRewarded);
        //     SendFirebaseRevenueEvent("CPM_greaterthan_500", 50000, ecpmRewarded);
        //     SendFirebaseRevenueEvent("CPM_greaterthan_100", 10000, ecpmRewarded);
        //     SendFirebaseRevenueEvent("CPM_greaterthan_10", 1000, ecpmRewarded);
        //     // SendFirebaseRevenueEvent("CPM_greaterthan_0.001", 0.1, ecpmRewarded);
        // }

        private void SendFirebaseRevenueEvent(string key, double threshold, double ecpmValue)
        {
            AnalyticsManager.Instance.LogEvent_FirebaseAdRevanueEvent(key, threshold, ecpmValue);
        }
        #endregion

        #region COROUTINES
        IEnumerator CheckInternetConnectionAndRetryLoadAdCall(Action actionToCall)
        {
            bool isNetConnection = false;
            bool isResultAquired = false;

            while (true)
            {
                InternetManager.Instance.CheckNetConnection((state) =>
                {
                    isResultAquired = true;
                    isNetConnection = state;
                });

                while (!isResultAquired)
                {
                    yield return null;
                }

                if (isNetConnection)
                    break;
                else
                    yield return new WaitForSecondsRealtime(tryInternetCheckWait);
            }

            actionToCall?.Invoke();
        }
        #endregion

        #region UI_CALLBACKS       

        #endregion
    }

    public enum AdType
    {
        RewardedAd,
        InterstitialAd
    }

}
