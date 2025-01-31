using UnityEngine;

namespace com.tag.nut_sort {
    [CreateAssetMenu(fileName = "UseExtraBoltDailyGoalTaskDataSO", menuName = Constant.GAME_NAME + "/Daily Goals/Task/UseExtraBoltDailyGoalTaskDataSO")]
    public class UseExtraBoltDailyGoalTaskDataSO : BaseDailyGoalTaskSystemDataSO
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
            BoosterManager.Instance.RegisterOnBoosterUse(OnBoosterUser);
        }

        public override void UnregisterDailyGoalEvents()
        {
            BoosterManager.Instance.DeRegisterOnBoosterUse(OnBoosterUser);
        }
        #endregion

        #region PRIVATE_METHODS
        #endregion

        #region EVENT_HANDLERS
        private void OnBoosterUser(BoosterType booster)
        {
            if (booster == BoosterType.EXTRA_BOLT)
                DailyGoalsManager.Instance.AddDailyGoalTaskProgress(dailyGoalsTaskType, 1);
        }
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}