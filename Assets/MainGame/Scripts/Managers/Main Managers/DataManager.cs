using GameAnalyticsSDK;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tag.NutSort
{
    public class DataManager : SerializedManager<DataManager>
    {
        #region PRIVATE_VARIABLES
        [SerializeField] private Dictionary<int, Currency> currencyMapping = new Dictionary<int, Currency>();
        [SerializeField] private Dictionary<int, Currency> boosterMapping = new Dictionary<int, Currency>();
        private PlayerData playerData;
        #endregion

        #region PUBLIC_VARIABLES
        #endregion

        #region PROPERTIES
        public static PlayerData PlayerData
        {
            get
            {
                if (Instance.playerData == null)
                    Instance.playerData = new PlayerData();
                return Instance.playerData;
            }
        }
        public static Currency PlayerLevel
        {
            get
            {
                return Instance.currencyMapping[CurrencyConstant.LEVEL];
            }
        }
        #endregion

        #region Unity_callback

        public override void Awake()
        {
            base.Awake();
            Init();
            OnLoadingDone();
        }

        private void Init()
        {
            PlayerPrefbsHelper.SaveData = true;
            CurrencyInit();
            BoosterInit();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            OnCurrencyUnload();
            OnBoosterUnLood();
        }

        public Currency GetCurrency(int currencyID)
        {
            if (currencyMapping.ContainsKey(currencyID))
                return currencyMapping[currencyID];
            return null;
        }

        public void AddCurrency(int currencyID, int value, Action successAction = null, Vector3 position = default(Vector3))
        {
            if (currencyMapping.ContainsKey(currencyID))
                currencyMapping[currencyID].Add(value, successAction, position);
        }

        public bool HasEnoughCurrency(int currencyId, int value)
        {
            if (currencyMapping.ContainsKey(currencyId))
                return currencyMapping[currencyId].HasEnoughValue(value);
            return false;
        }

        public bool HasEnoughBooster(int bosterId, int value = 1)
        {
            if (boosterMapping.ContainsKey(bosterId))
                return boosterMapping[bosterId].HasEnoughValue(value);
            return false;
        }

        public void AddBoosters(int boosterTye, int value)
        {
            if (currencyMapping.ContainsKey(boosterTye))
                currencyMapping[boosterTye].Add(value);
        }

        public bool CanUseUndoBooster()
        {
            return HasEnoughBooster(BoosterIdConstant.UNDO, 1);
        }

        public Currency GetBooster(int boosterId)
        {
            if (boosterMapping.ContainsKey(boosterId))
                return boosterMapping[boosterId];
            return null;
        }

        public bool CanUseExtraScrewBooster()
        {
            return HasEnoughBooster(BoosterIdConstant.EXTRA_SCREW, 1);
        }

        public void PurchaseNoAdsPack()
        {
            PlayerData.PurchaseNoAdsPack();
            RaiseOnNoAdsPackPurchased();
        }

        public bool CanPurchaseNoAdsPack()
        {
            return !IsNoAdsPackPurchased();
        }

        public bool IsNoAdsPackPurchased()
        {
            return PlayerData.IsNoAdsPackPurchased();
        }
        #endregion

        #region private Methods

        private void CurrencyInit()
        {
            foreach (var item in currencyMapping)
                item.Value.Init();
        }

        private void BoosterInit()
        {
            foreach (var item in boosterMapping)
                item.Value.Init();
        }

        private void OnCurrencyUnload()
        {
            foreach (var item in currencyMapping)
                item.Value.OnDestroy();
        }

        private void OnBoosterUnLood()
        {
            foreach (var item in boosterMapping)
                item.Value.OnDestroy();
        }
        #endregion

        #region EVENT_HANDLERS
        public delegate void OnNoAdsPackEvent();
        public static event OnNoAdsPackEvent onNoAdsPackPurchased;
        public static void RaiseOnNoAdsPackPurchased()
        {
            if (onNoAdsPackPurchased != null)
                onNoAdsPackPurchased();
        }
        #endregion

        #region public methods
        public Dictionary<int, Currency> GetAllCurrency()
        {
            return currencyMapping;
        }

        public PlayerLevelProgressData GetPlayerLevelProgressData()
        {
            return PlayerData.GetPlayerLevelProgressData();
        }

        public void SavePlayerLevelProgressData(PlayerLevelProgressData playerLevelProgressData)
        {
            PlayerData.SavePlayerLevelProgressData(playerLevelProgressData);
        }

        public DailyGoalsPlayerPersistantData GetDailyGoalsPlayerData()
        {
            return PlayerData.GetDailyGoalsPlayerData();
        }

        public void SaveDailyGoalsPlayerData(DailyGoalsPlayerPersistantData dailyGoalsPlayerData)
        {
            PlayerData.SaveDailyGoalsPlayerData(dailyGoalsPlayerData);
        }

        public LeaderBoardPlayerPersistantData GetLeaderboardPlayerData()
        {
            return PlayerData.GetLeaderboardPlayerData();
        }

        public void SaveLeaderboardPlayerData(LeaderBoardPlayerPersistantData leaderboardPlayerData)
        {
            PlayerData.SaveLeaderboardPlayerData(leaderboardPlayerData);
        }

        public DailyRewardPlayerData GetDailyRewardsPlayerData()
        {
            return PlayerData.GetDailyRewardsPlayerData();
        }

        public void SaveDailyRewardsPlayerData(DailyRewardPlayerData dailyRewardPlayerData)
        {
            PlayerData.SaveDailyRewardsPlayerData(dailyRewardPlayerData);
        }

        public GameStatsPlayerPersistantData GetGameStatsPlayerData()
        {
            return PlayerData.GetGameStatsPlayerData();
        }

        public void SaveGameStatsPlayerData(GameStatsPlayerPersistantData gameStatsPlayerData)
        {
            PlayerData.SaveGameStatsPlayerData(gameStatsPlayerData);
        }

        public TutorialsPlayerData GetTutorialsPlayerPersistantData()
        {
            return PlayerData.GetTutorialsPlayerPersistantData();
        }

        public void SaveTutorialsPlayerData(TutorialsPlayerData tutorialsPlayerData)
        {
            PlayerData.SaveTutorialsPlayerData(tutorialsPlayerData);
        }

        public AdjustEventPlayerData GetAdjustEventPlayerData()
        {
            return PlayerData.GetAdjustEventPlayerData();
        }

        public void SaveAdjustEventPlayerData(AdjustEventPlayerData adjustEventPlayerData)
        {
            PlayerData.SaveAdjustEventPlayerData(adjustEventPlayerData);
        }

        public static Dictionary<string, string> GetPlayerPrefsData()
        {
            Dictionary<string, string> dataDictionary = PlayerData.GetPlayerPrefsData();
            // add all currency and boosters here
            return dataDictionary;
        }
        #endregion

        #region UNITY_EDITOR_FUNCTIONS
#if UNITY_EDITOR
        [Button]
        public void AddCurrency([CurrencyId] int currencyId, Currency currency)
        {
            if (!currencyMapping.ContainsKey(currencyId))
                currencyMapping.Add(currencyId, currency);
        }

        [Button]
        public void AddBooster([BoosterId] int currencyId, Currency currency)
        {
            if (!boosterMapping.ContainsKey(currencyId))
                boosterMapping.Add(currencyId, currency);
        }
#endif
        #endregion
    }
}