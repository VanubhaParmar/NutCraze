using Newtonsoft.Json;
using System.Collections.Generic;

namespace Tag.NutSort {
    public class PlayerData
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        private PersistentVariable<PlayerLevelProgressData> playerLevelProgressData = new PersistentVariable<PlayerLevelProgressData>(PlayerPrefsKeys.Player_Level_Progress_Data_Key, null);
        private PersistentVariable<TutorialsPlayerData> tutorialsPlayerData = new PersistentVariable<TutorialsPlayerData>(PlayerPrefsKeys.Tutorial_Player_Data_Key, null);
        private PersistentVariable<DailyGoalsPlayerPersistantData> dailyGoalsPlayerData = new PersistentVariable<DailyGoalsPlayerPersistantData>(PlayerPrefsKeys.Daily_Goals_Player_Data_Key, null);
        private PersistentVariable<LeaderBoardPlayerPersistantData> leaderboardPlayerData = new PersistentVariable<LeaderBoardPlayerPersistantData>(PlayerPrefsKeys.Leaderboard_Player_Data_Key, null);
        private PersistentVariable<DailyRewardPlayerData> dailyRewardPlayerData = new PersistentVariable<DailyRewardPlayerData>(PlayerPrefsKeys.DailyReward_Player_Data_Key, null);
        private PersistentVariable<GameStatsPlayerPersistantData> gameStatsPlayerData = new PersistentVariable<GameStatsPlayerPersistantData>(PlayerPrefsKeys.GameStats_Player_Data_Key, null);
        private PersistentVariable<AdjustEventPlayerData> adjustEventPlayerData = new PersistentVariable<AdjustEventPlayerData>(PlayerPrefsKeys.AdjustEvents_Player_Data_Key, null);
        private PersistentVariable<bool> noAdsPurchaseState = new PersistentVariable<bool>(PlayerPrefsKeys.NoAdsPurchaseStateKey, false);
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public bool IsNoAdsPackPurchased()
        {
            return noAdsPurchaseState.Value;
        }

        public void PurchaseNoAdsPack()
        {
            noAdsPurchaseState.Value = true;
        }

        public PlayerLevelProgressData GetPlayerLevelProgressData()
        {
            return playerLevelProgressData.Value;
        }

        public void SavePlayerLevelProgressData(PlayerLevelProgressData playerLevelProgressData)
        {
            this.playerLevelProgressData.Value = playerLevelProgressData;
        }

        public DailyGoalsPlayerPersistantData GetDailyGoalsPlayerData()
        {
            return dailyGoalsPlayerData.Value;
        }

        public void SaveDailyGoalsPlayerData(DailyGoalsPlayerPersistantData dailyGoalsPlayerData)
        {
            this.dailyGoalsPlayerData.Value = dailyGoalsPlayerData;
        }

        public LeaderBoardPlayerPersistantData GetLeaderboardPlayerData()
        {
            return leaderboardPlayerData.Value;
        }

        public void SaveLeaderboardPlayerData(LeaderBoardPlayerPersistantData leaderboardPlayerData)
        {
            this.leaderboardPlayerData.Value = leaderboardPlayerData;
        }

        public DailyRewardPlayerData GetDailyRewardsPlayerData()
        {
            return dailyRewardPlayerData.Value;
        }

        public void SaveDailyRewardsPlayerData(DailyRewardPlayerData dailyRewardPlayerData)
        {
            this.dailyRewardPlayerData.Value = dailyRewardPlayerData;
        }

        public GameStatsPlayerPersistantData GetGameStatsPlayerData()
        {
            return gameStatsPlayerData.Value;
        }

        public void SaveGameStatsPlayerData(GameStatsPlayerPersistantData gameStatsPlayerData)
        {
            this.gameStatsPlayerData.Value = gameStatsPlayerData;
        }

        public TutorialsPlayerData GetTutorialsPlayerPersistantData()
        {
            return tutorialsPlayerData.Value;
        }

        public void SaveTutorialsPlayerData(TutorialsPlayerData tutorialsPlayerData)
        {
            this.tutorialsPlayerData.Value = tutorialsPlayerData;
        }

        public AdjustEventPlayerData GetAdjustEventPlayerData()
        {
            return adjustEventPlayerData.Value;
        }

        public void SaveAdjustEventPlayerData(AdjustEventPlayerData adjustEventPlayerData)
        {
            this.adjustEventPlayerData.Value = adjustEventPlayerData;
        }

        public Dictionary<string, string> GetPlayerPrefsData()
        {
            Dictionary<string, string> dataDictionary = new Dictionary<string, string>();
            dataDictionary.Add(playerLevelProgressData._key, playerLevelProgressData.RawValue);
            dataDictionary.Add(noAdsPurchaseState._key, noAdsPurchaseState.RawValue);
            dataDictionary.Add(tutorialsPlayerData._key, tutorialsPlayerData.RawValue);
            dataDictionary.Add(gameStatsPlayerData._key, gameStatsPlayerData.RawValue);
            return dataDictionary;
        }


        #endregion

        #region PRIVATE_METHODS
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }

    #region MAIN_PLAYER_PROGRESS_DATA
    public class MainPlayerProgressData
    {
        [JsonProperty("naps")] public bool noAdsPurchaseState;
    }

    public class PlayerLevelProgressData
    {
        [JsonProperty("cpl")] public int currentPlayingLevel;
        [JsonProperty("cplt")] public LevelType currentPlayingLevelType;
        [JsonProperty("cplpmdi")] public List<PlayerLevelProgressMoveDataInfo> playerLevelProgressMoveDataInfos = new List<PlayerLevelProgressMoveDataInfo>();
        [JsonProperty("bscu")] public int boosterScrewCapacityUpgrade;
        [JsonProperty("crt")] public int currentRunningTime;
    }

    public class PlayerLevelProgressMoveDataInfo
    {
        [JsonProperty("mfs")] public GridCellId moveFromScrew;
        [JsonProperty("mts")] public GridCellId moveToScrew;
        [JsonProperty("tnon")] public int transferredNumberOfNuts;

        public PlayerLevelProgressMoveDataInfo() { }
        public PlayerLevelProgressMoveDataInfo(GridCellId moveFromScrew, GridCellId moveToScrew, int transferredNumberOfNuts)
        {
            this.moveFromScrew = moveFromScrew;
            this.moveToScrew = moveToScrew;
            this.transferredNumberOfNuts = transferredNumberOfNuts;
        }
    }

    public class CurrencyMappingData
    {
        [JsonProperty("cid"), CurrencyId] public int currencyID;
        [JsonProperty("cur")] public Currency currency;
    }
    #endregion

    public class PlayerPrefsKeys
    {
        public const string Currancy_Data_Key = "CurrancyPlayerData";
        public const string NoAdsPurchaseStateKey = "NoAdsPurchaseStateKey";
        public const string Player_Level_Progress_Data_Key = "PlayerLevelProgressData";
        public const string Tutorial_Player_Data_Key = "TutorialPlayerData";
        public const string Daily_Goals_Player_Data_Key = "DailyGoalsPlayerData";
        public const string Leaderboard_Player_Data_Key = "LeaderboardPlayerData";
        public const string DailyReward_Player_Data_Key = "DailyRewardPlayerData";
        public const string GameStats_Player_Data_Key = "GameStatsPlayerData";
        public const string AdjustEvents_Player_Data_Key = "AdjustEventsPlayerData";
    }
}