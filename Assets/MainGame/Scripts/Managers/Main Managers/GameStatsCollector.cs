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
        private GameStatsPlayerPersistantData gameStatsPlayerData;
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
            DataManager.Instance.GetCurrency(CurrencyConstant.COIN).RegisterOnCurrencyChangeEvent(CoinCurrency_Change);
            LevelManager.Instance.RegisterOnLevelLoad(OnLevelLoad);
            LevelManager.Instance.RegisterOnLevelComplete(OnLevelComplete);

            TrackingBridge.Instance.OnNewSessionStart += Instance_OnNewSessionStart;
        }

        private void OnDisable()
        {
            DataManager.Instance.GetCurrency(CurrencyConstant.COIN).RemoveOnCurrencyChangeEvent(CoinCurrency_Change);
            LevelManager.Instance.DeRegisterOnLevelLoad(OnLevelLoad);
            LevelManager.Instance.DeRegisterOnLevelComplete(OnLevelComplete);

            TrackingBridge.Instance.OnNewSessionStart -= Instance_OnNewSessionStart;
        }
        #endregion

        #region PUBLIC_METHODS
        public int GetAveragePlayedLevelsInPastDays()
        {
            if (gameStatsPlayerData == null || gameStatsPlayerData.lastDaysPlayedLevels.Count == 0)
                return 0;

            float avgLevels = 0;
            foreach (var kvp in gameStatsPlayerData.lastDaysPlayedLevels)
            {
                avgLevels += kvp.Value;
            }

            return Mathf.CeilToInt(avgLevels / gameStatsPlayerData.lastDaysPlayedLevels.Count);
        }

        public void OnGameCurrencyChanged(int currencyId, int currencyChange, GameCurrencyValueChangedReason gameCurrencyValueChangedReason = GameCurrencyValueChangedReason.CURRENCY_SPENT)
        {
            Debug.Log($"Game Currency Changed : {currencyId} {currencyChange} {gameCurrencyValueChangedReason.ToString()}");

            if (gameCurrencyValueChangedReason == GameCurrencyValueChangedReason.CURRENCY_EARNED_THROUGH_ADS_OR_IAP || gameCurrencyValueChangedReason == GameCurrencyValueChangedReason.CURRENCY_EARNED_THROUGH_SYSTEM)
                gameStatsPlayerData.numberOfEarnActionsInSession++;

            if (gameStatsPlayerData.levelBasedCurrencyData.ContainsKey(currencyId))
            {
                var levelBasedCurrencyData = SerializeUtility.DeserializeObject<GameStatCurrencyInfo>(gameStatsPlayerData.levelBasedCurrencyData[currencyId]);
                if (levelBasedCurrencyData != null)
                {
                    if (gameCurrencyValueChangedReason == GameCurrencyValueChangedReason.CURRENCY_SPENT)
                        levelBasedCurrencyData.spend += currencyChange;
                    else if (gameCurrencyValueChangedReason == GameCurrencyValueChangedReason.CURRENCY_EARNED_THROUGH_ADS_OR_IAP)
                        levelBasedCurrencyData.earn += currencyChange;
                    else if (gameCurrencyValueChangedReason == GameCurrencyValueChangedReason.CURRENCY_EARNED_THROUGH_SYSTEM)
                        levelBasedCurrencyData.freeEarn += currencyChange;

                    gameStatsPlayerData.levelBasedCurrencyData[currencyId] = SerializeUtility.SerializeObject(levelBasedCurrencyData);
                }
            }

            if (gameStatsPlayerData.sessionBasedCurrencyData.ContainsKey(currencyId))
            {
                var sessionBasedCurrencyData = SerializeUtility.DeserializeObject<GameStatCurrencyInfo>(gameStatsPlayerData.sessionBasedCurrencyData[currencyId]);
                if (sessionBasedCurrencyData != null)
                {
                    if (gameCurrencyValueChangedReason == GameCurrencyValueChangedReason.CURRENCY_SPENT)
                        sessionBasedCurrencyData.spend += currencyChange;
                    else if (gameCurrencyValueChangedReason == GameCurrencyValueChangedReason.CURRENCY_EARNED_THROUGH_ADS_OR_IAP)
                        sessionBasedCurrencyData.earn += currencyChange;
                    else if (gameCurrencyValueChangedReason == GameCurrencyValueChangedReason.CURRENCY_EARNED_THROUGH_SYSTEM)
                        sessionBasedCurrencyData.freeEarn += currencyChange;

                    gameStatsPlayerData.sessionBasedCurrencyData[currencyId] = SerializeUtility.SerializeObject(sessionBasedCurrencyData);
                }
            }
            SaveData();
        }

        public void OnPopUpTriggered(GameStatPopUpTriggerType gameStatPopUpTriggerType = GameStatPopUpTriggerType.USER_TRIGGERED)
        {
            if (gameStatPopUpTriggerType == GameStatPopUpTriggerType.USER_TRIGGERED)
                gameStatsPlayerData.numberOfUserTriggeredPopups++;
            else if (gameStatPopUpTriggerType == GameStatPopUpTriggerType.SYSTEM_TRIGGERED)
                gameStatsPlayerData.numberOfSystemTriggeredPopups++;
            SaveData();
        }

        public void GameCurrenciesData_MarkNewLevel()
        {
            gameStatsPlayerData.numberOfUserTriggeredPopups = 0;
            gameStatsPlayerData.numberOfSystemTriggeredPopups = 0;
            gameStatsPlayerData.levelBasedCurrencyData.Clear();
            Dictionary<int, Currency> currencies = DataManager.Instance.GetAllCurrency();
            foreach (KeyValuePair<int, Currency> item in currencies)
            {
                var currency = item.Value;
                int currencyId = item.Key;
                if (currency == null) continue;

                GameStatCurrencyInfo currencyInfo = new GameStatCurrencyInfo();
                currencyInfo.currencyName = currency.currencyName;
                currencyInfo.startValue = currency.Value;

                if (gameStatsPlayerData.levelBasedCurrencyData.ContainsKey(currencyId))
                    gameStatsPlayerData.levelBasedCurrencyData[currencyId] = SerializeUtility.SerializeObject(currencyInfo);
                else
                    gameStatsPlayerData.levelBasedCurrencyData.Add(currencyId, SerializeUtility.SerializeObject(currencyInfo));
            }
            SaveData();
        }

        public void GameCurrenciesData_MarkLevelEnd()
        {
            Dictionary<int, Currency> currencies = DataManager.Instance.GetAllCurrency();
            foreach (KeyValuePair<int, Currency> item in currencies)
            {
                var currency = item.Value;
                int currencyId = item.Key;
                if (currency == null) continue;

                GameStatCurrencyInfo currencyInfo = SerializeUtility.DeserializeObject<GameStatCurrencyInfo>(gameStatsPlayerData.levelBasedCurrencyData[currencyId]);
                if (currencyInfo == null) continue;

                currencyInfo.finalValue = currency.Value;
                gameStatsPlayerData.levelBasedCurrencyData[currencyId] = SerializeUtility.SerializeObject(currencyInfo);
            }
            SaveData();
        }

        public void GameCurrenciesData_MarkNewSession()
        {
            gameStatsPlayerData.numberOfEarnActionsInSession = 0;
            gameStatsPlayerData.numberOfFailedLevelsInSession = 0;
            gameStatsPlayerData.numberOfPassedLevelsInSession = 0;
            gameStatsPlayerData.lowestCoinBalanceDuringSession = DataManager.Instance.GetCurrency(CurrencyConstant.COIN).Value;
            gameStatsPlayerData.sessionBasedCurrencyData.Clear();



            Dictionary<int, Currency> currencies = DataManager.Instance.GetAllCurrency();
            foreach (KeyValuePair<int, Currency> item in currencies)
            {
                var currency = item.Value;
                int currencyId = item.Key;
                if (currency == null) continue;

                GameStatCurrencyInfo currencyInfo = new GameStatCurrencyInfo();
                currencyInfo.currencyName = currency.currencyName;
                currencyInfo.startValue = currency.Value;

                if (gameStatsPlayerData.sessionBasedCurrencyData.ContainsKey(currencyId))
                    gameStatsPlayerData.sessionBasedCurrencyData[currencyId] = SerializeUtility.SerializeObject(currencyInfo);
                else
                    gameStatsPlayerData.sessionBasedCurrencyData.Add(currencyId, SerializeUtility.SerializeObject(currencyInfo));
            }
            SaveData();
        }

        public GameStatsPlayerPersistantData GetGameStateData()
        {
            return gameStatsPlayerData;
        }
        #endregion

        #region PRIVATE_METHODS
        private void SaveData()
        {
            DataManager.Instance.SaveGameStatsPlayerData(gameStatsPlayerData);
        }

        private void InitializeGameStatesPlayerData()
        {
            gameStatsPlayerData = DataManager.Instance.GetGameStatsPlayerData();
            if (gameStatsPlayerData == null)
                gameStatsPlayerData = new GameStatsPlayerPersistantData();

            bool isLastPlayedSessionTimeAvailable = gameStatsPlayerData.lastPlayedSessionDate.TryParseDateTime(out DateTime lastPlayedSessionTime);
            var currentDateTime = TimeManager.Now;

            if (isLastPlayedSessionTimeAvailable)
            {
                if (lastPlayedSessionTime.Day != currentDateTime.Day)
                    gameStatsPlayerData.totalNumberOfRetriesDoneInDay = 0;
            }

            lastPlayedSessionTimeString = gameStatsPlayerData.lastPlayedSessionDate;
            gameStatsPlayerData.lastPlayedSessionDate = TimeManager.Now.GetPlayerPrefsSaveString();
            SaveData();
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
            var coinsData = DataManager.Instance.GetCurrency(CurrencyConstant.COIN);

            if (coinsData.Value < gameStatsPlayerData.lowestCoinBalanceDuringLevel)
            {
                gameStatsPlayerData.lowestCoinBalanceDuringLevel = coinsData.Value;
                isSave = true;
            }

            if (coinsData.Value < gameStatsPlayerData.lowestCoinBalanceDuringSession)
            {
                gameStatsPlayerData.lowestCoinBalanceDuringSession = coinsData.Value;
                isSave = true;
            }

            if (isSave)
                SaveData();
        }

        private void OnLevelLoad()
        {
            int level = DataManager.PlayerLevel.Value;
            if (gameStatsPlayerData.coinBalanceStoredCurrentLevel != level)
            {
                gameStatsPlayerData.lowestCoinBalanceDuringLevel = DataManager.Instance.GetCurrency(CurrencyConstant.COIN).Value;
                gameStatsPlayerData.coinBalanceStoredCurrentLevel = level;
            }
            SaveData();
        }

        private void OnLevelComplete()
        {
            WaitAFrameAndCall(OnGameplayOverStatsCollect);
        }

        private void OnGameplayOverStatsCollect()
        {
            currentPlayedLevelsInThisSession++;

            DateTime currentTime = GetCurrentDayDate();
            string currentTimeSaveKey = currentTime.GetPlayerPrefsSaveString();

            if (!gameStatsPlayerData.lastDaysPlayedLevels.ContainsKey(currentTimeSaveKey))
                gameStatsPlayerData.lastDaysPlayedLevels.Add(currentTimeSaveKey, 0);
            gameStatsPlayerData.lastDaysPlayedLevels[currentTimeSaveKey]++;

            gameStatsPlayerData.totalNumberOfRetriesDoneForLevel = 0;
            gameStatsPlayerData.numberOfPassedLevelsInSession++;

            SaveData();
        }

        private void Instance_OnNewSessionStart()
        {
            AdjustManager.Instance.Adjust_GameSessionStart();
            GameCurrenciesData_MarkNewSession();
        }

        private void UpdateLastDaysPlayedLevel()
        {
            DateTime currentTime = GetCurrentDayDate();

            List<string> keysToRemove = new List<string>();

            foreach (var kvp in gameStatsPlayerData.lastDaysPlayedLevels)
            {
                bool parseResult = kvp.Key.TryParseDateTime(out DateTime savedTime);
                if (string.IsNullOrEmpty(kvp.Key) || !parseResult || (currentTime.Date - savedTime).TotalDays >= Save_Data_Of_Past_X_Days)
                    keysToRemove.Add(kvp.Key);
            }

            keysToRemove.ForEach(x => gameStatsPlayerData.lastDaysPlayedLevels.Remove(x));
            SaveData();
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