using System;
using System.Collections;
using UnityEngine;
using Tag.NutSort;

namespace Tag.Ad
{
    public class BaseAd : MonoBehaviour
    {
        #region PUBLIC_VARS

        public BaseRewardedAdHandler baseRewardedAdHandler;
        public BaseInterstitialAd baseInterstitialAd;

        public const string PrefsKeyConsent = "PkConsent";

        #endregion

        #region PRIVATE_VARS

        private RewardAdShowCallType rewardAdShowCallType;
        private Action actionWatched;
        private Action actionShowed;
        private Action actionOnNoAds;

        //private Level interstitialStartFrom = new Level(1, 1, 7);
        private int interstitialStartFrom = 5;
        //private int interstitialAdAfterLevelsPlayed = 0;
        private int levelPlayedSinceLastAdShown = 0;
        private float lastTimeInterstitialShowed = 0;
        private float interstitialAdIntervalInSecond = 300f;

        public float LastTimeInterstitialShowed
        {
            get => lastTimeInterstitialShowed;
            set => lastTimeInterstitialShowed = value;
        }

        private bool isAdShownForFirstTimePref
        {
            get { return PlayerPrefs.GetInt("firstTimeAdShow", 0) == 1; }
            set { PlayerPrefs.SetInt("firstTimeAdShow", (value) ? 1 : 0); }
        }

        protected bool IsCMPDone
        {
            get { return PlayerPrefs.GetInt("isCmpDone", 0) == 1; }
            set { PlayerPrefs.SetInt("isCmpDone", (value) ? 1 : 0); }
        }

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public virtual void Init()
        {
        }

        public virtual void ShowInterstitial()
        {
            if (CanShowInterstitial() && baseInterstitialAd.IsAdLoaded())
            {
                Debug.Log("Show Interstitial");
                //SoundManager.Instance.MuteMusicAndSFX();
                levelPlayedSinceLastAdShown = 0;
                lastTimeInterstitialShowed = Time.time;
                isAdShownForFirstTimePref = true;
                //NoAdsPushView.MarkAdShownForSession();
                //DailyTaskManager.Instance.AddDailyTaskProgress(TaskType.WATCH_AD, 1);
                baseInterstitialAd.ShowAd();
            }
            else
            {
                if (CanLoadInterstitialAtShow())
                {
                    baseInterstitialAd.LoadAd();
                }
            }
        }

        public virtual void ShowInterstitialAdWithoutCondition()
        {
            if (baseInterstitialAd.IsAdLoaded())
            {
                Debug.Log("Show Interstitial");
                //SoundManager.Instance.MuteMusicAndSFX();
                levelPlayedSinceLastAdShown = 0;
                lastTimeInterstitialShowed = Time.time;
                isAdShownForFirstTimePref = true;
                //NoAdsPushView.MarkAdShownForSession();
                baseInterstitialAd.ShowAd();
            }
            else
            {
                baseInterstitialAd.LoadAd();
            }
        }

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
                lastTimeInterstitialShowed = Time.time;
                baseRewardedAdHandler.ShowAd(actionWatched, actionShowed, rewardAdShowCallType);
            }
            else
            {
                baseRewardedAdHandler.LoadAd();
                StartCoroutine(WaitAndShowRewardAdCoroutine());
            }
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

        public void AddLevelPlayedCount()
        {
            levelPlayedSinceLastAdShown++;
        }

        public bool IsAskedForConsent()
        {
            return PlayerPrefs.HasKey(PrefsKeyConsent);
        }

        #endregion

        #region PRIVATE_FUNCTIONS
        private bool CanShowInterstatialAdsAccordoingToLevel()
        {
            return PlayerPersistantData.GetMainPlayerProgressData().playerGameplayLevel >= interstitialStartFrom;
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

        private bool CanShowInterstitial()
        {
            Debug.Log("<CanShowInterstatial> 0");
            if (IsRemoveAdPurchased())
                return false;

            //Debug.Log("<CanShowInterstatial> 1");
            //if (HasPlayerLandedOnUnlockLevel())
            //{
            //    return true;
            //}

            Debug.Log("<CanShowInterstatial> 2");
            if (CanShowInterstatialAdsAccordoingToLevel())
                return false;

            Debug.LogError($"<CanShowInterstatial> 3  Last time interstitial shown : {lastTimeInterstitialShowed} Current time difference : {Time.time - lastTimeInterstitialShowed}  Required Time Difference : {interstitialAdIntervalInSecond} can Show : {!(Time.time - lastTimeInterstitialShowed < interstitialAdIntervalInSecond)}");
            if (Time.time - lastTimeInterstitialShowed < interstitialAdIntervalInSecond)
                return false;

            Debug.Log("<CanShowInterstatial> 4");
            return true;
        }

        private bool CanLoadInterstitialAtInit()
        {
            if (!InternetManager.Instance.IsReachableToNetwork())
                return false;
            Debug.LogError("Network On");

            if (IsRemoveAdPurchased())
                return false;
            Debug.LogError("RemoveAd Not Purchased");

            if (CanShowInterstatialAdsAccordoingToLevel())
                return false;
            Debug.LogError("At valid Level");

            return true;
        }

        private bool CanLoadInterstitialAtShow()
        {
            if (!InternetManager.Instance.IsReachableToNetwork())
                return false;

            Debug.LogError("Network On");

            if (IsRemoveAdPurchased())
                return false;

            Debug.LogError("RemoveAd Not Purchased");

            if (CanShowInterstatialAdsAccordoingToLevel())
                return false;

            Debug.LogError("At Valid Level");

            if (baseInterstitialAd.IsAdLoaded())
                return false;

            Debug.LogError("Ad Not Loaded Already");

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
            //CommonUIManger.Instance.adLoadingView.ShowView();
            yield return new WaitForSecondsRealtime(2f); // used Realtime because on AddMoreCustomer Ad Booster Feature Popup Make Timescale = 0 so we nee to run Coroutin
            //CommonUIManger.Instance.adLoadingView.HideView();

            if (baseRewardedAdHandler.IsAdLoaded())
            {
                //SoundManager.Instance.MuteMusicAndSFX();
                baseRewardedAdHandler.ShowAd(actionWatched, actionShowed, rewardAdShowCallType);
                //DataManager.Instance.playerData.stats.totalVideoAdWatched++;
                //DataManager.Instance.SavePlayerData();
            }
            else
            {
                //CommonUIManger.Instance.noAdAvailableView.ShowView();
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
