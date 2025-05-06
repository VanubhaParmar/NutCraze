using DG.Tweening;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tag.NutSort
{
    public class LevelFailManager : SerializedManager<LevelFailManager>
    {
        #region PRIVATE_VARIABLES
        [SerializeField] private Dictionary<LevelFailABTestType, LevelFailReviveConfig> reviveConfigMapping = new Dictionary<LevelFailABTestType, LevelFailReviveConfig>();
        [SerializeField] private LevelFailABTestRemoteConfig levelFailABTestRemoteConfig;

        private const string LEVEL_FAIL_AB_TEST_TYPE_PREFKEY = "LevelFailABTestTypePrefsKey";
        #endregion

        #region PUBLIC_VARIABLES
        #endregion

        #region PROPERTIES
        public LevelFailReviveConfig ReviveConfig
        {
            get => reviveConfigMapping[CurrentTestingType];
        }

        public LevelFailABTestType CurrentTestingType
        {
            get
            {
                if (!PlayerPrefbsHelper.HasKey(LEVEL_FAIL_AB_TEST_TYPE_PREFKEY))
                {
                    int defaultValue = (int)GetDefaultLevelFailABTestType();
                    PlayerPrefbsHelper.SetInt(LEVEL_FAIL_AB_TEST_TYPE_PREFKEY, defaultValue);
                }
                return (LevelFailABTestType)PlayerPrefbsHelper.GetInt(LEVEL_FAIL_AB_TEST_TYPE_PREFKEY);
            }
            set
            {
                PlayerPrefbsHelper.SetInt(LEVEL_FAIL_AB_TEST_TYPE_PREFKEY, (int)value);
            }
        }
        private LevelFailView LevelFailView => MainSceneUIManager.Instance.GetView<LevelFailView>();
        private GameplayView GameplayView => MainSceneUIManager.Instance.GetView<GameplayView>();
        private ShopView ShopView => MainSceneUIManager.Instance.GetView<ShopView>();
        private LevelFailSaveData LevelFailSaveData => LevelProgressManager.Instance.LevelSaveData.levelFailSaveData;
        private GameplayManager GameplayManager => GameplayManager.Instance;
        #endregion

        #region UNITY_CALLBACKS
        public override void Awake()
        {
            base.Awake();
            StartCoroutine(WaitForRCValuesFetched(() =>
            {
                Debug.Log($"levelFailABTestRemoteConfig.GetValue<int>() {levelFailABTestRemoteConfig.GetValue<int>()}  {(LevelFailABTestType)levelFailABTestRemoteConfig.GetValue<int>()}");
                CurrentTestingType = (LevelFailABTestType)levelFailABTestRemoteConfig.GetValue<int>();
                OnLoadingDone();
            }));
        }
        #endregion

        #region PUBLIC_METHODS
        public void CheckForLevelFail()
        {
            if (!LevelProgressManager.Instance.IsAnyMoveDone)
                return;

            if (GameplayManager.IsPlayingLevel && GameplayManager.GameplayStateData.TotalPossibleMoves <= 0 && !IsLevelComplete())
            {
                OnLevelFail();
            }
        }

        public void OnLevelFail()
        {
            LevelProgressManager.Instance.PauseLevelTimer();
            HandleLevelFail();
        }
        #endregion

        #region PRIVATE_METHODS
        private LevelFailABTestType GetDefaultLevelFailABTestType()
        {
            return (LevelFailABTestType)levelFailABTestRemoteConfig.GetDefaultValue<int>();
        }

        private bool IsLevelComplete()
        {
            return !GameplayManager.GameplayStateData.levelNutsUniqueColorsSortCompletionState.ContainsValue(false);// All Screw Sort is Completed
        }

        [Button]
        private void HandleLevelFail()
        {
            if (LevelProgressManager.Instance.CurrentLevelType == LevelType.SPECIAL_LEVEL)
            {
                HandleSpecailLevelFail();
                return;
            }

            switch (CurrentTestingType)
            {
                case LevelFailABTestType.Control:
                    HandleLevelFailByVariant1ControlNoChange();
                    break;
                case LevelFailABTestType.Variant2_HighlightBoosters:
                    HandleLevelFailByVariant2HighlightBoosters();
                    break;
                case LevelFailABTestType.Variant3_Ads:
                    HandleLevelFailByVariant3Ads();
                    break;
                case LevelFailABTestType.Variant4_Coins:
                    HandleLevelFailByVariant4Coins();
                    break;
                case LevelFailABTestType.Variant5_AdsAndCoins:
                    HandleLevelFailByVariant5AdsAndCoins();
                    break;
                default:
                    break;
            }
        }



        private void ReviveWithAd(int screwCapacity)
        {
            AdManager.Instance.ShowRewardedAd(OnAdCompleted, RewardAdShowCallType.Level_Fail_Ad, AnalyticsConstants.GA_LevelFailAdPlace);
            void OnAdCompleted()
            {
                ScrewManager.Instance.AddSimpleScrew(screwCapacity);
                LevelFailSaveData.revivedWithAds++;
                LevelProgressManager.Instance.ResumeLevelTimer();
                CloseLevelFailPopup();
            }
        }

        private void ReviveWithCoins(int coinAmount, int screwCapacity)
        {
            Currency coin = DataManager.Instance.GetCurrency(CurrencyConstant.COIN);
            if (coin.HasEnoughValue(coinAmount))
            {
                coin.Add(-coinAmount);
                ScrewManager.Instance.AddSimpleScrew(screwCapacity);
                LevelFailSaveData.revivedWithCoin++;
                LevelProgressManager.Instance.ResumeLevelTimer();
                CloseLevelFailPopup();
                GameStatsCollector.Instance.OnGameCurrencyChanged(CurrencyConstant.COIN, -coinAmount, CurrencyChangeReason.SPENT);
            }
            else
            {
                ShopView.Show();
            }
        }

        private void RestartLevel()
        {
            CloseLevelFailPopup();
            GameplayManager.Instance.RestartGamePlay();
        }

        private void HandleSpecailLevelFail()
        {
            int screwCapacityToAdd = 1;
            LevelFailView.Show(RestartLevel,
                canReviveWithAds: CanReviveWithAds(),
                canShowRetryButton: true,
                screwCapacityWithAds: screwCapacityToAdd,
                onWatchAdClicked: () => { ReviveWithAd(screwCapacityToAdd); });
        }

        private void HandleLevelFailByVariant1ControlNoChange()
        {
            LevelProgressManager.Instance.ResumeLevelTimer();
            //there is no any change in this variant
        }

        private void HandleLevelFailByVariant2HighlightBoosters()
        {
            GameplayView.SetLevelFailLayout(true, () =>
            {
                LevelProgressManager.Instance.ResumeLevelTimer();
            });
        }

        private void HandleLevelFailByVariant3Ads()
        {
            GameplayView.Hide();
            int screwCapacityToAdd = ScrewManager.Instance.GetMaxCapacityFromPeerScrew();
            LevelFailView.Show(RestartLevel,
                canReviveWithAds: CanReviveWithAds(),
                screwCapacityWithAds: screwCapacityToAdd,
                canShowRetryButton: true,
                canShowCloseButton: false,
                onWatchAdClicked: () => { ReviveWithAd(screwCapacityToAdd); });
        }

        private void HandleLevelFailByVariant4Coins()
        {
            GameplayView.Hide();
            int screwCapacityToAdd = ScrewManager.Instance.GetMaxCapacityFromPeerScrew();
            int coinAmount = GetCoinAmount();
            LevelFailView.Show(RestartLevel,
                canReviveWithCoins: CanReviveWithCoins(),
                canShowRetryButton: true,
                canShowCloseButton: false,
                coinAmount: coinAmount,
                screwCapacityWithCoins: screwCapacityToAdd,
                onSpendCoinsClicked: () => ReviveWithCoins(coinAmount, screwCapacityToAdd));
        }

        private void HandleLevelFailByVariant5AdsAndCoins()
        {
            GameplayView.Hide();
            int coinAmount = GetCoinAmount();
            int screwCapacityToAddWithCoin = ScrewManager.Instance.GetMaxCapacityFromPeerScrew();
            int screwCapacityToAddWithAds = 1;
            LevelFailView.Show(RestartLevel,
                canReviveWithAds: CanReviveWithAds(),
                canReviveWithCoins: CanReviveWithCoins(),
                canShowRetryButton: false,
                canShowCloseButton: true,
                coinAmount: coinAmount,
                screwCapacityWithAds: screwCapacityToAddWithAds,
                screwCapacityWithCoins: screwCapacityToAddWithCoin,
                onWatchAdClicked: () => { ReviveWithAd(screwCapacityToAddWithAds); },
                onSpendCoinsClicked: () => ReviveWithCoins(coinAmount, screwCapacityToAddWithCoin));
        }

        private bool CanReviveWithAds()
        {
            return LevelFailSaveData != null && LevelFailSaveData.revivedWithAds < ReviveConfig.maxReviveThroughAds;
        }

        private bool CanReviveWithCoins()
        {
            return LevelFailSaveData != null && LevelFailSaveData.revivedWithCoin < ReviveConfig.MaxReviveThroughCoins;
        }

        private int GetCoinAmount()
        {
            if (LevelFailSaveData != null)
                return ReviveConfig.GetCoinAmount(LevelFailSaveData.revivedWithCoin);
            return 0;
        }

        private void CloseLevelFailPopup()
        {
            LevelFailView.Hide();
            GameplayView.Show();
        }
        #endregion

        #region COROUTINES
        private IEnumerator WaitForRCValuesFetched(Action onComplete)
        {
#if UNITY_EDITOR
            yield return null;
            onComplete.Invoke();
#elif !UNITY_EDITOR
            yield return new WaitForGARemoteConfigToLoad();
            onComplete.Invoke();
#endif
        }
        #endregion
    }

    public enum LevelFailABTestType
    {
        Control = 0,         // No change
        Variant2_HighlightBoosters = 1,  // Show "No Moves Left" at bottom with highlight boosters button
        Variant3_Ads = 2,    // Show "Out of move" popup with Play On (ad) and Retry buttons
        Variant4_Coins = 3,  // Similar to Variant 3 but with coins instead of ads
        Variant5_AdsAndCoins = 4  // Choice between watching ad or spending coins
    }

    public class LevelFailReviveConfig
    {
        public int maxReviveThroughAds;
        public Dictionary<int, int> levelFailCoinMapping = new Dictionary<int, int>();
        public int MaxReviveThroughCoins => levelFailCoinMapping.Count;

        public int GetCoinAmount(int revivedWithCoin)
        {
            if (levelFailCoinMapping.ContainsKey(revivedWithCoin))
                return levelFailCoinMapping[revivedWithCoin];
            return levelFailCoinMapping.Last().Value;
        }
    }

    [Serializable]
    public class LevelFailSaveData
    {
        [JsonProperty("rwcc")] public int revivedWithCoin;
        [JsonProperty("rwac")] public int revivedWithAds;
    }
}