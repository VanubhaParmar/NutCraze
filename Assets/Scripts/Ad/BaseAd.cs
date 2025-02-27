using System;
using System.Collections;
using System.Collections.Generic;
using Tag.NutSort;
using UnityEngine;

namespace Tag.Ad
{
    public class BaseAd : MonoBehaviour
    {
        #region PUBLIC_VARS

        public BaseRewardedAdHandler baseRewardedAdHandler;
        public BaseInterstitialAd baseInterstitialAd;
        public BaseBannerAd baseBannerAd;

        public const string PrefsKeyConsent = "PkConsent";

        #endregion

        #region PRIVATE_VARS

        private RewardAdShowCallType rewardAdShowCallType;
        private Action actionWatched;
        private Action actionShowed;
        private Action actionOnNoAds;

        private Action actionToCallOnInitSuccess;

        //private Level interstitialStartFrom = new Level(1, 1, 7);
        //private int interstitialStartFrom = 5;
        //private int interstitialAdAfterLevelsPlayed = 0;
        //private int levelPlayedSinceLastAdShown = 0;
        private List<float> lastTimeInterstitialShowed = new List<float>();
        //private float interstitialAdIntervalInSecond = 300f;

        //public float LastTimeInterstitialShowed
        //{
        //    get => lastTimeInterstitialShowed;
        //    set => lastTimeInterstitialShowed = value;
        //}

        private bool isAdShownForFirstTimePref
        {
            get { return PlayerPrefbsHelper.GetInt("firstTimeAdShow", 0) == 1; }
            set { PlayerPrefbsHelper.SetInt("firstTimeAdShow", (value) ? 1 : 0); }
        }

        protected bool IsCMPDone
        {
            get { return PlayerPrefbsHelper.GetInt("isCmpDone", 0) == 1; }
            set { PlayerPrefbsHelper.SetInt("isCmpDone", (value) ? 1 : 0); }
        }

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public virtual void Init(Action actionToCallOnInitSuccess = null)
        {
            this.actionToCallOnInitSuccess = actionToCallOnInitSuccess;

            lastTimeInterstitialShowed = new List<float>();
            for (int i = 0; i < AdManager.Instance.AdConfigData.interstitialAdConfigDatas.Count; i++)
            {
                int startLevel = AdManager.Instance.AdConfigData.interstitialAdConfigDatas[i].startLevel;
                int currentLevel = PlayerPersistantData.GetMainPlayerProgressData().playerGameplayLevel;

                lastTimeInterstitialShowed.Add(Mathf.Max(startLevel, currentLevel));
            }
        }

        public virtual void ShowInterstitial(InterstatialAdPlaceType interstatialAdPlaceType)
        {
            Debug.Log("Try Show Interstitial " + interstatialAdPlaceType.ToString());
            if (CanShowInterstitial(interstatialAdPlaceType) && baseInterstitialAd.IsAdLoaded())
            {
                Debug.Log("Show Interstitial " + interstatialAdPlaceType.ToString());
                //SoundManager.Instance.MuteMusicAndSFX();
                //levelPlayedSinceLastAdShown = 0;

                //lastTimeInterstitialShowed[(int)interstatialAdPlaceType] = Time.time; // >>>>>>>>> Time Logic
                lastTimeInterstitialShowed[(int)interstatialAdPlaceType] = PlayerPersistantData.GetMainPlayerProgressData().playerGameplayLevel; // >>>>>>>>> Level Logic

                isAdShownForFirstTimePref = true;
                //NoAdsPushView.MarkAdShownForSession();
                //DailyTaskManager.Instance.AddDailyTaskProgress(TaskType.WATCH_AD, 1);
                baseInterstitialAd.ShowAd();
            }
            else
            {
                if (CanLoadInterstitial(interstatialAdPlaceType))
                {
                    Debug.Log("Load Interstitial " + interstatialAdPlaceType.ToString());
                    baseInterstitialAd.LoadAd();
                }
            }
        }

        //public virtual void ShowInterstitialAdWithoutCondition()
        //{
        //    if (baseInterstitialAd.IsAdLoaded())
        //    {
        //        Debug.Log("Show Interstitial");
        //        //SoundManager.Instance.MuteMusicAndSFX();
        //        levelPlayedSinceLastAdShown = 0;
        //        lastTimeInterstitialShowed = Time.time;
        //        isAdShownForFirstTimePref = true;
        //        //NoAdsPushView.MarkAdShownForSession();
        //        baseInterstitialAd.ShowAd();
        //    }
        //    else
        //    {
        //        baseInterstitialAd.LoadAd();
        //    }
        //}

        public virtual void ShowRewardedVideo(Action actionWatched, Action actionShowed = null, Action actionOnNoAds = null, RewardAdShowCallType rewardAdShowCallType = RewardAdShowCallType.None)
        {
            this.actionWatched = actionWatched;
            this.actionShowed = actionShowed;
            this.actionOnNoAds = actionOnNoAds;
            this.rewardAdShowCallType = rewardAdShowCallType;

            if (baseRewardedAdHandler.IsAdLoaded())
            {
                Debug.Log("<AMRSDK> Show RewardedVideo");
                //SoundManager.Instance.MuteMusicAndSFX();
                //lastTimeInterstitialShowed = Time.time;
                baseRewardedAdHandler.ShowAd(actionWatched, actionShowed, rewardAdShowCallType);
            }
            else
            {
                baseRewardedAdHandler.LoadAd();
                StartCoroutine(WaitAndShowRewardAdCoroutine());
            }
        }
        public virtual void StartBannerAdAutoRefresh()
        {

        }

        public virtual void ShowBannerAd()
        {
            baseBannerAd.ShowBanner();
        }
        public virtual void HideBannerAd()
        {
            baseBannerAd.HideBanner();
        }
        public virtual bool IsBannerAdLoaded()
        {
            return baseBannerAd.IsBannerAdLoaded();
        }

        public virtual Rect GetBannerRect()
        {
            return baseBannerAd.GetBannerRect();
        }

        //public void SetInterstitialAdData(int interstitialAdLevelCountGap, int unlockLevel)
        //{
        //    interstitialAdAfterLevelsPlayed = interstitialAdLevelCountGap;
        //    this.interstitialStartFrom = unlockLevel;
        //    //Debug.Log(interstitialAdLevelCountGap + " : w " + this.interstitialStartFrom.world + " R " + this.interstitialStartFrom.restaurant + " L " + this.interstitialStartFrom.level);
        //}

        //public void SetInterstitialAdData(float interstitialAdIntervalInSeconds, int interstitialStartFrom)
        //{
        //    interstitialAdIntervalInSecond = interstitialAdIntervalInSeconds;
        //    this.interstitialStartFrom = interstitialStartFrom;
        //    //Debug.Log(interstitialAdIntervalInSeconds + " : w " + this.interstitialStartFrom.world + " R " + this.interstitialStartFrom.restaurant + " L " + this.interstitialStartFrom.level);
        //}

        public virtual void OnInitSuccess()
        {
            if (actionToCallOnInitSuccess != null)
                actionToCallOnInitSuccess();
        }

        //public void AddLevelPlayedCount()
        //{
        //    levelPlayedSinceLastAdShown++;
        //}

        public bool IsAskedForConsent()
        {
            return PlayerPrefbsHelper.HasKey(PrefsKeyConsent);
        }

        #endregion

        #region PRIVATE_FUNCTIONS
        private bool CanShowInterstatialAdsAccordoingToLevel(InterstatialAdPlaceType interstatialAdPlaceType)
        {
            return AdManager.Instance.AdConfigData.CanShowInterstitialAd(interstatialAdPlaceType);
        }

        private bool IsRemoveAdPurchased()
        {
            return false;
        }

        //private bool HasPlayerLandedOnUnlockLevel()
        //{
        //    CompareLevelResult unlockCompareLevelResult = Utility.CompareLevel(DataManager.Instance.playerData.level, interstitialStartFrom);
        //    return unlockCompareLevelResult == CompareLevelResult.Equal && !isAdShownForFirstTimePref;
        //}

        private bool CanShowInterstitial(InterstatialAdPlaceType interstatialAdPlaceType)
        {
            //Debug.Log("<CanShowInterstatial> 0");
            if (IsRemoveAdPurchased())
                return false;

            //Debug.Log("<CanShowInterstatial> 1");
            //if (HasPlayerLandedOnUnlockLevel())
            //{
            //    return true;
            //}

            //Debug.Log("<CanShowInterstatial> 2");
            if (!CanShowInterstatialAdsAccordoingToLevel(interstatialAdPlaceType))
                return false;

            // >>>>>>>>>>> Time Logic
            //float lastTimeShowed = lastTimeInterstitialShowed[(int)interstatialAdPlaceType];
            //float interstitialAdIntervalInSecond = GetInterstitialAdIntervalInSecond(interstatialAdPlaceType);

            ////Debug.LogError($"<CanShowInterstatial> 3  Last time interstitial shown : {lastTimeInterstitialShowed} Current time difference : {Time.time - lastTimeShowed}  " + $"Required Time Difference : {interstitialAdIntervalInSecond} can Show : {!(Time.time - lastTimeShowed < interstitialAdIntervalInSecond)}");
            //if (Time.time - lastTimeShowed < interstitialAdIntervalInSecond)
            //    return false;
            // >>>>>>>>>>> Time Logic

            // >>>>>>>>>>> Level Logic
            float lastTimeShowed = lastTimeInterstitialShowed[(int)interstatialAdPlaceType];
            float currentValue = PlayerPersistantData.GetMainPlayerProgressData().playerGameplayLevel;

            int levelDifference = GetInterstitialAdIntervalInLevels(interstatialAdPlaceType);
            if (currentValue - lastTimeShowed < levelDifference)
                return false;
            // >>>>>>>>>>> Level Logic

            //Debug.Log("<CanShowInterstatial> 4");
            return true;
        }

        //private float GetInterstitialAdIntervalInSecond(InterstatialAdPlaceType interstatialAdPlaceType)
        //{
        //    return AdManager.Instance.AdConfigData.GetShowInterstitialAdIntervalTime(interstatialAdPlaceType);
        //}

        private int GetInterstitialAdIntervalInLevels(InterstatialAdPlaceType interstatialAdPlaceType)
        {
            return AdManager.Instance.AdConfigData.GetShowInterstitialAdIntervalLevel(interstatialAdPlaceType);
        }

        //private bool CanLoadInterstitialAtInit()
        //{
        //    if (!InternetManager.Instance.IsReachableToNetwork())
        //        return false;
        //    Debug.LogError("Network On");

        //    if (IsRemoveAdPurchased())
        //        return false;
        //    Debug.LogError("RemoveAd Not Purchased");

        //    if (CanShowInterstatialAdsAccordoingToLevel())
        //        return false;
        //    Debug.LogError("At valid Level");

        //    return true;
        //}

        private bool CanLoadInterstitial(InterstatialAdPlaceType interstatialAdPlaceType)
        {
            if (!InternetManager.IsReachableToNetwork())
                return false;

            //Debug.LogError("Network On");

            if (IsRemoveAdPurchased())
                return false;

            //Debug.LogError("RemoveAd Not Purchased");

            if (!CanShowInterstatialAdsAccordoingToLevel(interstatialAdPlaceType))
                return false;

            //Debug.LogError("At Valid Level");

            if (baseInterstitialAd.IsAdLoaded())
                return false;

            //Debug.LogError("Ad Not Loaded Already");

            return true;
        }

        public void ResetActions()
        {
            actionShowed = actionWatched = actionOnNoAds = null;
        }

        #endregion

        #region CO-ROUTINES

        private IEnumerator WaitAndShowRewardAdCoroutine()
        {
            GlobalUIManager.Instance.GetView<InGameLoadingView>().Show(extraMessage: UserPromptMessageConstants.RewardedAdLoadingMessage);
            yield return new WaitForSecondsRealtime(2f); // used Realtime because on AddMoreCustomer Ad Booster Feature Popup Make Timescale = 0 so we nee to run Coroutin
            GlobalUIManager.Instance.GetView<InGameLoadingView>().Hide();

            if (baseRewardedAdHandler.IsAdLoaded())
            {
                //SoundManager.Instance.MuteMusicAndSFX();
                baseRewardedAdHandler.ShowAd(actionWatched, actionShowed, rewardAdShowCallType);
                //DataManager.Instance.playerData.stats.totalVideoAdWatched++;
                //DataManager.Instance.SavePlayerData();
            }
            else
            {
                GlobalUIManager.Instance.GetView<UserPromptView>().Show(UserPromptMessageConstants.RewardedNotAvailableMessage);
                Debug.Log("No Ad Available at this time");
                if (actionOnNoAds != null)
                    actionOnNoAds();
                ResetActions();
            }
        }

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       

        #endregion
    }
}
