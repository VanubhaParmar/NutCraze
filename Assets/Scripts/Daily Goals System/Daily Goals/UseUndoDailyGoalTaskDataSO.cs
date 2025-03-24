using UnityEngine;

namespace Tag.NutSort
{
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
            return LocalizationHelper.GetTranslate(taskDescriptionFormat);
        }
        public override void RegisterDailyGoalEvents()
        {
            BoosterManager.RegisterOnBoosterUse(OnBoosterUse);
        }
        public override void UnregisterDailyGoalEvents()
        {
            BoosterManager.DeRegisterOnBoosterUse(OnBoosterUse);
        }
        #endregion

        #region PRIVATE_METHODS
        private void OnBoosterUse(int boosterId)
        {
            if (boosterId == BoosterIdConstant.UNDO)
                DailyGoalsManager.Instance.AddProgress(dailyGoalsTaskType, 1);
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