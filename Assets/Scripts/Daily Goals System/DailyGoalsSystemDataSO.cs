using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "DailyGoalsSystemDataSO", menuName = Constant.GAME_NAME + "/Daily Goals/DailyGoalsSystemDataSO")]
    public class DailyGoalsSystemDataSO : SerializedScriptableObject
    {
        #region PUBLIC_VARIABLES
        public int unlockAtLevel = 5;
        public int assignTasksCount = 3;
        public TimeDuration refreshTimeAtEveryDay; // 24 hours format for when all the tasks will be refreshed everyday

        [Space]
        public List<BaseDailyGoalTaskSystemDataSO> allDailyTasks;
        public List<DailyGoalPlayerData> fixedTaskPlayerData;

        [Space]
        public BaseReward allTaskCompleteReward;
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public BaseDailyGoalTaskSystemDataSO GetDailyGoalsTaskDataOf(DailyGoalsTaskType dailyGoalsTaskType)
        {
            return allDailyTasks.Find(x => x.dailyGoalsTaskType == dailyGoalsTaskType);
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

    public abstract class BaseDailyGoalTaskSystemDataSO : SerializedScriptableObject
    {
        public DailyGoalsTaskType dailyGoalsTaskType;
        public string taskDescriptionFormat;
        public List<DailyGoalLevelTargetInfo> dailyGoalLevelTargetInfos;

        public abstract string GetDailyGoalTaskTitle(DailyGoalPlayerData dailyGoalPlayerData);
        public virtual DailyGoalPlayerData OnAssignThisTask(int taskLevel)
        {
            var goalData = dailyGoalLevelTargetInfos.Find(x => x.dailyGoalLevel == taskLevel);

            DailyGoalPlayerData dailyGoalPlayerData = new DailyGoalPlayerData();
            dailyGoalPlayerData.dailyGoalsTaskType = dailyGoalsTaskType;
            dailyGoalPlayerData.dailyGoalTargetCount = goalData == null ? dailyGoalLevelTargetInfos.GetLastItemFromList().dailyGoalLevelTarget : goalData.dailyGoalLevelTarget;
            dailyGoalPlayerData.dailyGoalCurrentProgress = 0;
            dailyGoalPlayerData.dailyGoalExtraData = new Dictionary<string, string>();

            return dailyGoalPlayerData;
        }

        public bool DoesTaskLevelDataExist(int taskLevel)
        {
            return dailyGoalLevelTargetInfos.Find(x => x.dailyGoalLevel == taskLevel) != null;
        }
    }

    public class DailyGoalLevelTargetInfo
    {
        public int dailyGoalLevel;
        public int dailyGoalLevelTarget;
    }
}