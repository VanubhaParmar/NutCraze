//using GameAnalyticsSDK;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tag.NutSort;

namespace Tag.Ad
{
    public class MixAdHandlerApplovinMax : BaseRewardedAdHandler
    {
        #region PUBLIC_VARS

        #endregion

        #region PRIVATE_VARS

        [SerializeField] private string interstitialIdAndroid;
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
            this.actionWatched = actionWatched;
            AdType adType = GetHighestCMPAdType();
            if (adType == AdType.RewardedAd)
            {
                ShowRewardedVideo();
                //AnalyticsManager.Instance.LogEvent_RV_Watched();
            }
            else if (adType == AdType.InterstitialAd)
            {
                ShowInterstitial();
            }
            //AnalyticsManager.Instance.AdGAEvent(GAAdAction.Show, GAAdType.RewardedVideo, adInfo);
            //DataManager.Instance.playerData.stats.totalVideoAdWatched++;
            //DataManager.Instance.SavePlayerData();
            //AnalyticsManager.Instance.LogEvent_New_RewardedAdInfo(rewardAdShowCallType.ToString());
        }

        public override bool IsAdLoaded()
        {
            return IsRewardedVideoAvailable() || IsInterstitialAvailable();
        }

        public override void LoadAd()
        {
            LoadRewardedVideo();
            LoadInterstitial();
        }

        //Logic For Simple Inter Ad.

        public void LoadSimpleInterstitialAd()
        {
            MaxSdk.LoadInterstitial(interstitialIdAndroid);
        }

        public void ShowSimpleInterstitialAd()
        {
            MaxSdk.ShowInterstitial(interstitialIdAndroid);
        }

        public bool IsSimpleInterstitialAdLoaded()
        {
            return MaxSdk.IsInterstitialReady(interstitialIdAndroid);
        }

        public void InitSimpleInterstitialAd()
        {
            LoadSimpleInterstitialAd();
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        private void InitializeRewardedAds()
        {
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdDismissedEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;
            LoadRewardedVideo();
        }

        private void InitializeinterstitialAds()
        {
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialAdLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialAdFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialAdDisplayedEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialAdClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialAdDismissedEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnInterstitialAdRevenuePaidEvent;
            LoadInterstitial();
        }

        private void LoadRewardedVideo()
        {
            StartCoroutine(LoadRewardAdCO());
        }

        private void LoadInterstitial()
        {
            StartCoroutine(LoadInterstitialCO());
        }

        private bool IsInterstitialAvailable()
        {
            string id = GetHighestCPMInterstitialId();
            if (revenuMapping.ContainsKey(id))
            {
                if (MaxSdk.IsInterstitialReady(id))
                    return true;
            }
            return false;
        }

        private bool IsRewardedVideoAvailable()
        {
            string id = GetHighestCPMRewardedId();
            if (revenuMapping.ContainsKey(id))
            {
                if (MaxSdk.IsRewardedAdReady(id))
                    return true;
            }
            return false;
        }

        private void ShowRewardedVideo()
        {
            string id = GetHighestCPMRewardedId();
            adShowTime = DateTime.Now;
            MaxSdk.ShowRewardedAd(id);
            Debug.LogError("Highest Ecpm Rewarded Ad :- " + id);
        }

        private void ShowInterstitial()
        {
            string id = GetHighestCPMInterstitialId();
            if (MaxSdk.IsInterstitialReady(id))
            {
                Debug.LogError("Highest Ecpm Interstitial Ad :- " + id);
                MaxSdk.ShowInterstitial(id);
            }
            else
            {
                LoadInterstitial();
            }
        }

        private string GetHighestCPMRewardedId()
        {
            double highestCPM = 0;
            string id = null;
            foreach (var item in revenuMapping)
            {
                if (rewardedVideoIdsAndroid.Contains(item.Key))
                {
                    if (highestCPM < item.Value)
                    {
                        highestCPM = item.Value;
                        id = item.Key;
                    }
                }
            }
            if (string.IsNullOrEmpty(id))
            {
                return rewardedVideoIdsAndroid[0];
            }
            return id;
        }

        private string GetHighestCPMInterstitialId()
        {
            double highestCPM = 0;
            string id = null;

            foreach (var item in revenuMapping)
            {
                if (rewardedInterstitialIdsAndroid.Contains(item.Key))
                {
                    if (highestCPM < item.Value)
                    {
                        highestCPM = item.Value;
                        id = item.Key;
                    }
                }
            }
            if (string.IsNullOrEmpty(id))
            {
                return rewardedInterstitialIdsAndroid[0];
            }
            return id;
        }

        private List<string> GetLoadRewardedAdId()
        {
            List<string> ids = new List<string>();
            if (revenuMapping.Count <= 0)
            {
                ids.AddRange(rewardedVideoIdsAndroid);
            }
            else
            {
                for (int i = 0; i < rewardedVideoIdsAndroid.Count; i++)
                {
                    if (!revenuMapping.ContainsKey(rewardedVideoIdsAndroid[i]))
                        ids.Add(rewardedVideoIdsAndroid[i]);
                }
            }
            if (ids.Count <= 0)
                ids.Add(rewardedVideoIdsAndroid[0]);
            return ids;
        }

        private List<string> GetLoadInterstitialAdId()
        {
            List<string> ids = new List<string>();
            if (revenuMapping.Count <= 0)
            {
                ids.AddRange(rewardedInterstitialIdsAndroid);
            }
            else
            {
                for (int i = 0; i < rewardedInterstitialIdsAndroid.Count; i++)
                {
                    if (!revenuMapping.ContainsKey(rewardedInterstitialIdsAndroid[i]))
                        ids.Add(rewardedInterstitialIdsAndroid[i]);
                }
            }
            if (ids.Count <= 0)
                ids.Add(rewardedInterstitialIdsAndroid[0]);
            return ids;
        }

        private AdType GetHighestCMPAdType()
        {
            foreach (var item in revenuMapping)
            {
                Debug.LogError("revenuMapping " + item.Key + "____" + item.Value);
            }

            string highestCPMRewardedId = GetHighestCPMRewardedId();
            string highestCPMInterstitialId = GetHighestCPMInterstitialId();
            if (revenuMapping.ContainsKey(highestCPMRewardedId) && revenuMapping.ContainsKey(highestCPMInterstitialId))
            {
                Debug.LogError("revenuMapping.ContainsKey(highestCPMRewardedId) " + highestCPMRewardedId);
                Debug.LogError("revenuMapping.ContainsKey(highestCPMInterstitialId) " + highestCPMInterstitialId);

                Debug.LogError("revenuMapping[highestCPMRewardedId] " + revenuMapping[highestCPMRewardedId]);
                Debug.LogError("revenuMapping[highestCPMInterstitialId] " + revenuMapping[highestCPMInterstitialId]);

                if (revenuMapping[highestCPMRewardedId] < revenuMapping[highestCPMInterstitialId])
                {
                    return AdType.InterstitialAd;
                }
            }
            return AdType.RewardedAd;
        }

        #endregion

        #region CO-ROUTINES

        IEnumerator LoadRewardAdCO()
        {
            List<string> ids = GetLoadRewardedAdId();
            for (int i = 0; i < ids.Count; i++)
            {
                //AnalyticsManager.Instance.LogEvent_New_RewardedAdRequested();
                //AnalyticsManager.Instance.AdGAEvent(GAAdAction.Request, GAAdType.RewardedVideo, this.adInfo);
                MaxSdk.LoadRewardedAd(ids[i]);
                yield return new WaitForSeconds(2);
            }
        }

        IEnumerator LoadInterstitialCO()
        {
            List<string> ids = GetLoadInterstitialAdId();
            for (int i = 0; i < ids.Count; i++)
            {
                MaxSdk.LoadInterstitial(ids[i]);
                yield return new WaitForSeconds(2);
            }
        }

        #endregion

        #region EVENT_HANDLERS

        private void OnRewardedVideoLoadFail()
        {
            //AnalyticsManager.Instance.LogEvent_New_RewardedAdFailed();
            LoadRewardedVideo();
        }

        private void OnInterstitialLoadFail()
        {
            LoadInterstitial();
        }

        private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (!revenuMapping.ContainsKey(adUnitId))
                revenuMapping.Add(adUnitId, adInfo.Revenue);
            else
                revenuMapping[adUnitId] = adInfo.Revenue;
            //AnalyticsManager.Instance.LogEvent_New_RewardedAdFilled();
            //AnalyticsManager.Instance.AdGAEvent(GAAdAction.Loaded, GAAdType.RewardedVideo, this.adInfo);
        }

        private void OnInterstitialAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (interstitialIdAndroid == adUnitId)
            {
                Debug.LogError("OnInterstitial AdLoadedEvent " + adUnitId);
                //AnalyticsManager.Instance.LogEvent_New_InterstitialAdFilled();
                //AnalyticsManager.Instance.AdGAEvent(GAAdAction.Loaded, GAAdType.Interstitial, "None");
                return;
            }

            //AnalyticsManager.Instance.LogEvent_New_RewardedAdFilled();
            Debug.LogError("OnInterstitial-Rewarded AdLoadedEvent " + adUnitId);
            if (!revenuMapping.ContainsKey(adUnitId))
                revenuMapping.Add(adUnitId, adInfo.Revenue);
            else
                revenuMapping[adUnitId] = adInfo.Revenue;
        }

        private void OnRewardedAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            if (revenuMapping.ContainsKey(adUnitId))
            {
                revenuMapping.Remove(adUnitId);
            }
            OnRewardedVideoLoadFail();
            //AnalyticsManager.Instance.AdGAEvent(GAAdAction.FailedShow, GAAdType.RewardedVideo, adInfo);
            this.adInfo = "None";

        }

        private void OnInterstitialAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            if (interstitialIdAndroid == adUnitId)
            {
                Debug.LogError("OnInterstitial AdFailedEvent " + adUnitId);
                //AnalyticsManager.Instance.LogEvent_New_InterstitialAdFailed();
                //AnalyticsManager.Instance.AdGAEvent(GAAdAction.FailedShow, GAAdType.Interstitial, "None");
                LoadSimpleInterstitialAd();
                return;
            }

            Debug.LogError("OnInterstitial-Rewarded AdFailedEvent " + adUnitId);
            if (revenuMapping.ContainsKey(adUnitId))
            {
                revenuMapping.Remove(adUnitId);
            }
            OnInterstitialLoadFail();
        }

        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            if (revenuMapping.ContainsKey(adUnitId))
            {
                revenuMapping.Remove(adUnitId);
            }
            OnRewardedVideoLoadFail();
            //AnalyticsManager.Instance.AdGAEvent(GAAdAction.FailedShow, GAAdType.RewardedVideo, this.adInfo);
            this.adInfo = "None";


        }

        private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            if (interstitialIdAndroid == adUnitId)
            {
                Debug.LogError("OnInterstitial AdFailedToDisplayEvent " + adUnitId);
                LoadSimpleInterstitialAd();
                return;
            }

            Debug.LogError("OnInterstitial-Rewarded AdFailedToDisplayEvent " + adUnitId);
            if (revenuMapping.ContainsKey(adUnitId))
            {
                revenuMapping.Remove(adUnitId);
            }
            OnInterstitialLoadFail();
        }

        private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("Rewarded ad displayed");

        }

        private void OnInterstitialAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (interstitialIdAndroid == adUnitId)
            {
                Debug.LogError("OnInterstitial AdDisplayedEvent " + adUnitId);
                //DataManager.Instance.playerData.stats.totalInterstiAdWatched++;
                //DataManager.Instance.SavePlayerData();
                //AnalyticsManager.Instance.LogEvent_New_InterstitialAdShowed();
                //AnalyticsManager.Instance.LogEvent_INT_Watched();
                //AnalyticsManager.Instance.AdGAEvent(GAAdAction.Show, GAAdType.Interstitial, "None");
                return;
            }

            Debug.LogError("OnInterstitial-Rewarded AdDisplayedEvent " + adUnitId);
        }

        private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("Rewarded ad clicked");
            //AnalyticsManager.Instance.AdGAEvent(GAAdAction.Clicked, GAAdType.RewardedVideo, this.adInfo);

        }

        private void OnInterstitialAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (interstitialIdAndroid == adUnitId)
            {
                Debug.LogError("OnInterstitial AdClickedEvent " + adUnitId);
                return;
            }

            Debug.LogError("OnInterstitial-Rewarded AdClickedEvent " + adUnitId);
        }

        private void OnRewardedAdDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            RemoveRewardedAdIdsFromMapping(adUnitId);
            double seconds = 100;
            if (adShowTime != DateTime.MinValue)
                seconds = DateTime.Now.Subtract(adShowTime).TotalSeconds;
            if (actionWatched != null && (isRewardAdWatched || seconds > 15))
            {
                actionWatched();
            }
            else if (actionShowed != null)
            {
                actionShowed();
            }
            LoadRewardedVideo();
            isRewardAdWatched = false;
        }

        private void RemoveRewardedAdIdsFromMapping(string adUnitId)
        {
            if (!destoryRewardedVideoIdsAndroid.Contains(adUnitId))
            {
                for (int i = 0; i < destoryRewardedVideoIdsAndroid.Count; i++)
                {
                    if (revenuMapping.ContainsKey(destoryRewardedVideoIdsAndroid[i]) && MaxSdk.IsRewardedAdReady(destoryRewardedVideoIdsAndroid[i]))
                    {
                        revenuMapping.Remove(destoryRewardedVideoIdsAndroid[i]);
                        Debug.LogError("RemoveRewardedAdIdsFromMapping  " + destoryRewardedVideoIdsAndroid[i]);
                    }
                }
            }
            if (revenuMapping.ContainsKey(adUnitId))
            {
                revenuMapping.Remove(adUnitId);
            }
        }

        private void OnInterstitialAdDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (interstitialIdAndroid == adUnitId)
            {
                Debug.LogError("OnInterstitial AdDismissedEvent " + adUnitId);
                LoadSimpleInterstitialAd();
                return;
            }

            Debug.LogError("OnInterstitial-Rewarded AdDismissedEvent " + adUnitId);
            RemoveInterstitialAdIdsFromMapping(adUnitId);
            LoadInterstitial();
            if (actionWatched != null && (isRewardAdWatched))
            {
                actionWatched();
            }
            else if (actionShowed != null)
            {
                actionShowed();
            }
            isRewardAdWatched = false;
        }

        private void RemoveInterstitialAdIdsFromMapping(string adUnitId)
        {
            if (!destoryInterstitialIdsAndroid.Contains(adUnitId))
            {
                for (int i = 0; i < destoryInterstitialIdsAndroid.Count; i++)
                {
                    if (revenuMapping.ContainsKey(destoryInterstitialIdsAndroid[i]) && MaxSdk.IsRewardedAdReady(destoryInterstitialIdsAndroid[i]))
                    {
                        revenuMapping.Remove(destoryInterstitialIdsAndroid[i]);
                        Debug.LogError("RemoveInterstitialAdIdsFromMapping  " + destoryInterstitialIdsAndroid[i]);
                    }
                }
            }
            if (revenuMapping.ContainsKey(adUnitId))
            {
                revenuMapping.Remove(adUnitId);
            }
        }

        private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("Rewarded ad received reward");
            isRewardAdWatched = true;
            //AnalyticsManager.Instance.AdGAEvent(GAAdAction.RewardReceived, GAAdType.RewardedVideo, this.adInfo);
            this.adInfo = "None";
        }

        private void OnInterstitialAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            if (interstitialIdAndroid == adUnitId)
            {
                Debug.LogError("OnInterstitial AdRevenuePaidEvent " + adUnitId);
                //AdjustManager.Instance.TrackAdRevenue(adInfo);
                return;
            }

            Debug.LogError("OnInterstitial-Rewarded AdRevenuePaidEvent " + adUnitId);
            //AdjustManager.Instance.TrackAdRevenue(adInfo);
            isRewardAdWatched = true;
        }

        private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            //AdjustManager.Instance.TrackAdRevenue(adInfo);
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
