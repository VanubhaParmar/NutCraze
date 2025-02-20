using GameAnalyticsSDK;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Tag.NutSort
{
    public class DailyRewardManager : Manager<DailyRewardManager>
    {
        #region PUBLIC_VARS
        public DailyRewardDataSO DailyRewardDataSO => _dailyRewardDataSO;
        #endregion

        #region PRIVATE_VARS
        [SerializeField] private DailyRewardDataSO _dailyRewardDataSO;
        private bool isInitialized = false;
        private DailyRewardPlayerData dailyRewardPlayerData;
        #endregion

        #region UNITY_CALLBACKS
        public override void Awake()
        {
            base.Awake();
            isInitialized = false;
            InitializeDailyRewardsManager();
            OnLoadingDone();
        }

        private void OnEnable()
        {
            LevelManager.Instance.RegisterOnLevelComplete(OnLevelCompelte);
        }

        private void OnDisable()
        {
            LevelManager.Instance.DeRegisterOnLevelComplete(OnLevelCompelte);
        }
        #endregion

        #region PUBLIC_FUNCTIONS
        public bool IsSystemUnlocked()
        {
            return DataManager.PlayerLevel.Value >= DailyRewardDataSO.unlockLevel;
        }

        public int GetCurrentDay()
        {
            return dailyRewardPlayerData.currentClaimedDay;
        }

        public bool CanClaimTodayReward()
        {
            if (!isInitialized)
                return false;

            return !dailyRewardPlayerData.lastClaimedDate.TryParseDateTime(out DateTime lastClaimedDate) ||
                (TimeManager.Now.Date - lastClaimedDate).TotalDays >= 1;
        }

        public void OnClaimTodayReward()
        {
            var currentDayReward = GetDayReward(dailyRewardPlayerData.currentClaimedDay);
            currentDayReward.GiveRewards();

            var currencyReward = currentDayReward.rewards.Find(x => x.GetRewardType() == RewardType.Currency);
            if (currencyReward != null)
            {
                int amount = currencyReward.GetAmount();
                GameStatsCollector.Instance.OnGameCurrencyChanged(CurrencyConstant.COIN, amount, GameCurrencyValueChangedReason.CURRENCY_EARNED_THROUGH_SYSTEM);
                AnalyticsManager.Instance.LogResourceEvent(GAResourceFlowType.Source, AnalyticsConstants.CoinCurrency, amount, AnalyticsConstants.ItemType_Reward, AnalyticsConstants.ItemId_DailyRewards);
            }

            dailyRewardPlayerData.currentClaimedDay++;
            if (dailyRewardPlayerData.currentClaimedDay >= DailyRewardDataSO.rewardDataSets.Count)
                dailyRewardPlayerData.currentClaimedDay = 0;

            dailyRewardPlayerData.lastClaimedDate = TimeManager.Now.Date.GetPlayerPrefsSaveString();
            SaveData();
            GameManager.RaiseOnRewardsClaimedUIRefresh();
        }

        public void ShowDailyRewardsViewAndClaimTodayRewards(Action actionToCallOnClaimComplete)
        {
            MainSceneUIManager.Instance.GetView<DailyRewardView>().ShowAndCollectRewards(actionToCallOnClaimComplete);
        }

        public RewardsDataSO GetDayReward(int day)
        {
            if (day < 0 || day >= _dailyRewardDataSO.rewardDataSets.Count)
                return _dailyRewardDataSO.rewardDataSets[0];
            return _dailyRewardDataSO.rewardDataSets[day];
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        public void InitializeDailyRewardsManager()
        {
            if (!IsSystemUnlocked())
                return;

            dailyRewardPlayerData = DataManager.Instance.GetDailyRewardsPlayerData();
            if (dailyRewardPlayerData == null)
            {
                dailyRewardPlayerData = new DailyRewardPlayerData();
                SaveData();
            }

            isInitialized = true;
        }

        private void SaveData()
        {
            DataManager.Instance.SaveDailyRewardsPlayerData(dailyRewardPlayerData);
        }
        #endregion

        #region CO-ROUTINES
        #endregion

        #region EVENT_HANDLERS
        private void OnLevelCompelte()
        {
            if (!isInitialized)
                InitializeDailyRewardsManager();
        }
        #endregion

        #region UI_CALLBACKS  
        #endregion

        #region EDITOR_FUNCTIONS
#if UNITY_EDITOR
        [Button]
        public void Editor_ForwardDays(int days = 1)
        {
            if (dailyRewardPlayerData == null)
                return;

            if (days > 1)
            {
                dailyRewardPlayerData.currentClaimedDay += days - 1;
                if (dailyRewardPlayerData.currentClaimedDay >= DailyRewardDataSO.rewardDataSets.Count)
                    dailyRewardPlayerData.currentClaimedDay %= DailyRewardDataSO.rewardDataSets.Count;
            }

            dailyRewardPlayerData.lastClaimedDate = TimeManager.Now.Date.AddDays(-1).GetPlayerPrefsSaveString();
            SaveData();
        }
#endif
        #endregion
    }

    public class DailyRewardPlayerData
    {
        [JsonProperty("lcday")] public int currentClaimedDay;
        [JsonProperty("lcdate")] public string lastClaimedDate;
    }
}
