using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.tag.nut_sort {
    [CreateAssetMenu(fileName = "UseUndoDailyGoalTaskDataSO", menuName = Constant.GAME_NAME + "/Daily Goals/Task/UseUndoDailyGoalTaskDataSO")]
    public class UseUndoDailyGoalTaskDataSO : BaseDailyGoalTaskSystemDataSO
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public override string GetDailyGoalTaskTitle(DailyGoalPlayerData dailyGoalPlayerData)
        {
            return taskDescriptionFormat;
        }
        public override void RegisterDailyGoalEvents()
        {
            BoosterManager.Instance.RegisterOnBoosterUse(OnBoosterUse);
        }
        public override void UnregisterDailyGoalEvents()
        {
            BoosterManager.Instance.DeRegisterOnBoosterUse(OnBoosterUse);
        }
        #endregion

        #region PRIVATE_METHODS
        private void OnBoosterUse(BoosterType booster)
        {
            if (booster == BoosterType.UNDO)
                DailyGoalsManager.Instance.AddDailyGoalTaskProgress(dailyGoalsTaskType, 1);
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}