using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.tag.nut_sort {
    public class AdManager : SerializedManager<AdManager>
    {
        #region PUBLIC_VARS

        public AdManagerDataSO adManagerDataSO;
        public BaseAd baseAd;
        public RewardAdShowCallType rewardAdShowCallType;
        public static bool isNoAdLoadedIconDeactiveEnable = true;
        public bool isCMPOn = false;

        public AdConfigData AdConfigData => myAdConfigData;
        public string AdNameType => adNameType;
        #endregion

        #region PRIVATE_VARS
        [ShowInInspector, ReadOnly] private AdConfigData myAdConfigData;
        [SerializeField] private AdsDataRemoteConfig AdsDataRemoteConfig;
        private List<Action> onAdLoad = new List<Action>();
        private const string PrefsKeyConsent = "PkConsent";
        private string adNameType = "Init";

        private AdManagerPlayerData _adManagerPlayerData
        {
            get { return SerializeUtility.DeserializeObject<AdManagerPlayerData>(AdManagerDataString); }
            set { AdManagerDataString = SerializeUtility.SerializeObject(value); }
        }
        private string AdManagerDataString
        {
            get { return PlayerPrefbsHelper.GetString(AdManagerDataPrefsKey, SerializeUtility.SerializeObject(GetDefaultAdManagerPlayerData())); }
            set { PlayerPrefbsHelper.SetString(AdManagerDataPrefsKey, value); }
        }
        private const string AdManagerDataPrefsKey = "AdManagerPlayerData";
        #endregion

        #region UNITY_CALLBACKS

        private void OnEnable()
        {
            GameAnalyticsManager.onRCValuesFetched += GameAnalyticsManager_onRCValuesFetched;
        }

        private void OnDisable()
        {
            GameAnalyticsManager.onRCValuesFetched -= GameAnalyticsManager_onRCValuesFetched;
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

            SetAdRCData(AdsDataRemoteConfig.GetValue<AdConfigData>());

            baseAd.gameObject.SetActive(true);
            baseAd.Init(OnLoadingDone);
        }

        public void ShowInterstitial(InterstatialAdPlaceType interstatialAdPlaceType, string adSourceName)
        {
            if (!Constant.IsAdOn || IsNoAdsPurchased() || !DevProfileHandler.Instance.CurrentDevelopmentProfile.canShowInterstitialAds)
                return;

            this.adNameType = adSourceName;
            baseAd.ShowInterstitial(interstatialAdPlaceType);
        }

        public void StartBannerAdAutoRefresh()
        {
            if (!Constant.IsAdOn || !CanShowBannerAd() || IsNoAdsPurchased())
                return;

            baseAd.ShowBannerAd();
        }

        public void ShowBannerAd(out bool isShowCallSuccess)
        {
            if (!Constant.IsAdOn || !CanShowBannerAd() || IsNoAdsPurchased())
            {
                isShowCallSuccess = false;
                return;
            }

            baseAd.ShowBannerAd();
            isShowCallSuccess = true;
        }

        public bool IsBannerAdLoaded()
        {
            return baseAd.IsBannerAdLoaded();
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

        public bool CanShowRewardedAd()
        {
            CheckForResetRewardsAdCount();
            return _adManagerPlayerData.currentShowedRewardedAdsCount < AdConfigData.rewardsAdsCount;
        }

        public void ShowRewardedAd(Action actionWatched, RewardAdShowCallType rewardAdShowCallType, string adSourceName, Action actionShowed = null, Action actionOnNoAds = null)
        {
            if (!Constant.IsAdOn)
            {
                actionWatched.Invoke();
                return;
            }

            if (!IsInternetAvailable())
            {
                GlobalUIManager.Instance.GetView<UserPromptView>().Show(UserPromptMessageConstants.NoInternetConnection);
                //CommonUIManger.Instance.noInterNetAvailable.ShowView(AnalyticsConstants.Parameters.RewardedAd);
                actionOnNoAds?.Invoke();
                return;
            }

#if UNITY_EDITOR
            actionWatched.Invoke();
            return;
#elif UNITY_ANDROID && !UNITY_EDITOR
            this.adNameType = adSourceName;
            this.rewardAdShowCallType = rewardAdShowCallType;
            baseAd.ShowRewardedVideo(actionWatched, actionShowed, actionOnNoAds, rewardAdShowCallType);
#endif
        }

        public void OnRewardedAdShowed()
        {
            var adManagerData = _adManagerPlayerData;
            adManagerData.currentShowedRewardedAdsCount++;
            _adManagerPlayerData = adManagerData;
        }

        public bool IsRewardedAdLoad()
        {
            return baseAd.baseRewardedAdHandler.IsAdLoaded();
        }

        public bool IsInternetAvailable()
        {
            return InternetManager.IsReachableToNetwork();
        }

        public void SetAdRCData(AdConfigData adConfigData)
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

        private AdManagerPlayerData GetDefaultAdManagerPlayerData()
        {
            AdManagerPlayerData adManagerPlayerData = new AdManagerPlayerData();
            adManagerPlayerData.currentShowedRewardedAdsCount = 0;
            adManagerPlayerData.lastRewardsAdsCountResetTime = GetCurrentReferenceRewardsAdsCountResetTime(TimeManager.Now).GetPlayerPrefsSaveString();

            return adManagerPlayerData;
        }

        public void CheckForResetRewardsAdCount()
        {
            var adManagerData = _adManagerPlayerData;

            bool parse = adManagerData.lastRewardsAdsCountResetTime.TryParseDateTime(out DateTime lastResetTime);
            bool canReset = !parse;

            if (parse)
                canReset = Math.Abs((GetCurrentReferenceRewardsAdsCountResetTime(TimeManager.Now) - lastResetTime).TotalSeconds) > 0.1f;

            if (canReset)
            {
                adManagerData.currentShowedRewardedAdsCount = 0;
                adManagerData.lastRewardsAdsCountResetTime = GetCurrentReferenceRewardsAdsCountResetTime(TimeManager.Now).GetPlayerPrefsSaveString();

                _adManagerPlayerData = adManagerData;
            }
        }

        private DateTime GetCurrentReferenceRewardsAdsCountResetTime(DateTime currentTime)
        {
            DateTime startRefTime = currentTime.Date.AddTimeDuration(adManagerDataSO.refreshRewardAdsCapLocalTime);
            if (currentTime < startRefTime)
                startRefTime = startRefTime.AddDays(-1);

            TimeSpan totalDifference = currentTime - startRefTime;
            int totalHourMultiplier = totalDifference.Hours / AdConfigData.rewardsAdHoursInterval;

            return startRefTime.AddHours(totalHourMultiplier * AdConfigData.rewardsAdHoursInterval);
        }

        private void Init()
        {
            if (!IsAskedForConsent())
            {
                //GlobalUIManager.Instance.GetView<AdConsentView>().Show(SetConsent);
            }
        }

        private void SetConsent()
        {
            PlayerPrefbsHelper.SetInt(PrefsKeyConsent, 1);
        }

        private bool IsAskedForConsent()
        {
            return PlayerPrefbsHelper.HasKey(PrefsKeyConsent);
        }

        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        private void GameAnalyticsManager_onRCValuesFetched()
        {
            SetAdRCData(AdsDataRemoteConfig.GetValue<AdConfigData>());
        }

        #endregion

        #region UI_CALLBACKS

        #endregion

        #region EDITOR_FUNCTIONS
        [Button]
        public void Editor_DebugAdManagerPlayerData()
        {
            Debug.Log(SerializeUtility.SerializeObject(_adManagerPlayerData));
        }

        [Button]
        public void Editor_OnRewardAdShowed()
        {
            OnRewardedAdShowed();
        }
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
        }

        public List<InterstitialAdConfigData> interstitialAdConfigDatas = new List<InterstitialAdConfigData>();
        public int showBannerAdsAfterLevel = 0;
        public int rewardsAdHoursInterval = 4;
        public int rewardsAdsCount = 20;
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

        public int GetInterstitialAdStartLevel(InterstatialAdPlaceType placeType)
        {
            InterstitialAdConfigData interstitialAdConfigData = interstitialAdConfigDatas.Find(x => x.interstatialAdPlaceType == placeType);
            if (interstitialAdConfigData != null)
                return interstitialAdConfigData.startLevel;

            return 1;
        }

        //public float GetShowInterstitialAdIntervalTime(InterstatialAdPlaceType placeType)
        //{
        //    int currentPlayerLevel = PlayerPersistantData.GetMainPlayerProgressData().playerGameplayLevel;
        //    InterstitialAdConfigData interstitialAdConfigData = interstitialAdConfigDatas.Find(x => x.interstatialAdPlaceType == placeType);
        //    if (interstitialAdConfigData != null)
        //        return interstitialAdConfigData.GetTimeInterval(currentPlayerLevel);

        //    return 0f;
        //}

        public int GetShowInterstitialAdIntervalLevel(InterstatialAdPlaceType placeType)
        {
            DateTime firstSessionStartDT = DataManager.Instance.FirstSessionStartDateTime;
            var timeDuration = TimeManager.Now - firstSessionStartDT;

            InterstitialAdConfigData interstitialAdConfigData = interstitialAdConfigDatas.Find(x => x.interstatialAdPlaceType == placeType);
            if (interstitialAdConfigData != null)
            {
                int levelInterval = interstitialAdConfigData.GetLevelInterval(timeDuration.TotalDays);
                Debug.Log($"<color=yellow>Interstitial Ad : Time difference : {timeDuration.TotalDays.ToString("F2")} config Level : {levelInterval}</color>");
                return levelInterval;
            }

            Debug.Log($"<color=yellow>Interstitial Ad : Time difference : {timeDuration.TotalHours.ToString("F2")} No Config found !");
            return 1;
        }
    }

    public class InterstitialAdConfigData
    {
        public InterstatialAdPlaceType interstatialAdPlaceType;
        public int startLevel;
        public List<AdTimeIntervalLevelConfigData> adConfigDatas;

        //public float GetTimeInterval(int level)
        //{
        //    for (int j = adTimeIntervalLevelConfigDatas.Count - 1; j >= 0; j--)
        //    {
        //        if (level >= adTimeIntervalLevelConfigDatas[j].fromLevel)
        //        {
        //            return adTimeIntervalLevelConfigDatas[j].timeInterval;
        //        }
        //    }

        //    return adTimeIntervalLevelConfigDatas.First().timeInterval;
        //}

        public int GetLevelInterval(double currentNumberOfDays)
        {
            for (int i = 0; i < adConfigDatas.Count; i++)
            {
                if (adConfigDatas[i].numberOfDays >= currentNumberOfDays)
                    return adConfigDatas[i].levelInterval;
                else
                    currentNumberOfDays -= adConfigDatas[i].numberOfDays;
            }

            return adConfigDatas.GetLastItemFromList().levelInterval;
        }
    }

    public class AdTimeIntervalLevelConfigData
    {
        //public int fromLevel;
        //public float timeInterval;

        public int numberOfDays;
        public int levelInterval;
    }

    public class AdManagerPlayerData
    {
        public string lastRewardsAdsCountResetTime;
        public int currentShowedRewardedAdsCount;
    }

    public enum InterstatialAdPlaceType
    {
        Game_Win_Screen = 0,
        Reload_Level = 1,
    }
}