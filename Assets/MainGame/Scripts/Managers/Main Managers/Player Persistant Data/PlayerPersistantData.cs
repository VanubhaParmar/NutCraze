using Newtonsoft.Json;
using System.Collections.Generic;

namespace com.tag.nut_sort {
    public static class PlayerPersistantData
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        private static PersistentVariable<MainPlayerProgressData> _mainPlayerProgressData = new PersistentVariable<MainPlayerProgressData>(PlayerPrefsKeys.Main_Player_Progress_Data_Key, null);
        private static PersistentVariable<PlayerLevelProgressData> _playerLevelProgressData = new PersistentVariable<PlayerLevelProgressData>(PlayerPrefsKeys.Player_Level_Progress_Data_Key, null);
        private static PersistentVariable<TutorialsPlayerData> _tutorialsPlayerData = new PersistentVariable<TutorialsPlayerData>(PlayerPrefsKeys.Tutorial_Player_Data_Key, null);
        private static PersistentVariable<DailyGoalsPlayerPersistantData> _dailyGoalsPlayerData = new PersistentVariable<DailyGoalsPlayerPersistantData>(PlayerPrefsKeys.Daily_Goals_Player_Data_Key, null);
        private static PersistentVariable<LeaderBoardPlayerPersistantData> _leaderboardPlayerData = new PersistentVariable<LeaderBoardPlayerPersistantData>(PlayerPrefsKeys.Leaderboard_Player_Data_Key, null);
        private static PersistentVariable<DailyRewardPlayerData> _dailyRewardPlayerData = new PersistentVariable<DailyRewardPlayerData>(PlayerPrefsKeys.DailyReward_Player_Data_Key, null);
        private static PersistentVariable<GameStatsPlayerPersistantData> _gameStatsPlayerData = new PersistentVariable<GameStatsPlayerPersistantData>(PlayerPrefsKeys.GameStats_Player_Data_Key, null);
        private static PersistentVariable<AdjustEventPlayerData> _adjustEventPlayerData = new PersistentVariable<AdjustEventPlayerData>(PlayerPrefsKeys.AdjustEvents_Player_Data_Key, null);
        private static Dictionary<int, Currency> _currencyDict = new Dictionary<int, Currency>();
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public static MainPlayerProgressData GetMainPlayerProgressData()
        {
            return _mainPlayerProgressData.Value;
        }

        public static void SetMainPlayerProgressData(MainPlayerProgressData mainPlayerProgressData)
        {
            _mainPlayerProgressData.Value = mainPlayerProgressData;
        }

        public static PlayerLevelProgressData GetPlayerLevelProgressData()
        {
            return _playerLevelProgressData.Value;
        }

        public static void SetPlayerLevelProgressData(PlayerLevelProgressData playerLevelProgressData)
        {
            _playerLevelProgressData.Value = playerLevelProgressData;
        }

        public static DailyGoalsPlayerPersistantData GetDailyGoalsPlayerData()
        {
            return _dailyGoalsPlayerData.Value;
        }

        public static void SetDailyGoalsPlayerData(DailyGoalsPlayerPersistantData dailyGoalsPlayerData)
        {
            _dailyGoalsPlayerData.Value = dailyGoalsPlayerData;
        }

        public static LeaderBoardPlayerPersistantData GetLeaderboardPlayerData()
        {
            return _leaderboardPlayerData.Value;
        }

        public static void SetLeaderboardPlayerData(LeaderBoardPlayerPersistantData leaderboardPlayerData)
        {
            _leaderboardPlayerData.Value = leaderboardPlayerData;
        }

        public static DailyRewardPlayerData GetDailyRewardsPlayerData()
        {
            return _dailyRewardPlayerData.Value;
        }

        public static void SetDailyRewardsPlayerData(DailyRewardPlayerData dailyRewardPlayerData)
        {
            _dailyRewardPlayerData.Value = dailyRewardPlayerData;
        }

        public static GameStatsPlayerPersistantData GetGameStatsPlayerData()
        {
            return _gameStatsPlayerData.Value;
        }

        public static void SetGameStatsPlayerData(GameStatsPlayerPersistantData gameStatsPlayerData)
        {
            _gameStatsPlayerData.Value = gameStatsPlayerData;
        }

        public static TutorialsPlayerData GetTutorialsPlayerPersistantData()
        {
            return _tutorialsPlayerData.Value;
        }

        public static void SetTutorialsPlayerPersistantData(TutorialsPlayerData tutorialsPlayerData)
        {
            _tutorialsPlayerData.Value = tutorialsPlayerData;
        }

        public static AdjustEventPlayerData GetAdjustEventPlayerPersistantData()
        {
            return _adjustEventPlayerData.Value;
        }

        public static void SetAdjustEventPlayerPersistantData(AdjustEventPlayerData adjustEventPlayerData)
        {
            _adjustEventPlayerData.Value = adjustEventPlayerData;
        }

        public static Dictionary<int, Currency> GetCurrancyDictionary()
        {
            return _currencyDict;
        }

        public static void SetCurrancyDictionary(Dictionary<int, Currency> currencyDict)
        {
            _currencyDict = currencyDict;
        }

        // For Playfab Use Only >>>
        public static Dictionary<string, string> GetPlayerPersistantCurrancyData()
        {
            Dictionary<string, string> dataDictionary = new Dictionary<string, string>();
            foreach (var pair in _currencyDict)
            {
                dataDictionary.Add(pair.Value.key, pair.Value.Value.ToString());
            }
            return dataDictionary;
        }

        // For Playfab Use Only >>>
        public static void SetPlayerPersistantCurrancyData(Dictionary<string, string> currancyData)
        {
            foreach (var pair in currancyData)
            {
                foreach (var values in _currencyDict.Values)
                {
                    if (values.key == pair.Key && int.TryParse(pair.Value, out int currancyVal))
                    {
                        values.SetValue(currancyVal);
                        break;
                    }
                }
            }
        }

        public static Dictionary<string, string> GetPlayerPrefsData()
        {
            Dictionary<string, string> dataDictionary = new Dictionary<string, string>();
            dataDictionary.Add(PlayerPrefsKeys.Currancy_Data_Key, SerializeUtility.SerializeObject(GetPlayerPersistantCurrancyData()));
            dataDictionary.Add(_mainPlayerProgressData._key, _mainPlayerProgressData.RawValue);
            dataDictionary.Add(_playerLevelProgressData._key, _playerLevelProgressData.RawValue);
            dataDictionary.Add(_tutorialsPlayerData._key, _tutorialsPlayerData.RawValue);
            dataDictionary.Add(_gameStatsPlayerData._key, _gameStatsPlayerData.RawValue);
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
        [JsonProperty("pglev")] public int playerGameplayLevel;
        [JsonProperty("ubc")] public int undoBoostersCount;
        [JsonProperty("esbc")] public int extraScrewBoostersCount;
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
        public const string Main_Player_Progress_Data_Key = "MainPlayerProgressData";
        public const string Player_Level_Progress_Data_Key = "PlayerLevelProgressData";
        public const string Tutorial_Player_Data_Key = "TutorialPlayerData";
        public const string Daily_Goals_Player_Data_Key = "DailyGoalsPlayerData";
        public const string Leaderboard_Player_Data_Key = "LeaderboardPlayerData";
        public const string DailyReward_Player_Data_Key = "DailyRewardPlayerData";
        public const string GameStats_Player_Data_Key = "GameStatsPlayerData";
        public const string AdjustEvents_Player_Data_Key = "AdjustEventsPlayerData";
    }
}