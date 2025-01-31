using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.tag.nut_sort {
    public static class DailyGoalsProgressHelper
    {
        #region PUBLIC_VARIABLES
        public static bool AreAllTaskCompleted => areAllTaskCompleted;
        #endregion

        #region PRIVATE_VARIABLES
        private static Dictionary<DailyGoalsTaskType, int> taskProgress = new Dictionary<DailyGoalsTaskType, int>();
        private static bool areAllTaskCompleted;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public static bool IsAnyProgress()
        {
            return taskProgress.Count > 0;
        }

        public static void AddTaskProgress(DailyGoalsTaskType dailyGoalsTaskType, int progress)
        {
            if (taskProgress.ContainsKey(dailyGoalsTaskType))
                taskProgress[dailyGoalsTaskType] = progress;
            else
                taskProgress.Add(dailyGoalsTaskType, progress);
        }

        public static int GetTaskProgress(DailyGoalsTaskType dailyGoalsTaskType)
        {
            if (taskProgress.ContainsKey(dailyGoalsTaskType))
                return taskProgress[dailyGoalsTaskType];
            return 0;
        }

        public static void ResetProgress()
        {
            SetAllTaskCompleted(false);
            taskProgress.Clear();
        }

        public static void SetAllTaskCompleted(bool state)
        {
            areAllTaskCompleted = state;
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
}