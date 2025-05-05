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
        #endregion

        #region PRIVATE_VARIABLES
        private const int Save_Data_Of_Past_X_Days = 7;
        [ShowInInspector, ReadOnly] private int currentPlayedLevelsInThisSession = 0;

        private string lastPlayedSessionTimeString;
        private GameStatsPlayerPersistantData statsData;
        #endregion

        #region PROPERTIES
        public int CurrentPlayedLevelsInThisSession => currentPlayedLevelsInThisSession;
        public string LastPlayedSessionTimeString => lastPlayedSessionTimeString;
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
            LevelManager.Instance.RegisterOnLevelReload(OnLevelReload);

            TrackingBridge.Instance.OnNewSessionStart += Instance_OnNewSessionStart;
        }

        private void OnDisable()
        {
            DataManager.Instance.GetCurrency(CurrencyConstant.COIN).RemoveOnCurrencyChangeEvent(CoinCurrency_Change);
            LevelManager.Instance.DeRegisterOnLevelLoad(OnLevelLoad);
            LevelManager.Instance.DeRegisterOnLevelComplete(OnLevelComplete);
            LevelManager.Instance.DeRegisterOnLevelReload(OnLevelReload);

            TrackingBridge.Instance.OnNewSessionStart -= Instance_OnNewSessionStart;
        }
        #endregion

        #region PUBLIC_METHODS
        public int GetAveragePlayedLevelsInPastDays()
        {
            if (statsData == null || statsData.lastDaysPlayedLevels.Count == 0)
                return 0;

            float avgLevels = 0;
            foreach (var kvp in statsData.lastDaysPlayedLevels)
            {
                avgLevels += kvp.Value;
            }

            return Mathf.CeilToInt(avgLevels / statsData.lastDaysPlayedLevels.Count);
        }

        public void OnGameCurrencyChanged(int currencyId, int currencyChange, CurrencyChangeReason changedReason = CurrencyChangeReason.SPENT)
        {
            Debug.Log($"Game Currency Changed : {currencyId} {currencyChange} {changedReason.ToString()}");

            if (changedReason == CurrencyChangeReason.EARNED_THROUGH_ADS_OR_IAP || changedReason == CurrencyChangeReason.EARNED_THROUGH_SYSTEM)
                statsData.numberOfEarnActionsInSession++;

            if (statsData.levelBasedCurrencyData.ContainsKey(currencyId))
            {
                var levelBasedCurrencyData = SerializeUtility.DeserializeObject<GameStatCurrencyInfo>(statsData.levelBasedCurrencyData[currencyId]);
                if (levelBasedCurrencyData != null)
                {
                    if (changedReason == CurrencyChangeReason.SPENT)
                        levelBasedCurrencyData.spend += currencyChange;
                    else if (changedReason == CurrencyChangeReason.EARNED_THROUGH_ADS_OR_IAP)
                        levelBasedCurrencyData.earn += currencyChange;
                    else if (changedReason == CurrencyChangeReason.EARNED_THROUGH_SYSTEM)
                        levelBasedCurrencyData.freeEarn += currencyChange;

                    statsData.levelBasedCurrencyData[currencyId] = SerializeUtility.SerializeObject(levelBasedCurrencyData);
                }
            }

            if (statsData.sessionBasedCurrencyData.ContainsKey(currencyId))
            {
                var sessionBasedCurrencyData = SerializeUtility.DeserializeObject<GameStatCurrencyInfo>(statsData.sessionBasedCurrencyData[currencyId]);
                if (sessionBasedCurrencyData != null)
                {
                    if (changedReason == CurrencyChangeReason.SPENT)
                        sessionBasedCurrencyData.spend += currencyChange;
                    else if (changedReason == CurrencyChangeReason.EARNED_THROUGH_ADS_OR_IAP)
                        sessionBasedCurrencyData.earn += currencyChange;
                    else if (changedReason == CurrencyChangeReason.EARNED_THROUGH_SYSTEM)
                        sessionBasedCurrencyData.freeEarn += currencyChange;

                    statsData.sessionBasedCurrencyData[currencyId] = SerializeUtility.SerializeObject(sessionBasedCurrencyData);
                }
            }
            SaveData();
        }

        public void OnPopUpTriggered(GameStatPopUpTriggerType gameStatPopUpTriggerType = GameStatPopUpTriggerType.USER_TRIGGERED)
        {
            if (gameStatPopUpTriggerType == GameStatPopUpTriggerType.USER_TRIGGERED)
                statsData.numberOfUserTriggeredPopups++;
            else if (gameStatPopUpTriggerType == GameStatPopUpTriggerType.SYSTEM_TRIGGERED)
                statsData.numberOfSystemTriggeredPopups++;

            SaveData();
        }

        public void GameCurrenciesData_MarkNewLevel()
        {
            statsData.numberOfUserTriggeredPopups = 0;
            statsData.numberOfSystemTriggeredPopups = 0;

            statsData.levelBasedCurrencyData.Clear();

            List<Currency> currencies = DataManager.Instance.GetAllCurrencies();
            for (int i = 0; i < currencies.Count; i++)
            {
                var curData = currencies[i];
                if (curData == null)
                    continue;
                GameStatCurrencyInfo currencyInfo = new GameStatCurrencyInfo();
                currencyInfo.currencyName = curData.currencyName;
                currencyInfo.startValue = curData.Value;
                if (statsData.levelBasedCurrencyData.ContainsKey(curData.CurrencyID))
                    statsData.levelBasedCurrencyData[curData.CurrencyID] = SerializeUtility.SerializeObject(currencyInfo);
                else
                    statsData.levelBasedCurrencyData.Add(curData.CurrencyID, SerializeUtility.SerializeObject(currencyInfo));
            }
            SaveData();
        }

        public void GameCurrenciesData_MarkLevelEnd()
        {
            List<Currency> currencies = DataManager.Instance.GetAllCurrencies();
            for (int i = 0; i < currencies.Count; i++)
            {
                var curData = currencies[i];
                if (curData == null)
                    continue;
                GameStatCurrencyInfo currencyInfo = SerializeUtility.DeserializeObject<GameStatCurrencyInfo>(statsData.levelBasedCurrencyData[curData.CurrencyID]);
                if (currencyInfo == null)
                    continue;
                currencyInfo.finalValue = curData.Value;
                statsData.levelBasedCurrencyData[curData.CurrencyID] = SerializeUtility.SerializeObject(currencyInfo);
            }
            SaveData();
        }

        public void GameCurrenciesData_MarkNewSession()
        {
            statsData.numberOfEarnActionsInSession = 0;
            statsData.numberOfFailedLevelsInSession = 0;
            statsData.numberOfPassedLevelsInSession = 0;
            statsData.lowestCoinBalanceDuringSession = DataManager.Instance.GetCurrency(CurrencyConstant.COIN).Value;
            statsData.sessionBasedCurrencyData.Clear();

            List<Currency> currencies = DataManager.Instance.GetAllCurrencies();
            for (int i = 0; i < currencies.Count; i++)
            {
                var curData = currencies[i];
                if (curData == null)
                    continue;
                GameStatCurrencyInfo currencyInfo = new GameStatCurrencyInfo();
                currencyInfo.currencyName = curData.currencyName;
                currencyInfo.startValue = curData.Value;
                if (statsData.sessionBasedCurrencyData.ContainsKey(curData.CurrencyID))
                    statsData.sessionBasedCurrencyData[curData.CurrencyID] = SerializeUtility.SerializeObject(currencyInfo);
                else
                    statsData.sessionBasedCurrencyData.Add(curData.CurrencyID, SerializeUtility.SerializeObject(currencyInfo));
            }
            SaveData();
        }

        public GameStatsPlayerPersistantData GetStatesData()
        {
            return statsData;
        }
        #endregion

        #region PRIVATE_METHODS
        private void LoadSaveData()
        {
            statsData = PlayerPersistantData.GetGameStatsPlayerData();
            if (statsData == null)
                statsData = new GameStatsPlayerPersistantData();
        }

        private void SaveData()
        {
            PlayerPersistantData.SetGameStatsPlayerData(statsData);
        }

        private void InitializeGameStatesPlayerData()
        {
            LoadSaveData();

            bool isLastPlayedSessionTimeAvailable = statsData.lastPlayedSessionDate.TryParseDateTime(out DateTime lastPlayedSessionTime);
            var currentDateTime = TimeManager.Now;

            if (isLastPlayedSessionTimeAvailable)
            {
                if (lastPlayedSessionTime.Day != currentDateTime.Day)
                    statsData.totalNumberOfRetriesDoneInDay = 0;
            }

            lastPlayedSessionTimeString = statsData.lastPlayedSessionDate;
            statsData.lastPlayedSessionDate = TimeManager.Now.GetPlayerPrefsSaveString();
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
                SaveData();
        }

        private void OnLevelLoad()
        {
            int level = DataManager.PlayerLevel;
            if (statsData.coinBalanceStoredCurrentLevel != level)
            {
                statsData.lowestCoinBalanceDuringLevel = DataManager.Instance.GetCurrency(CurrencyConstant.COIN).Value;
                statsData.coinBalanceStoredCurrentLevel = level;
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

            if (!statsData.lastDaysPlayedLevels.ContainsKey(currentTimeSaveKey))
                statsData.lastDaysPlayedLevels.Add(currentTimeSaveKey, 0);
            statsData.lastDaysPlayedLevels[currentTimeSaveKey]++;

            statsData.totalNumberOfRetriesDoneForLevel = 0;
            statsData.numberOfPassedLevelsInSession++;

            SaveData();
        }

        private void OnLevelReload()
        {
            statsData.totalNumberOfRetriesDoneInDay++;
            statsData.totalNumberOfRetriesDoneForLevel++;
            statsData.numberOfFailedLevelsInSession++;
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

            foreach (var kvp in statsData.lastDaysPlayedLevels)
            {
                bool parseResult = kvp.Key.TryParseDateTime(out DateTime savedTime);
                if (string.IsNullOrEmpty(kvp.Key) || !parseResult || (currentTime.Date - savedTime).TotalDays >= Save_Data_Of_Past_X_Days)
                    keysToRemove.Add(kvp.Key);
            }

            keysToRemove.ForEach(x => statsData.lastDaysPlayedLevels.Remove(x));
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
        [Button]
        public void Editor_OnLevelOver()
        {
            OnLevelComplete();
        }

        [Button]
        public void Editor_PrintData()
        {
            var data = SerializeUtility.SerializeObject(statsData);
            Debug.Log(data);
            GUIUtility.systemCopyBuffer = data;

            Debug.Log("AVG Levels : " + (GetAveragePlayedLevelsInPastDays()));
        }

        [Button]
        public void Editor_ClearData()
        {
            statsData = null;
            SaveData();
        }

        [Button]
        public void Editor_UpdateLastPlayedData()
        {
            UpdateLastDaysPlayedLevel();
        }
        #endregion
    }

    public enum CurrencyChangeReason
    {
        NONE,
        SPENT,
        EARNED_THROUGH_ADS_OR_IAP,
        EARNED_THROUGH_SYSTEM,
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