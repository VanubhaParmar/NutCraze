using UnityEngine;
using System.Collections.Generic;
using System;
using Tag.Ad;
using System.Linq;
using Sirenix.OdinInspector;

namespace Tag.NutSort
{
    public class AdManager : SerializedManager<AdManager>
    {
        #region PUBLIC_VARS

        public AdManagerDataSO adManagerDataSO;
        public BaseAd baseAd;
        public RewardAdShowCallType rewardAdShowCallType;
        public static bool isNoAdLoadedIconDeactiveEnable = true;
        public bool isCMPOn = false;

        public AdConfigData AdConfigData => myAdConfigData;
        #endregion

        #region PRIVATE_VARS

        //private Level unlockLevel = new Level(1, 2, 1);
        //private int unlockLevel = 3;

        [ShowInInspector, ReadOnly] private AdConfigData myAdConfigData;
        [SerializeField] private AdsDataRemoteConfig AdsDataRemoteConfig;
        private List<Action> onAdLoad = new List<Action>();
        private const string PrefsKeyConsent = "PkConsent";

        #endregion

        #region UNITY_CALLBACKS

        private void OnEnable()
        {
            FirebaseRemoteConfigManager.onRCValuesFetched += FirebaseRemoteConfigManager_onRCValuesFetched;
        }

        private void OnDisable()
        {
            FirebaseRemoteConfigManager.onRCValuesFetched -= FirebaseRemoteConfigManager_onRCValuesFetched;
        }

        public override void Awake()
        {
            base.Awake();
            OnInitializeAdManager();
        }

        #endregion

        #region PUBLIC_FUNCTIONS

        public void OnInitializeAdManager()
        {
            //Application.targetFrameRate = 60;
            //Init();

            myAdConfigData = adManagerDataSO.GetDefaultAdConfigData();
            if (FirebaseManager.Instance.FirebaseRC.remoteConfigFetchTaskStatus == FirebaseRemoteConfigManager.RemoteConfigFetchTaskStatus.COMPLETED)
                SetInterstitialAdData(AdsDataRemoteConfig.GetValue<AdConfigData>());

            baseAd.gameObject.SetActive(true);
            baseAd.Init(OnLoadingDone);
        }

        public void ShowInterstitial(InterstatialAdPlaceType interstatialAdPlaceType)
        {
            if (!Constant.IsAdOn || IsNoAdsPurchased())
                return;

            baseAd.ShowInterstitial(interstatialAdPlaceType);
        }

        public void ShowBannerAd()
        {
            if (!Constant.IsAdOn || !CanShowBannerAd() || IsNoAdsPurchased())
                return;

            baseAd.ShowBannerAd();
        }

        public void HideBannerAd()
        {
            baseAd.HideBannerAd();
        }

        public Rect GetBannerRect()
        {
            return baseAd.GetBannerRect();
        }

        public bool CanShowBannerAd()
        {
            return MainSceneUIManager.Instance != null && MainSceneUIManager.Instance.GetView<BannerAdsView>().gameObject.activeInHierarchy &&
                PlayerPersistantData.GetMainPlayerProgressData().playerGameplayLevel >= AdConfigData.showBannerAdsAfterLevel;
        }

        public bool IsNoAdsPurchased()
        {
            return DataManager.Instance.IsNoAdsPackPurchased();
        }

        public void ShowRewardedAd(Action actionWatched, RewardAdShowCallType rewardAdShowCallType, Action actionShowed = null, Action actionOnNoAds = null)
        {
            if (!Constant.IsAdOn)
            {
                actionWatched.Invoke();
                return;
            }

            if (!IsInternetAvailable())
            {
                //CommonUIManger.Instance.noInterNetAvailable.ShowView(AnalyticsConstants.Parameters.RewardedAd);
                actionOnNoAds?.Invoke();
                return;
            }
            this.rewardAdShowCallType = rewardAdShowCallType;
            baseAd.ShowRewardedVideo(actionWatched, actionShowed, actionOnNoAds, rewardAdShowCallType);
        }

        public bool IsRewardedAdLoad()
        {
            return baseAd.baseRewardedAdHandler.IsAdLoaded();
        }

        public bool IsInternetAvailable()
        {
            return InternetManager.IsReachableToNetwork();
        }

        public void SetInterstitialAdData(AdConfigData adConfigData)
        {
            myAdConfigData = adConfigData;
            isCMPOn = adConfigData.isCMPOn;
            //int interstitialAdLevelCountGap;
            //float interstitialAdTimeToShow;

            //if (PlayerPersistantData.GetMainPlayerProgressData().playerGameplayLevel < adConfigData.unlockLevelAfterNumOfLevels)
            //{
            //    interstitialAdLevelCountGap = adConfigData.interstitialAdsIntervalInLevelGap;
            //    unlockLevel = adConfigData.unlockLevel;
            //    interstitialAdTimeToShow = adConfigData.interstitialAdsIntervalInSeconds;
            //}
            //else
            //{
            //    interstitialAdLevelCountGap = adConfigData.interstitialAdsIntervalAfterSomeNumOfLevelsInLevelGap;
            //    unlockLevel = adConfigData.unlockLevelAfterNumOfLevels;
            //    interstitialAdTimeToShow = adConfigData.interstitialAdsIntervalAfterSomeNumOfLevelsInSeconds;
            //}

            //baseAd.SetInterstitialAdData(interstitialAdTimeToShow, unlockLevel);
        }

        public void AddListenerMouseButtonDown(Action action)
        {
            if (!onAdLoad.Contains(action))
                onAdLoad.Add(action);
        }

        public void RemoveListenerMouseButtonDown(Action action)
        {
            if (onAdLoad.Contains(action))
                onAdLoad.Remove(action);
        }

        public void InvakeOnAdLoad()
        {
            foreach (var ev in onAdLoad)
            {
                ev?.Invoke();
            }
        }

        //public bool IsRequiedLevelForInerAd(InterstatialAdPlaceType interstatialAdPlaceType)
        //{
        //    return PlayerPersistantData.GetMainPlayerProgressData().playerGameplayLevel >= AdConfigData.interstitialAdConfigDatas.Find(x => x.interstatialAdPlaceType == interstatialAdPlaceType).startLevel;
        //}

        //public void AddLevelPlayedCount()
        //{
        //    baseAd.AddLevelPlayedCount();
        //}

        #endregion

        #region PRIVATE_FUNCTIONS

        private void Init()
        {
            if (!IsAskedForConsent())
            {
                //GlobalUIManager.Instance.GetView<AdConsentView>().Show(SetConsent);
            }
        }

        private void SetConsent()
        {
            PlayerPrefs.SetInt(PrefsKeyConsent, 1);
        }

        private bool IsAskedForConsent()
        {
            return PlayerPrefs.HasKey(PrefsKeyConsent);
        }

        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        private void FirebaseRemoteConfigManager_onRCValuesFetched()
        {
            SetInterstitialAdData(AdsDataRemoteConfig.GetValue<AdConfigData>());
        }

        #endregion

        #region UI_CALLBACKS

        #endregion
    }

    public enum RewardAdShowCallType
    {
        None = 0,
        Undo_Booster_Ad = 1,
        Extra_Booster_Ad = 2,
    }

    [System.Serializable]
    public class AdConfigData
    {
        public AdConfigData()
        {
            isCMPOn = false;
            interstitialAdConfigDatas = new List<InterstitialAdConfigData>();
            showBannerAdsAfterLevel = 0;
        }

        public List<InterstitialAdConfigData> interstitialAdConfigDatas = new List<InterstitialAdConfigData>();
        public int showBannerAdsAfterLevel = 0;
        public bool isCMPOn;

        public bool CanShowBannerAd()
        {
            int currentPlayerLevel = PlayerPersistantData.GetMainPlayerProgressData().playerGameplayLevel;
            return currentPlayerLevel >= showBannerAdsAfterLevel;
        }

        public bool CanShowInterstitialAd(InterstatialAdPlaceType placeType)
        {
            int currentPlayerLevel = PlayerPersistantData.GetMainPlayerProgressData().playerGameplayLevel;
            InterstitialAdConfigData interstitialAdConfigData = interstitialAdConfigDatas.Find(x => x.interstatialAdPlaceType == placeType);
            if (interstitialAdConfigData != null)
                return interstitialAdConfigData.startLevel <= currentPlayerLevel;

            return true;
        }

        public float GetShowInterstitialAdIntervalTime(InterstatialAdPlaceType placeType)
        {
            int currentPlayerLevel = PlayerPersistantData.GetMainPlayerProgressData().playerGameplayLevel;
            InterstitialAdConfigData interstitialAdConfigData = interstitialAdConfigDatas.Find(x => x.interstatialAdPlaceType == placeType);
            if (interstitialAdConfigData != null)
                return interstitialAdConfigData.GetTimeInterval(currentPlayerLevel);

            return 0f;
        }
    }

    public class InterstitialAdConfigData
    {
        public InterstatialAdPlaceType interstatialAdPlaceType;
        public int startLevel;
        public List<AdTimeIntervalLevelConfigData> adTimeIntervalLevelConfigDatas;

        public float GetTimeInterval(int level)
        {
            for (int j = adTimeIntervalLevelConfigDatas.Count - 1; j >= 0; j--)
            {
                if (level >= adTimeIntervalLevelConfigDatas[j].fromLevel)
                {
                    return adTimeIntervalLevelConfigDatas[j].timeInterval;
                }
            }

            return adTimeIntervalLevelConfigDatas.First().timeInterval;
        }
    }

    public class AdTimeIntervalLevelConfigData
    {
        public int fromLevel;
        public float timeInterval;
    }

    public enum InterstatialAdPlaceType
    {
        Game_Win_Screen = 0,
        Reload_Level = 1,
    }
}