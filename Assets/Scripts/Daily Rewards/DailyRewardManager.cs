using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
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
            GameplayManager.onGameplayLevelOver += GameplayManager_onGameplayLevelOver;
        }

        private void OnDisable()
        {
            GameplayManager.onGameplayLevelOver -= GameplayManager_onGameplayLevelOver;
        }
        #endregion

        #region PUBLIC_FUNCTIONS
        public bool IsSystemUnlocked()
        {
            return PlayerPersistantData.GetMainPlayerProgressData().playerGameplayLevel >= DailyRewardDataSO.unlockLevel;
        }

        public int GetCurrentDay()
        {
            return PlayerPersistantData.GetDailyRewardsPlayerData().currentClaimedDay;
        }

        public bool CanClaimTodayReward()
        {
            if (!isInitialized)
                return false;

            var dailyReardsPlayerData = PlayerPersistantData.GetDailyRewardsPlayerData();
            if (dailyReardsPlayerData == null)
                dailyReardsPlayerData = new DailyRewardPlayerData();

            return string.IsNullOrEmpty(dailyReardsPlayerData.lastClaimedDate) || !CustomTime.TryParseDateTime(dailyReardsPlayerData.lastClaimedDate, out DateTime lastClaimedDate) ||
                (CustomTime.GetCurrentTime().Date - lastClaimedDate).TotalDays >= 1;
        }

        public void OnClaimTodayReward()
        {
            var dailyReardsPlayerData = PlayerPersistantData.GetDailyRewardsPlayerData();

            var currentDayReward = GetDayReward(dailyReardsPlayerData.currentClaimedDay);
            currentDayReward.GiveRewards();

            var currencyReward = currentDayReward.rewards.Find(x => x.GetRewardType() == RewardType.Currency);
            if (currencyReward != null)
                GameplayManager.Instance.LogCoinRewardFaucetEvent(AnalyticsConstants.ItemId_DailyRewards, currencyReward.GetAmount());

            dailyReardsPlayerData.currentClaimedDay++;
            if (dailyReardsPlayerData.currentClaimedDay >= DailyRewardDataSO.rewardDataSets.Count)
                dailyReardsPlayerData.currentClaimedDay = 0;

            dailyReardsPlayerData.lastClaimedDate = CustomTime.GetCurrentTime().Date.GetPlayerPrefsSaveString();
            PlayerPersistantData.SetDailyRewardsPlayerData(dailyReardsPlayerData);

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

            var dailyRewardsPlayerData = PlayerPersistantData.GetDailyRewardsPlayerData();
            if (dailyRewardsPlayerData == null)
            {
                dailyRewardsPlayerData = new DailyRewardPlayerData();
                PlayerPersistantData.SetDailyRewardsPlayerData(dailyRewardsPlayerData);
            }

            isInitialized = true;
        }
        #endregion

        #region CO-ROUTINES
        #endregion

        #region EVENT_HANDLERS
        private void GameplayManager_onGameplayLevelOver()
        {
            if (!isInitialized)
                InitializeDailyRewardsManager();
        }
        #endregion

        #region UI_CALLBACKS  
        #endregion

        #region EDITOR_FUNCTIONS
        [Button]
        public void Editor_LogPlayerData()
        {
            Debug.Log(SerializeUtility.SerializeObject(PlayerPersistantData.GetDailyRewardsPlayerData()));
        }

        [Button]
        public void Editor_ClearPlayerData()
        {
            PlayerPersistantData.SetDailyRewardsPlayerData(null);
        }

        [Button]
        public void Editor_ForwardDays(int days = 1)
        {
            var data = PlayerPersistantData.GetDailyRewardsPlayerData();
            if (data == null)
                return;

            if (days > 1)
            {
                data.currentClaimedDay += days - 1;
                if (data.currentClaimedDay >= DailyRewardDataSO.rewardDataSets.Count)
                    data.currentClaimedDay %= DailyRewardDataSO.rewardDataSets.Count;
            }

            data.lastClaimedDate = CustomTime.GetCurrentTime().Date.AddDays(-1).GetPlayerPrefsSaveString();

            PlayerPersistantData.SetDailyRewardsPlayerData(data);
        }
        #endregion
    }

    public class DailyRewardPlayerData
    {
        [JsonProperty("lcday")] public int currentClaimedDay;
        [JsonProperty("lcdate")] public string lastClaimedDate;
    }
}
