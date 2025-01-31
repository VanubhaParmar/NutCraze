using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.tag.nut_sort {
    public class LeaderboardManager : SerializedManager<LeaderboardManager>
    {
        #region PUBLIC_VARIABLES

        public const int Max_Top_Rank = 3;
        public LeaderboardData LeaderboardData => leaderboardData;
        public SystemTimer LeaderboardRunTimer => leaderboardRunTimer;
        public int LeaderBoardEventRunTimeInDays => LeaderBoardRemoteConfigInfo.leaderboardRunTimeInDays;
        public bool IsSystemInitialized => isInitialized;
        public LeaderBoardRemoteConfigInfo LeaderBoardRemoteConfigInfo => myLeaderboardRCInfo;
        public LeaderBoardProgressTracker LeaderBoardProgressTracker => leaderBoardProgressTracker;

        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private LeaderboardData leaderboardData;
        private bool isInitialized;

        private SystemTimer leaderboardRunTimer;

        [ShowInInspector, ReadOnly] private List<BaseLeaderBoardPlayer> leaderBoardPlayers = new List<BaseLeaderBoardPlayer>();

        private const string Leaderboard_Player_Name = "You";

        [SerializeField] private LeaderboardDataRemoteConfig leaderboardDataRemoteConfig;
        [ShowInInspector, ReadOnly] private LeaderBoardRemoteConfigInfo myLeaderboardRCInfo;

        private LeaderBoardProgressTracker leaderBoardProgressTracker;
        private LeaderBoardPlayerPersistantData leaderBoardPlayerData;
        private int botTargetScore; // Cache bot target score
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        public override void Awake()
        {
            base.Awake();

            StartCoroutine(WaitForRCToLoad(() =>
            {
                Debug.Log("==>>> Initializing Leaderboard <<<==");
                SetLeaderboardRCData(leaderboardDataRemoteConfig.GetValue<LeaderBoardRemoteConfigInfo>());

                isInitialized = false;
                InitializeLeaderboardManager();
            }));

            OnLoadingDone();
        }

        private void OnEnable()
        {
            GameplayManager.onGameplayLevelOver += GameplayManager_onGameplayLevelOver;
            GameAnalyticsManager.onRCValuesFetched += GameAnalyticsManager_onRCValuesFetched;
        }

        private void OnDisable()
        {
            GameplayManager.onGameplayLevelOver -= GameplayManager_onGameplayLevelOver;
            GameAnalyticsManager.onRCValuesFetched -= GameAnalyticsManager_onRCValuesFetched;
        }
        #endregion

        #region PUBLIC_METHODS
        public bool IsLeaderboardUnlocked()
        {
            return PlayerPersistantData.GetMainPlayerProgressData().playerGameplayLevel >= LeaderBoardRemoteConfigInfo.startAtLevel;
        }

        public bool IsLeaderboardEventRunningAccordingToCalender()
        {
            DateTime currentTime = TimeManager.Now;
            return currentTime >= GetRecentLeaderboardEventStartTime() && currentTime <= GetRecentLeaderboardEventEndTime();
        }

        public bool IsCurrentLeaderboardEventActive()
        {
            if (leaderBoardPlayerData == null)
                return false;

            if (leaderBoardPlayerData.leaderboardStartTimeString.TryParseDateTime(out DateTime leaderboardStartTime))
                return (leaderboardStartTime - GetRecentLeaderboardEventStartTime()).TotalSeconds == 0f && IsLeaderboardEventRunningAccordingToCalender(); // true - event is runnning, false - event is over, give rewards and wait/start new event

            return false;
        }

        public bool IsLastEventResultReadyToShow()
        {
            if (leaderBoardPlayerData == null)
                return false;

            if (leaderBoardPlayerData.leaderboardStartTimeString.TryParseDateTime(out DateTime leaderboardStartTime))
                return !leaderBoardPlayerData.isEventResultShown;

            return false;
        }

        public bool CanOpenLeaderboardUI()
        {
            return IsCurrentLeaderboardEventActive() || IsLastEventResultReadyToShow();
        }

        public DateTime GetRecentLeaderboardEventStartTime()
        {
            DateTime currentDate = TimeManager.Now.Date;

            // Calculate days to subtract to reach the most recent start day
            int daysToSubtract = ((int)currentDate.DayOfWeek - (int)LeaderBoardRemoteConfigInfo.startDay + 7) % 7;

            // Get the most recent start date by subtracting the calculated days
            return currentDate.AddDays(-daysToSubtract);
        }

        public DateTime GetNextLeaderboardEventStartTime()
        {
            return GetRecentLeaderboardEventStartTime().AddDays(7);
        }

        public DateTime GetRecentLeaderboardEventEndTime()
        {
            return GetRecentLeaderboardEventStartTime().AddDays(LeaderBoardEventRunTimeInDays);
        }

        public List<LeaderBoardPlayerScoreInfoUIData> GetLeaderboardPlayerUIDatas()
        {
            List<LeaderBoardPlayerScoreInfoUIData> leaderBoardPlayerScoreInfoUIDatas = new List<LeaderBoardPlayerScoreInfoUIData>();

            leaderBoardPlayers.ForEach(x => leaderBoardPlayerScoreInfoUIDatas.Add(x.GetLeaderboardPlayerScoreInfoUIData()));
            leaderBoardPlayerScoreInfoUIDatas.Sort(CompareFunction);
            for (int i = 0; i < leaderBoardPlayerScoreInfoUIDatas.Count; i++)
            {
                leaderBoardPlayerScoreInfoUIDatas[i].rank = i + 1;
            }

            return leaderBoardPlayerScoreInfoUIDatas;
        }

        public int GetBotTargetScore()
        {
            //return PlayerPersistantData.GetLeaderboardPlayerData().playerTargetScore;
            return botTargetScore;
        }

        public int GetPlayerCurrentScore()
        {
            return leaderBoardPlayerData.playerScore;
        }

        public void OnLeaderboardViewVisited()
        {
            if (leaderBoardPlayerData == null)
                return;

            if (!IsCurrentLeaderboardEventActive() && !leaderBoardPlayerData.isEventResultShown)
            {
                int playerRank = GetPlayerRank();

                if (playerRank <= Max_Top_Rank)
                {
                    var rewardToGive = leaderboardData.GetRankReward(playerRank);
                    rewardToGive.GiveRewards();

                    var currencyReward = rewardToGive.rewards.Find(x => x.GetRewardType() == RewardType.Currency);
                    if (currencyReward != null)
                    {
                        GameStatsCollector.Instance.OnGameCurrencyChanged((int)CurrencyType.Coin, currencyReward.GetAmount(), GameCurrencyValueChangedReason.CURRENCY_EARNED_THROUGH_SYSTEM);
                        GameplayManager.Instance.LogCoinRewardFaucetEvent(AnalyticsConstants.ItemId_Leaderboard, currencyReward.GetAmount());
                    }

                    GameManager.RaiseOnRewardsClaimedUIRefresh();
                }

                leaderBoardPlayerData.isEventResultShown = true;
                SaveData();

                AnalyticsManager.Instance.LogLeaderboardRankEvent(playerRank);

                InitializeLeaderboardManager();
            }
        }

        public int GetPlayerRank()
        {
            return GetLeaderboardPlayerUIDatas().Find(x => x.leaderboardPlayerType == LeaderboardPlayerType.UserPlayer).rank;
        }

        public RewardsDataSO GetRankReward(int rank)
        {
            return leaderboardData.GetRankReward(rank);
        }

        public void SetLeaderboardRCData(LeaderBoardRemoteConfigInfo leaderBoardRemoteConfigInfo)
        {
            myLeaderboardRCInfo = leaderBoardRemoteConfigInfo;
        }
        #endregion

        #region PRIVATE_METHODS
        private void SetData()
        {
            leaderBoardPlayerData = PlayerPersistantData.GetLeaderboardPlayerData();
            if (leaderBoardPlayerData == null)
            {
                leaderBoardPlayerData = new LeaderBoardPlayerPersistantData();
                SaveData();
            }
        }

        private void SaveData()
        {
            if (leaderBoardPlayerData != null)
                PlayerPersistantData.SetLeaderboardPlayerData(leaderBoardPlayerData);
        }

        private int CalculateAndGetTargetLevel()
        {
            return Mathf.Max(leaderboardData.minimumTargetLevel, GameStatsCollector.Instance.GetAveragePlayedLevelsInPastDays()) * LeaderBoardEventRunTimeInDays;
        }

        private void AddAndUpdatePlayerScore(int score = 1)
        {
            leaderBoardPlayerData.playerScore += score;
            SaveData();
        }

        private int CompareFunction(LeaderBoardPlayerScoreInfoUIData a, LeaderBoardPlayerScoreInfoUIData b)
        {
            if (a.score > b.score) return -1;
            if (a.score < b.score) return 1;

            if (a.leaderboardPlayerType == LeaderboardPlayerType.UserPlayer) return -1;
            if (b.leaderboardPlayerType == LeaderboardPlayerType.UserPlayer) return 1;

            return 0;
        }

        private void InitializeLeaderboardPlayers()
        {
            leaderBoardPlayers.Clear();

            LeaderBoardUserPlayer leaderBoardUserPlayer = new LeaderBoardUserPlayer();
            leaderBoardUserPlayer.Init(Leaderboard_Player_Name, LeaderboardPlayerType.UserPlayer);
            leaderBoardPlayers.Add(leaderBoardUserPlayer);

            for (int i = 0; i < leaderBoardPlayerData.leaderBoardBotPlayerPersistantDatas.Count; i++)
            {
                LeaderBoardBotPlayer leaderBoardBotPlayer = new LeaderBoardBotPlayer();
                leaderBoardBotPlayer.Init(leaderboardData.botNamesList.GetDataAtIndex(leaderBoardPlayerData.leaderBoardBotPlayerPersistantDatas[i].botNameIndex));
                leaderBoardBotPlayer.InitSeedData(leaderBoardPlayerData.leaderBoardBotPlayerPersistantDatas[i].randomSeed, leaderBoardPlayerData.leaderBoardBotPlayerPersistantDatas[i].botScoreMultiplier);
                leaderBoardPlayers.Add(leaderBoardBotPlayer);
            }
        }

        private void InitializeLeaderboardManager()
        {
            if (!IsLeaderboardUnlocked())
                return;
            SetData();

            bool isStartNewEvent = IsLeaderboardEventRunningAccordingToCalender();

            // Start new event if last event is over and its result is shown or no event have been played at all
            if (leaderBoardPlayerData.leaderboardStartTimeString.TryParseDateTime(out DateTime leaderboardStartTime))
            {
                bool isLeaderboardRunning = (leaderboardStartTime - GetRecentLeaderboardEventStartTime()).TotalSeconds == 0f && !leaderBoardPlayerData.isEventResultShown; // true - event is runnning, false - event is over, give rewards and wait/start new event
                isStartNewEvent &= !isLeaderboardRunning && leaderBoardPlayerData.isEventResultShown;
            }

            if (isStartNewEvent)
            {
                Debug.Log("<= Starting New Leaderboard Event =>");
                StartNewLeaderboardEvent();
            }

            InitializeLeaderboardPlayers();
            InitializedLeaderboardTimer();
            InitializeLeaderboardProgressTracker();

            botTargetScore = leaderBoardPlayerData.playerTargetScore;
            isInitialized = true;

            RaiseOnLeaderboardEventStateChanged();
        }

        private void InitializeLeaderboardProgressTracker()
        {
            if (leaderBoardPlayers == null || leaderBoardPlayers.Count == 0)
                return;

            leaderBoardProgressTracker = new LeaderBoardProgressTracker();
        }

        private void StartNewLeaderboardEvent()
        {
            leaderBoardPlayerData.leaderboardStartTimeString = GetRecentLeaderboardEventStartTime().GetPlayerPrefsSaveString();
            leaderBoardPlayerData.playerScore = 0;
            leaderBoardPlayerData.playerTargetScore = CalculateAndGetTargetLevel();
            leaderBoardPlayerData.leaderBoardBotPlayerPersistantDatas = new List<LeaderBoardBotPlayerPersistantData>();
            leaderBoardPlayerData.isEventResultShown = false;

            List<string> randomBotNames = leaderboardData.botNamesList.GetRandomList(leaderboardData.numberOfTotalParticipants - 1);

            for (int i = 0; i < leaderboardData.numberOfTotalParticipants - 1; i++)
            {
                LeaderBoardBotPlayerPersistantData botData = new LeaderBoardBotPlayerPersistantData();
                botData.botNameIndex = leaderboardData.botNamesList.data.IndexOf(randomBotNames[i]);
                botData.botScoreMultiplier = leaderboardData.GetBotMultiplierAtIndex(i);
                botData.randomSeed = Utility.GetNewRandomSeed(leaderboardData.randomSeedRange);

                leaderBoardPlayerData.leaderBoardBotPlayerPersistantDatas.Add(botData);
            }
            SaveData();
        }

        private void InitializedLeaderboardTimer()
        {
            if (leaderboardRunTimer == null)
                leaderboardRunTimer = new SystemTimer();

            leaderboardRunTimer.ResetTimerObject();

            if (IsLeaderboardEventRunningAccordingToCalender())
                leaderboardRunTimer.StartSystemTimer(GetRecentLeaderboardEventEndTime(), GetRecentLeaderboardEventStartTime(), OnLeaderboardEventOver);
            else
                leaderboardRunTimer.StartSystemTimer(GetNextLeaderboardEventStartTime(), OnLeaderboardEventStart);
        }

        private void OnLeaderboardEventOver()
        {
            InitializeLeaderboardManager();
            RaiseOnLeaderboardEventRunTimerOver();
        }

        private void OnLeaderboardEventStart()
        {
            InitializeLeaderboardManager();
        }
        #endregion

        #region EVENT_HANDLERS
        public delegate void LeaderboardVoidEvent();
        public static event LeaderboardVoidEvent onLeaderboardEventStateChanged;
        public static void RaiseOnLeaderboardEventStateChanged()
        {
            onLeaderboardEventStateChanged?.Invoke();
        }

        public static event LeaderboardVoidEvent onLeaderboardEventRunTimerOver;
        public static void RaiseOnLeaderboardEventRunTimerOver()
        {
            onLeaderboardEventRunTimerOver?.Invoke();
        }

        private void GameplayManager_onGameplayLevelOver()
        {
            if (!isInitialized)
                InitializeLeaderboardManager();
            else if (IsCurrentLeaderboardEventActive())
                AddAndUpdatePlayerScore();

            if (leaderBoardProgressTracker != null)
                leaderBoardProgressTracker.OnUpdateCurrentLeaderboardPosition();
        }

        private void GameAnalyticsManager_onRCValuesFetched()
        {
            SetLeaderboardRCData(leaderboardDataRemoteConfig.GetValue<LeaderBoardRemoteConfigInfo>());
        }
        #endregion

        #region COROUTINES
        IEnumerator WaitForRCToLoad(Action actionToCall)
        {
            SetLeaderboardRCData(leaderboardDataRemoteConfig.GetValue<LeaderBoardRemoteConfigInfo>()); // set default values
            yield return new WaitUntil(() => GameAnalyticsManager.Instance.IsRCValuesFetched);
            actionToCall?.Invoke();
        }
        #endregion

        #region UI_CALLBACKS
        #endregion

        #region TEST_FUNCTIONS
        [Button]
        public void Editor_TestEventStartAndEndTime(DayOfWeek dayOfWeek)
        {
            DateTime currentDate = TimeManager.Now.Date;

            // Calculate days to subtract to reach the most recent start day
            int daysToSubtract = ((int)currentDate.DayOfWeek - (int)dayOfWeek + 7) % 7;

            // Get the most recent start date by subtracting the calculated days
            DateTime startDay = currentDate.AddDays(-daysToSubtract);
            DateTime endDay = startDay.AddDays(leaderboardData.leaderboardRunTimeInDays).AddSeconds(-1);

            DateTime nextEventStart = startDay.AddDays(7);

            Debug.Log("Start Time : " + startDay.GetPlayerPrefsSaveString());
            Debug.Log("End Time : " + endDay.GetPlayerPrefsSaveString());
            Debug.Log("Next Start Time : " + nextEventStart.GetPlayerPrefsSaveString());
        }

        [Button]
        public void Editor_ReInitializedLeaderboardData()
        {
            InitializeLeaderboardManager();
        }

        [Button]
        public void Editor_AddScore(int addScore)
        {
            AddAndUpdatePlayerScore(addScore);
        }

        [Button]
        public void Editor_SetScore(int setScore)
        {
            var data = PlayerPersistantData.GetLeaderboardPlayerData();
            data.playerScore = setScore;

            PlayerPersistantData.SetLeaderboardPlayerData(data);
        }
        #endregion
    }

    public class LeaderBoardProgressTracker
    {
        public int lastLeaderboardKnownPosition;
        public int currentLeaderboardKnownPosition;

        public LeaderBoardProgressTracker()
        {
            var datas = LeaderboardManager.Instance.GetLeaderboardPlayerUIDatas();

            currentLeaderboardKnownPosition = datas.Find(x => x.leaderboardPlayerType == LeaderboardPlayerType.UserPlayer).rank;
            lastLeaderboardKnownPosition = currentLeaderboardKnownPosition;
        }

        public void OnUpdateCurrentLeaderboardPosition()
        {
            var datas = LeaderboardManager.Instance.GetLeaderboardPlayerUIDatas();

            lastLeaderboardKnownPosition = currentLeaderboardKnownPosition;
            currentLeaderboardKnownPosition = datas.Find(x => x.leaderboardPlayerType == LeaderboardPlayerType.UserPlayer).rank;
        }

        public bool HasMadeProgress()
        {
            return lastLeaderboardKnownPosition > currentLeaderboardKnownPosition;
        }
    }

    public class LeaderBoardPlayerPersistantData
    {
        [JsonProperty("lbsts")] public string leaderboardStartTimeString;
        [JsonProperty("ps")] public int playerScore;
        [JsonProperty("pts")] public int playerTargetScore;
        [JsonProperty("iers")] public bool isEventResultShown;
        [JsonProperty("lbbppd")] public List<LeaderBoardBotPlayerPersistantData> leaderBoardBotPlayerPersistantDatas = new List<LeaderBoardBotPlayerPersistantData>();
    }

    public class LeaderBoardBotPlayerPersistantData
    {
        [JsonProperty("bni")] public int botNameIndex;
        [JsonProperty("rs")] public int randomSeed;
        [JsonProperty("bsm")] public float botScoreMultiplier;
    }
}