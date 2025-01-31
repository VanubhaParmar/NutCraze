using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.tag.nut_sort {
    public class DailyGoalsManager : SerializedManager<DailyGoalsManager>
    {
        #region PUBLIC_VARIABLES
        public DailyGoalsSystemDataSO DailyGoalsSystemDataSO => _dailyGoalsSystemDataSO;
        public SystemTimer DailyGoalsResetTimer => dailyGoalsResetTimer;
        public List<DailyGoalPlayerData> DailyGoals => _dailyGoalsPlayerPersistantData.dailyGoalPlayerDatas;
        public bool IsSytemInitialized => isSytemInitialized;
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private DailyGoalsSystemDataSO _dailyGoalsSystemDataSO;

        private DailyGoalsPlayerPersistantData _dailyGoalsPlayerPersistantData;
        private SystemTimer dailyGoalsResetTimer;
        private bool isSytemInitialized;
        #endregion

        #region PROPERTIES

        #endregion

        #region UNITY_CALLBACKS
        public override void Awake()
        {
            base.Awake();
            InitializeDailyGoalsSystem();
            OnLoadingDone();
        }

        private void OnEnable()
        {
            GameplayManager.onGameplayLevelOver += GameplayManager_onGameplayLevelOver;
        }

        private void OnDisable()
        {
            GameplayManager.onGameplayLevelOver -= GameplayManager_onGameplayLevelOver;
            RemoveDailySystemGoalsEvents();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            RemoveDailySystemGoalsEvents();
        }
        #endregion

        #region PUBLIC_METHODS
        public bool CanInitializeSystem()
        {
            return PlayerPersistantData.GetMainPlayerProgressData().playerGameplayLevel >= _dailyGoalsSystemDataSO.unlockAtLevel;
        }

        public void AddDailyGoalTaskProgress(DailyGoalsTaskType dailyGoalsTaskType, int progress)
        {
            var taskData = _dailyGoalsPlayerPersistantData.dailyGoalPlayerDatas.Find(x => x.dailyGoalsTaskType == dailyGoalsTaskType);
            if (taskData != null && !taskData.IsTaskCompleted())
            {
                int clampedProgress = Mathf.Clamp(taskData.dailyGoalCurrentProgress + progress, 0, taskData.dailyGoalTargetCount) - taskData.dailyGoalCurrentProgress;
                DailyGoalsProgressHelper.AddTaskProgress(dailyGoalsTaskType, clampedProgress);

                taskData.AddProgress(progress);

                CheckForAllTaskComplete();

                SavePlayerPersistantData();
            }
        }

        public void StopSystem()
        {
            if (dailyGoalsResetTimer != null)
                dailyGoalsResetTimer.StopSystemTimer();

            GameplayManager.onGameplayLevelOver -= GameplayManager_onGameplayLevelOver;
            RemoveDailySystemGoalsEvents();
        }

        public bool AreAllTaskCompleted()
        {
            return _dailyGoalsPlayerPersistantData.dailyGoalPlayerDatas.Find(x => !x.IsTaskCompleted()) == null;
        }

        public bool IsAllTaskCompleteRewardClaimed()
        {
            return _dailyGoalsPlayerPersistantData.isGoalsRewardsCollected;
        }

        public BaseReward GetAllTaskCompleteReward()
        {
            return DailyGoalsSystemDataSO.allTaskCompleteReward;
        }
        #endregion

        #region PRIVATE_METHODS
        private void CheckForAllTaskComplete()
        {
            if (AreAllTaskCompleted() && !IsAllTaskCompleteRewardClaimed())
            {
                _dailyGoalsSystemDataSO.allTaskCompleteReward.GiveReward();
                _dailyGoalsPlayerPersistantData.isGoalsRewardsCollected = true;
                DailyGoalsProgressHelper.SetAllTaskCompleted(true);

                if (_dailyGoalsSystemDataSO.allTaskCompleteReward.GetRewardType() == RewardType.Currency)
                {
                    GameStatsCollector.Instance.OnGameCurrencyChanged((int)CurrencyType.Coin, _dailyGoalsSystemDataSO.allTaskCompleteReward.GetAmount(), GameCurrencyValueChangedReason.CURRENCY_EARNED_THROUGH_SYSTEM);
                    GameplayManager.Instance.LogCoinRewardFaucetEvent(AnalyticsConstants.ItemId_DailyTaskReward, _dailyGoalsSystemDataSO.allTaskCompleteReward.GetAmount());
                }

                RaiseOnAllDailyGoalsCompleted();
            }
        }

        private void InitializeDailyGoalsSystem()
        {
            if (!CanInitializeSystem())
                return;

            LoadPlayerPersistantData();
            if (_dailyGoalsPlayerPersistantData == null)
                _dailyGoalsPlayerPersistantData = new DailyGoalsPlayerPersistantData();

            if (!_dailyGoalsPlayerPersistantData.lastSystemRefreshedTime.TryParseDateTime(out DateTime lastParsedTime) || (TimeManager.Now - lastParsedTime).TotalDays >= 1f)
                InitializeNewDailySystemGoals();

            LoadDailySystemGoalsEvents();
            StartDailyGoalsRefreshTimer();
            isSytemInitialized = true;
        }

        private void LoadDailySystemGoalsEvents()
        {
            if (!isSytemInitialized)
                _dailyGoalsSystemDataSO.allDailyTasks.ForEach(x => x.RegisterDailyGoalEvents());
        }

        private void RemoveDailySystemGoalsEvents()
        {
            _dailyGoalsSystemDataSO.allDailyTasks.ForEach(x => x.UnregisterDailyGoalEvents());
        }

        private void InitializeNewDailySystemGoals()
        {
            List<BaseDailyGoalTaskSystemDataSO> randomTasksList = new List<BaseDailyGoalTaskSystemDataSO>();
            randomTasksList.AddRange(_dailyGoalsSystemDataSO.allDailyTasks);
            randomTasksList.Shuffle();

            List<DailyGoalPlayerData> newDailyGoals = new List<DailyGoalPlayerData>();
            int totalAssignedTasks = _dailyGoalsPlayerPersistantData.totalTasksAssignedCount;

            for (int i = 0; i < _dailyGoalsSystemDataSO.assignTasksCount; i++)
            {
                DailyGoalPlayerData newDailyGoal;

                if (totalAssignedTasks < _dailyGoalsSystemDataSO.fixedTaskPlayerData.Count)
                    newDailyGoal = _dailyGoalsSystemDataSO.fixedTaskPlayerData[totalAssignedTasks].Clone();
                else
                {
                    BaseDailyGoalTaskSystemDataSO targetTaskDataToAssign = randomTasksList.PopRandomItemFromList();

                    var currentTaskLevelData = _dailyGoalsPlayerPersistantData.dailyGoalTaskLevels.Find(x => x.dailyGoalsTaskType == targetTaskDataToAssign.dailyGoalsTaskType);
                    if (currentTaskLevelData == null)
                    {
                        currentTaskLevelData = new DailyGoalTaskLevel(targetTaskDataToAssign.dailyGoalsTaskType, 1);
                        _dailyGoalsPlayerPersistantData.dailyGoalTaskLevels.Add(currentTaskLevelData);
                    }

                    newDailyGoal = targetTaskDataToAssign.OnAssignThisTask(currentTaskLevelData.dailyGoalsTaskLevel);

                    // Increase task level after assignment
                    if (targetTaskDataToAssign.DoesTaskLevelDataExist(currentTaskLevelData.dailyGoalsTaskLevel + 1))
                        currentTaskLevelData.dailyGoalsTaskLevel++;
                }

                newDailyGoals.Add(newDailyGoal);
                totalAssignedTasks++;
            }

            _dailyGoalsPlayerPersistantData.dailyGoalPlayerDatas = newDailyGoals;
            _dailyGoalsPlayerPersistantData.totalTasksAssignedCount = totalAssignedTasks;
            _dailyGoalsPlayerPersistantData.lastSystemRefreshedTime = TimeManager.Now.Date.AddTimeDuration(_dailyGoalsSystemDataSO.refreshTimeAtEveryDay).GetPlayerPrefsSaveString();

            SavePlayerPersistantData();
        }

        private void StartDailyGoalsRefreshTimer()
        {
            _dailyGoalsPlayerPersistantData.lastSystemRefreshedTime.TryParseDateTime(out DateTime lastParsedTime);

            if (dailyGoalsResetTimer != null)
                dailyGoalsResetTimer.ResetTimerObject();
            else
                dailyGoalsResetTimer = new SystemTimer();

            dailyGoalsResetTimer.StartSystemTimer(lastParsedTime.AddDays(1f), OnDailyGoalsRefreshTimeOver);
        }

        private void OnDailyGoalsRefreshTimeOver()
        {
            InitializeDailyGoalsSystem();
        }

        private void LoadPlayerPersistantData()
        {
            _dailyGoalsPlayerPersistantData = PlayerPersistantData.GetDailyGoalsPlayerData();
        }

        private void SavePlayerPersistantData()
        {
            PlayerPersistantData.SetDailyGoalsPlayerData(_dailyGoalsPlayerPersistantData);
        }
        #endregion

        #region EVENT_HANDLERS
        public delegate void OnDailyGoalVoidEvent();
        public static event OnDailyGoalVoidEvent onAllDailyGoalsCompleted;
        public static void RaiseOnAllDailyGoalsCompleted()
        {
            if (onAllDailyGoalsCompleted != null)
                onAllDailyGoalsCompleted();
        }

        private void GameplayManager_onGameplayLevelOver()
        {
            if (!isSytemInitialized && CanInitializeSystem())
                InitializeDailyGoalsSystem();
        }
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion

        #region EDITOR_FUNCTIONS
#if UNITY_EDITOR
        [Button]
        public void Editor_PrintPlayerData()
        {
            Debug.Log(SerializeUtility.SerializeObject(PlayerPersistantData.GetDailyGoalsPlayerData()));
        }

        [Button]
        public void Editor_ForceInitializeSystem()
        {
            InitializeDailyGoalsSystem();
        }

        [Button]
        public void Editor_ClearPlayerData()
        {
            PlayerPersistantData.SetDailyGoalsPlayerData(null);
        }

        [Button]
        public void Editor_ChangeLastTaskRefreshedTime(string taskRefreshTime)
        {
            var data = PlayerPersistantData.GetDailyGoalsPlayerData();
            data.lastSystemRefreshedTime = taskRefreshTime;
            PlayerPersistantData.SetDailyGoalsPlayerData(data);
        }
#endif
        #endregion
    }

    public class DailyGoalsPlayerPersistantData
    {
        [JsonProperty("lsrt")] public string lastSystemRefreshedTime;
        [JsonProperty("igrc")] public bool isGoalsRewardsCollected;
        [JsonProperty("ttac")] public int totalTasksAssignedCount;
        [JsonProperty("dgppd")] public List<DailyGoalPlayerData> dailyGoalPlayerDatas = new List<DailyGoalPlayerData>();
        [JsonProperty("dgtld")] public List<DailyGoalTaskLevel> dailyGoalTaskLevels = new List<DailyGoalTaskLevel>();
    }

    public class DailyGoalTaskLevel
    {
        [JsonProperty("dgtt")] public DailyGoalsTaskType dailyGoalsTaskType;
        [JsonProperty("dgtl")] public int dailyGoalsTaskLevel;

        public DailyGoalTaskLevel() { }
        public DailyGoalTaskLevel(DailyGoalsTaskType dailyGoalsTaskType, int dailyGoalsTaskLevel)
        {
            this.dailyGoalsTaskType = dailyGoalsTaskType;
            this.dailyGoalsTaskLevel = dailyGoalsTaskLevel;
        }
    }

    public class DailyGoalPlayerData
    {
        [JsonProperty("dgtt")] public DailyGoalsTaskType dailyGoalsTaskType;
        [JsonProperty("dgtc")] public int dailyGoalTargetCount;
        [JsonProperty("dgcp")] public int dailyGoalCurrentProgress;
        [JsonProperty("dged")] public Dictionary<string, string> dailyGoalExtraData = new Dictionary<string, string>(); // Extra Goals data would be parsed from this dictionary

        public string GetGoalDataOfKey(string dataKey)
        {
            if (dailyGoalExtraData.ContainsKey(dataKey))
                return dailyGoalExtraData[dataKey];

            return "";
        }

        public void AddToGoalDataKey(string dataKey, string data)
        {
            if (dailyGoalExtraData.ContainsKey(dataKey))
                dailyGoalExtraData[dataKey] = data;
            else
                dailyGoalExtraData.Add(dataKey, data);
        }

        public DailyGoalPlayerData Clone()
        {
            DailyGoalPlayerData clone = new DailyGoalPlayerData();
            clone.dailyGoalsTaskType = this.dailyGoalsTaskType;
            clone.dailyGoalTargetCount = this.dailyGoalTargetCount;
            clone.dailyGoalCurrentProgress = this.dailyGoalCurrentProgress;
            foreach (var kvp in this.dailyGoalExtraData)
            {
                clone.dailyGoalExtraData.Add(kvp.Key, kvp.Value);
            }

            return clone;
        }

        public void AddProgress(int progress)
        {
            dailyGoalCurrentProgress = Mathf.Clamp(dailyGoalCurrentProgress + progress, 0, dailyGoalTargetCount);
        }

        public bool IsTaskCompleted()
        {
            return dailyGoalCurrentProgress >= dailyGoalTargetCount;
        }
    }

    public class DailyGoalsPersistantDataKeys
    {
        public const string Collect_Nut_Goal_ColorType_Key = "CoNuCoTy";
    }

    public enum DailyGoalsTaskType
    {
        None = 0,
        Finish_Bolt_Goal = 1,
        Collect_Color_Nut_Goal = 2,
        Level_Wins_Goal = 3,
        Use_Undo_Goal = 4,
        Use_Extra_Bolt_Goal = 5,
    }
}