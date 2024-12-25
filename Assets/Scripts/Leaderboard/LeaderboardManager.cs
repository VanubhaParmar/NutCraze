using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Tag.NutSort
{
    public class LeaderboardManager : SerializedManager<LeaderboardManager>
    {
        #region PUBLIC_VARIABLES
        public const int Max_Top_Rank = 3;
        public LeaderboardData LeaderboardData => leaderboardData;
        public SystemTimer LeaderboardRunTimer => leaderboardRunTimer;
        public int LeaderBoardEventRunTimeInDays => LeaderBoardRemoteConfigInfo.leaderboardRunTimeInDays;
        public bool IsSystemInitialized => isInitialized;
        public LeaderBoardRemoteConfigInfo LeaderBoardRemoteConfigInfo => myLeaderboardRCInfo;
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private LeaderboardData leaderboardData;
        private bool isInitialized;

        private SystemTimer leaderboardRunTimer;

        [Space]
        [ShowInInspector, ReadOnly] private List<BaseLeaderBoardPlayer> leaderBoardPlayers = new List<BaseLeaderBoardPlayer>();

        private const string Leaderboard_Player_Name = "You";

        [SerializeField] private LeaderboardDataRemoteConfig leaderboardDataRemoteConfig;
        [ShowInInspector, ReadOnly] private LeaderBoardRemoteConfigInfo myLeaderboardRCInfo;
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
            DateTime currentTime = CustomTime.GetCurrentTime();
            return currentTime >= GetRecentLeaderboardEventStartTime() && currentTime <= GetRecentLeaderboardEventEndTime();
        }

        public bool IsCurrentLeaderboardEventActive()
        {
            var leaderBoardPlayerData = PlayerPersistantData.GetLeaderboardPlayerData();
            if (leaderBoardPlayerData == null)
                return false;

            if (!string.IsNullOrEmpty(leaderBoardPlayerData.leaderboardStartTimeString) && CustomTime.TryParseDateTime(leaderBoardPlayerData.leaderboardStartTimeString, out DateTime leaderboardStartTime))
                return (leaderboardStartTime - GetRecentLeaderboardEventStartTime()).TotalSeconds == 0f && IsLeaderboardEventRunningAccordingToCalender(); // true - event is runnning, false - event is over, give rewards and wait/start new event

            return false;
        }

        public bool IsLastEventResultReadyToShow()
        {
            var leaderBoardPlayerData = PlayerPersistantData.GetLeaderboardPlayerData();
            if (leaderBoardPlayerData == null) return false;

            if (!string.IsNullOrEmpty(leaderBoardPlayerData.leaderboardStartTimeString) && CustomTime.TryParseDateTime(leaderBoardPlayerData.leaderboardStartTimeString, out DateTime leaderboardStartTime))
                return !leaderBoardPlayerData.isEventResultShown;

            return false;
        }

        public bool CanOpenLeaderboardUI()
        {
            return IsCurrentLeaderboardEventActive() || IsLastEventResultReadyToShow();
        }

        public DateTime GetRecentLeaderboardEventStartTime()
        {
            DateTime currentDate = CustomTime.GetCurrentTime().Date;
            
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
            return PlayerPersistantData.GetLeaderboardPlayerData().playerTargetScore;
        }

        public int GetPlayerCurrentScore()
        {
            return PlayerPersistantData.GetLeaderboardPlayerData().playerScore;
        }

        public void OnLeaderboardViewVisited()
        {
            var leaderBoardPlayerData = PlayerPersistantData.GetLeaderboardPlayerData();
            if (leaderBoardPlayerData == null) return;

            if (!IsCurrentLeaderboardEventActive() && !leaderBoardPlayerData.isEventResultShown)
            {
                int playerRank = GetPlayerRank();

                if (playerRank <= Max_Top_Rank)
                {
                    var rewardToGive = leaderboardData.GetRankReward(playerRank);
                    rewardToGive.GiveRewards();

                    var currencyReward = rewardToGive.rewards.Find(x => x.GetRewardType() == RewardType.Currency);
                    if (currencyReward != null)
                        GameplayManager.Instance.LogCoinRewardFaucetEvent(AnalyticsConstants.ItemId_Leaderboard, currencyReward.GetAmount());

                    GameManager.RaiseOnRewardsClaimedUIRefresh();
                }

                leaderBoardPlayerData.isEventResultShown = true;
                PlayerPersistantData.SetLeaderboardPlayerData(leaderBoardPlayerData);

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
        private int CalculateAndGetTargetLevel()
        {
            return Mathf.Max(leaderboardData.minimumTargetLevel, GameStatsCollector.Instance.GetAveragePlayedLevelsInPastDays());
        }

        private void AddAndUpdatePlayerScore(int score = 1)
        {
            var data = PlayerPersistantData.GetLeaderboardPlayerData();
            data.playerScore += score;

            PlayerPersistantData.SetLeaderboardPlayerData(data);
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
            var leaderBoardPlayerData = PlayerPersistantData.GetLeaderboardPlayerData();
            if (leaderBoardPlayerData == null)
                return;

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

            var leaderBoardPlayerData = PlayerPersistantData.GetLeaderboardPlayerData();
            if (leaderBoardPlayerData == null)
                leaderBoardPlayerData = new LeaderBoardPlayerPersistantData();

            bool isStartNewEvent = IsLeaderboardEventRunningAccordingToCalender();

            // Start new event if last event is over and its result is shown or no event have been played at all
            if (!string.IsNullOrEmpty(leaderBoardPlayerData.leaderboardStartTimeString) && CustomTime.TryParseDateTime(leaderBoardPlayerData.leaderboardStartTimeString, out DateTime leaderboardStartTime))
            {
                bool isLeaderboardRunning = (leaderboardStartTime - GetRecentLeaderboardEventStartTime()).TotalSeconds == 0f && !leaderBoardPlayerData.isEventResultShown; // true - event is runnning, false - event is over, give rewards and wait/start new event
                isStartNewEvent &= !isLeaderboardRunning && leaderBoardPlayerData.isEventResultShown;
            }

            if (isStartNewEvent)
            {
                Debug.Log("<= Starting New Leaderboard Event =>");
                StartNewLeaderboardEvent(leaderBoardPlayerData);
                PlayerPersistantData.SetLeaderboardPlayerData(leaderBoardPlayerData);
            }

            InitializeLeaderboardPlayers();
            InitializedLeaderboardTimer();
            isInitialized = true;

            RaiseOnLeaderboardEventStateChanged();
        }

        private void StartNewLeaderboardEvent(LeaderBoardPlayerPersistantData leaderBoardPlayerData)
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
                botData.botScoreMultiplier = (float)Math.Round(Random.Range(0.2f, 1.2f), 2);
                botData.randomSeed = Utility.GetNewRandomSeed(leaderboardData.randomSeedRange);

                leaderBoardPlayerData.leaderBoardBotPlayerPersistantDatas.Add(botData);
            }
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
        }

        private void GameAnalyticsManager_onRCValuesFetched()
        {
            SetLeaderboardRCData(leaderboardDataRemoteConfig.GetValue<LeaderBoardRemoteConfigInfo>());
        }
        #endregion

        #region COROUTINES
        IEnumerator WaitForRCToLoad(Action actionToCall)
        {
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
            DateTime currentDate = CustomTime.GetCurrentTime().Date;

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
        public void Editor_PrintData()
        {
            Debug.Log(SerializeUtility.SerializeObject(PlayerPersistantData.GetLeaderboardPlayerData()));
        }

        [Button]
        public void Editor_ClearLeaderboardData()
        {
            PlayerPersistantData.SetLeaderboardPlayerData(null);
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