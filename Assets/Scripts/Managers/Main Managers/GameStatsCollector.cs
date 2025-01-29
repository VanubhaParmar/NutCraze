using Mediation.Runtime.Scripts.Track;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class GameStatsCollector : SerializedManager<GameStatsCollector>
    {
        #region PUBLIC_VARIABLES
        [Header("TST")]
        public bool isUseTstTime;
        [ShowIf("isUseTstTime")] public string tstTime;

        public int CurrentPlayedLevelsInThisSession => currentPlayedLevelsInThisSession;
        public string LastPlayedSessionTimeString => lastPlayedSessionTimeString;
        #endregion

        #region PRIVATE_VARIABLES
        private const int Save_Data_Of_Past_X_Days = 7;
        [ShowInInspector, ReadOnly] private int currentPlayedLevelsInThisSession = 0;

        private string lastPlayedSessionTimeString;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        public override void Awake()
        {
            base.Awake();
            InitializeGameStatesPlayerData();
            UpdateLastDaysPlayedLevel();
            OnLoadingDone();

#if UNITY_EDITOR
            Instance_OnNewSessionStart();
#endif
        }

        private void OnEnable()
        {
            DataManager.Instance.GetCurrency((int)CurrencyType.Coin).RegisterOnCurrencyChangeEvent(CoinCurrency_Change);
            GameplayManager.onGameplayLevelStart += GameplayManager_onGameplayLevelStart;
            GameplayManager.onGameplayLevelOver += GameplayManager_onGameplayLevelOver;
            GameplayManager.onGameplayLevelReload += GameplayManager_onGameplayLevelReload;

            TrackingBridge.Instance.OnNewSessionStart += Instance_OnNewSessionStart;
        }

        private void OnDisable()
        {
            DataManager.Instance.GetCurrency((int)CurrencyType.Coin).RemoveOnCurrencyChangeEvent(CoinCurrency_Change);
            GameplayManager.onGameplayLevelStart -= GameplayManager_onGameplayLevelStart;
            GameplayManager.onGameplayLevelOver -= GameplayManager_onGameplayLevelOver;
            GameplayManager.onGameplayLevelReload -= GameplayManager_onGameplayLevelReload;

            TrackingBridge.Instance.OnNewSessionStart -= Instance_OnNewSessionStart;
        }
        #endregion

        #region PUBLIC_METHODS
        public int GetAveragePlayedLevelsInPastDays()
        {
            var statsData = PlayerPersistantData.GetGameStatsPlayerData();
            if (statsData == null || statsData.lastDaysPlayedLevels.Count == 0)
                return 0;

            float avgLevels = 0;
            foreach (var kvp in statsData.lastDaysPlayedLevels)
            {
                avgLevels += kvp.Value;
            }

            return Mathf.CeilToInt(avgLevels / statsData.lastDaysPlayedLevels.Count);
        }

        public void OnGameCurrencyChanged(int currencyId, int currencyChange, GameCurrencyValueChangedReason gameCurrencyValueChangedReason = GameCurrencyValueChangedReason.CURRENCY_SPENT)
        {
            Debug.Log($"Game Currency Changed : {currencyId} {currencyChange} {gameCurrencyValueChangedReason.ToString()}");
            var statsData = PlayerPersistantData.GetGameStatsPlayerData();

            if (gameCurrencyValueChangedReason == GameCurrencyValueChangedReason.CURRENCY_EARNED_THROUGH_ADS_OR_IAP || gameCurrencyValueChangedReason == GameCurrencyValueChangedReason.CURRENCY_EARNED_THROUGH_SYSTEM)
                statsData.numberOfEarnActionsInSession++;

            if (statsData.levelBasedCurrencyData.ContainsKey(currencyId))
            {
                var levelBasedCurrencyData = SerializeUtility.DeserializeObject<GameStatCurrencyInfo>(statsData.levelBasedCurrencyData[currencyId]);
                if (levelBasedCurrencyData != null)
                {
                    if (gameCurrencyValueChangedReason == GameCurrencyValueChangedReason.CURRENCY_SPENT)
                        levelBasedCurrencyData.spend += currencyChange;
                    else if (gameCurrencyValueChangedReason == GameCurrencyValueChangedReason.CURRENCY_EARNED_THROUGH_ADS_OR_IAP)
                        levelBasedCurrencyData.earn += currencyChange;
                    else if (gameCurrencyValueChangedReason == GameCurrencyValueChangedReason.CURRENCY_EARNED_THROUGH_SYSTEM)
                        levelBasedCurrencyData.freeEarn += currencyChange;

                    statsData.levelBasedCurrencyData[currencyId] = SerializeUtility.SerializeObject(levelBasedCurrencyData);
                }
            }

            if (statsData.sessionBasedCurrencyData.ContainsKey(currencyId))
            {
                var sessionBasedCurrencyData = SerializeUtility.DeserializeObject<GameStatCurrencyInfo>(statsData.sessionBasedCurrencyData[currencyId]);
                if (sessionBasedCurrencyData != null)
                {
                    if (gameCurrencyValueChangedReason == GameCurrencyValueChangedReason.CURRENCY_SPENT)
                        sessionBasedCurrencyData.spend += currencyChange;
                    else if (gameCurrencyValueChangedReason == GameCurrencyValueChangedReason.CURRENCY_EARNED_THROUGH_ADS_OR_IAP)
                        sessionBasedCurrencyData.earn += currencyChange;
                    else if (gameCurrencyValueChangedReason == GameCurrencyValueChangedReason.CURRENCY_EARNED_THROUGH_SYSTEM)
                        sessionBasedCurrencyData.freeEarn += currencyChange;

                    statsData.sessionBasedCurrencyData[currencyId] = SerializeUtility.SerializeObject(sessionBasedCurrencyData);
                }
            }

            PlayerPersistantData.SetGameStatsPlayerData(statsData);
        }

        public void OnPopUpTriggered(GameStatPopUpTriggerType gameStatPopUpTriggerType = GameStatPopUpTriggerType.USER_TRIGGERED)
        {
            var statsData = PlayerPersistantData.GetGameStatsPlayerData();
            if (gameStatPopUpTriggerType == GameStatPopUpTriggerType.USER_TRIGGERED)
                statsData.numberOfUserTriggeredPopups++;
            else if (gameStatPopUpTriggerType == GameStatPopUpTriggerType.SYSTEM_TRIGGERED)
                statsData.numberOfSystemTriggeredPopups++;

            PlayerPersistantData.SetGameStatsPlayerData(statsData);
        }

        public void GameCurrenciesData_MarkNewLevel()
        {
            var statsData = PlayerPersistantData.GetGameStatsPlayerData();

            statsData.numberOfUserTriggeredPopups = 0;
            statsData.numberOfSystemTriggeredPopups = 0;

            statsData.levelBasedCurrencyData.Clear();

            var currencyVals = Enum.GetValues(typeof(CurrencyType));
            for (int i = 0; i < currencyVals.Length; i++)
            {
                int currencyId = (int)currencyVals.GetValue(i);
                var curData = DataManager.Instance.GetCurrency(currencyId);
                if (curData == null) continue;

                GameStatCurrencyInfo currencyInfo = new GameStatCurrencyInfo();
                currencyInfo.currencyName = curData.currencyName;
                currencyInfo.startValue = curData.Value;

                if (statsData.levelBasedCurrencyData.ContainsKey(currencyId))
                    statsData.levelBasedCurrencyData[currencyId] = SerializeUtility.SerializeObject(currencyInfo);
                else
                    statsData.levelBasedCurrencyData.Add(currencyId, SerializeUtility.SerializeObject(currencyInfo));
            }

            PlayerPersistantData.SetGameStatsPlayerData(statsData);
        }

        public void GameCurrenciesData_MarkLevelEnd()
        {
            var statsData = PlayerPersistantData.GetGameStatsPlayerData();

            var currencyVals = Enum.GetValues(typeof(CurrencyType));
            for (int i = 0; i < currencyVals.Length; i++)
            {
                int currencyId = (int)currencyVals.GetValue(i);
                var curData = DataManager.Instance.GetCurrency(currencyId);
                if (curData == null) continue;

                GameStatCurrencyInfo currencyInfo = SerializeUtility.DeserializeObject<GameStatCurrencyInfo>(statsData.levelBasedCurrencyData[currencyId]);
                if (currencyInfo == null) continue;

                currencyInfo.finalValue = curData.Value;
                statsData.levelBasedCurrencyData[currencyId] = SerializeUtility.SerializeObject(currencyInfo);
            }

            PlayerPersistantData.SetGameStatsPlayerData(statsData);
        }

        public void GameCurrenciesData_MarkNewSession()
        {
            var statsData = PlayerPersistantData.GetGameStatsPlayerData();

            statsData.numberOfEarnActionsInSession = 0;
            statsData.numberOfFailedLevelsInSession = 0;
            statsData.numberOfPassedLevelsInSession = 0;
            statsData.lowestCoinBalanceDuringSession = DataManager.Instance.GetCurrency((int)CurrencyType.Coin).Value;
            statsData.sessionBasedCurrencyData.Clear();

            var currencyVals = Enum.GetValues(typeof(CurrencyType));
            for (int i = 0; i < currencyVals.Length; i++)
            {
                int currencyId = (int)currencyVals.GetValue(i);
                var curData = DataManager.Instance.GetCurrency(currencyId);
                if (curData == null) continue;

                GameStatCurrencyInfo currencyInfo = new GameStatCurrencyInfo();
                currencyInfo.currencyName = curData.currencyName;
                currencyInfo.startValue = curData.Value;

                if (statsData.sessionBasedCurrencyData.ContainsKey(currencyId))
                    statsData.sessionBasedCurrencyData[currencyId] = SerializeUtility.SerializeObject(currencyInfo);
                else
                    statsData.sessionBasedCurrencyData.Add(currencyId, SerializeUtility.SerializeObject(currencyInfo));
            }

            PlayerPersistantData.SetGameStatsPlayerData(statsData);
        }
        #endregion

        #region PRIVATE_METHODS
        private void InitializeGameStatesPlayerData()
        {
            var statsData = PlayerPersistantData.GetGameStatsPlayerData();
            if (statsData == null)
                statsData = GetDefaultPersistantData();

            bool isLastPlayedSessionTimeAvailable = statsData.lastPlayedSessionDate.TryParseDateTime(out DateTime lastPlayedSessionTime);
            var currentDateTime = TimeManager.Now;

            if (isLastPlayedSessionTimeAvailable)
            {
                if (lastPlayedSessionTime.Day != currentDateTime.Day)
                    statsData.totalNumberOfRetriesDoneInDay = 0;
            }

            lastPlayedSessionTimeString = statsData.lastPlayedSessionDate;
            statsData.lastPlayedSessionDate = TimeManager.Now.GetPlayerPrefsSaveString();
            PlayerPersistantData.SetGameStatsPlayerData(statsData);
        }

        private GameStatsPlayerPersistantData GetDefaultPersistantData()
        {
            var defaultGameData = new GameStatsPlayerPersistantData();
            return defaultGameData;
        }

        private DateTime GetCurrentDayDate()
        {
            if (isUseTstTime)
            {
                tstTime.TryParseDateTime(out var dt);
                return dt.Date;
            }

            return TimeManager.Now.Date;
        }
        #endregion

        #region EVENT_HANDLERS
        private void CoinCurrency_Change(int change)
        {
            bool isSave = false;
            var coinsData = DataManager.Instance.GetCurrency((int)CurrencyType.Coin);
            var statsData = PlayerPersistantData.GetGameStatsPlayerData();

            if (coinsData.Value < statsData.lowestCoinBalanceDuringLevel)
            {
                statsData.lowestCoinBalanceDuringLevel = coinsData.Value;
                isSave = true;
            }

            if (coinsData.Value < statsData.lowestCoinBalanceDuringSession)
            {
                statsData.lowestCoinBalanceDuringSession = coinsData.Value;
                isSave = true;
            }

            if (isSave)
                PlayerPersistantData.SetGameStatsPlayerData(statsData);
        }

        private void GameplayManager_onGameplayLevelStart()
        {
            var gameplayData = PlayerPersistantData.GetMainPlayerProgressData();
            var statsData = PlayerPersistantData.GetGameStatsPlayerData();
            if (statsData.coinBalanceStoredCurrentLevel != gameplayData.playerGameplayLevel)
            {
                statsData.lowestCoinBalanceDuringLevel = DataManager.Instance.GetCurrency((int)CurrencyType.Coin).Value;
                statsData.coinBalanceStoredCurrentLevel = gameplayData.playerGameplayLevel;
            }
            PlayerPersistantData.SetGameStatsPlayerData(statsData);
        }

        private void GameplayManager_onGameplayLevelOver()
        {
            WaitAFrameAndCall(OnGameplayOverStatsCollect);
        }

        private void OnGameplayOverStatsCollect()
        {
            currentPlayedLevelsInThisSession++;

            var statsData = PlayerPersistantData.GetGameStatsPlayerData();
            DateTime currentTime = GetCurrentDayDate();
            string currentTimeSaveKey = currentTime.GetPlayerPrefsSaveString();

            if (!statsData.lastDaysPlayedLevels.ContainsKey(currentTimeSaveKey))
                statsData.lastDaysPlayedLevels.Add(currentTimeSaveKey, 0);
            statsData.lastDaysPlayedLevels[currentTimeSaveKey]++;

            statsData.totalNumberOfRetriesDoneForLevel = 0;
            statsData.numberOfPassedLevelsInSession++;

            PlayerPersistantData.SetGameStatsPlayerData(statsData);
        }

        private void GameplayManager_onGameplayLevelReload()
        {
            var statsData = PlayerPersistantData.GetGameStatsPlayerData();
            statsData.totalNumberOfRetriesDoneInDay++;
            statsData.totalNumberOfRetriesDoneForLevel++;
            statsData.numberOfFailedLevelsInSession++;
            PlayerPersistantData.SetGameStatsPlayerData(statsData);
        }

        private void Instance_OnNewSessionStart()
        {
            AdjustManager.Instance.Adjust_GameSessionStart();
            GameCurrenciesData_MarkNewSession();
        }

        private void UpdateLastDaysPlayedLevel()
        {
            DateTime currentTime = GetCurrentDayDate();

            var statsData = PlayerPersistantData.GetGameStatsPlayerData();
            List<string> keysToRemove = new List<string>();

            foreach (var kvp in statsData.lastDaysPlayedLevels)
            {
                bool parseResult = kvp.Key.TryParseDateTime(out DateTime savedTime);
                if (string.IsNullOrEmpty(kvp.Key) || !parseResult || (currentTime.Date - savedTime).TotalDays >= Save_Data_Of_Past_X_Days)
                    keysToRemove.Add(kvp.Key);
            }

            keysToRemove.ForEach(x => statsData.lastDaysPlayedLevels.Remove(x));
            PlayerPersistantData.SetGameStatsPlayerData(statsData);
        }

        private void WaitAFrameAndCall(Action actionTocall)
        {
            StartCoroutine(WaitAFrameAndCallCoroutine(actionTocall));
        }
        #endregion

        #region COROUTINES
        IEnumerator WaitAFrameAndCallCoroutine(Action actionTocall)
        {
            yield return null;
            actionTocall?.Invoke();
        }
        #endregion

        #region UI_CALLBACKS
        [Button]
        public void Editor_OnLevelOver()
        {
            GameplayManager_onGameplayLevelOver();
        }

        [Button]
        public void Editor_PrintData()
        {
            var data = SerializeUtility.SerializeObject(PlayerPersistantData.GetGameStatsPlayerData());
            Debug.Log(data);
            GUIUtility.systemCopyBuffer = data;

            Debug.Log("AVG Levels : " + (GetAveragePlayedLevelsInPastDays()));
        }

        [Button]
        public void Editor_ClearData()
        {
            PlayerPersistantData.SetGameStatsPlayerData(null);
        }

        [Button]
        public void Editor_UpdateLastPlayedData()
        {
            UpdateLastDaysPlayedLevel();
        }
        #endregion
    }

    public enum GameCurrencyValueChangedReason
    {
        NONE,
        CURRENCY_SPENT,
        CURRENCY_EARNED_THROUGH_ADS_OR_IAP,
        CURRENCY_EARNED_THROUGH_SYSTEM,
    }

    public enum GameStatPopUpTriggerType
    {
        NONE,
        USER_TRIGGERED,
        SYSTEM_TRIGGERED
    }

    public class GameStatCurrencyInfo
    {
        [JsonProperty("cn")] public string currencyName;
        [JsonProperty("sv")] public int startValue;
        [JsonProperty("fern")] public int freeEarn;
        [JsonProperty("ern")] public int earn;
        [JsonProperty("spd")] public int spend;
        [JsonProperty("fv")] public int finalValue;
    }

    public class GameStatsPlayerPersistantData
    {
        [JsonProperty("ldpl")] public Dictionary<string, int> lastDaysPlayedLevels = new Dictionary<string, int>();

        // level based data
        [JsonProperty("tnordd")] public int totalNumberOfRetriesDoneInDay;
        [JsonProperty("tnordl")] public int totalNumberOfRetriesDoneForLevel;

        [JsonProperty("lcbdl")] public int lowestCoinBalanceDuringLevel;
        [JsonProperty("cbscl")] public int coinBalanceStoredCurrentLevel;

        [JsonProperty("lbcurd")] public Dictionary<int, string> levelBasedCurrencyData = new Dictionary<int, string>();

        [JsonProperty("noutp")] public int numberOfUserTriggeredPopups;
        [JsonProperty("noagp")] public int numberOfSystemTriggeredPopups;

        // session based data
        [JsonProperty("lcbdses")] public int lowestCoinBalanceDuringSession;
        [JsonProperty("lpsd")] public string lastPlayedSessionDate;

        [JsonProperty("sbcurd")] public Dictionary<int, string> sessionBasedCurrencyData = new Dictionary<int, string>();
        [JsonProperty("noeais")] public int numberOfEarnActionsInSession;

        [JsonProperty("noplis")] public int numberOfPassedLevelsInSession;
        [JsonProperty("noflis")] public int numberOfFailedLevelsInSession;
    }
}