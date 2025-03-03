using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "LevelWinsDailyGoalTaskDataSO", menuName = Constant.GAME_NAME + "/Daily Goals/Task/LevelWinsDailyGoalTaskDataSO")]
    public class LevelWinsDailyGoalTaskDataSO : BaseDailyGoalTaskSystemDataSO
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
            LevelManager.Instance.RegisterOnLevelComplete(OnLevelComplete);
        }

        public override void UnregisterDailyGoalEvents()
        {
            LevelManager.Instance.DeRegisterOnLevelComplete(OnLevelComplete);
        }
        #endregion

        #region PRIVATE_METHODS
        #endregion

        #region EVENT_HANDLERS
        private void OnLevelComplete()
        {
            DailyGoalsManager.Instance.AddProgress(dailyGoalsTaskType, 1);
        }
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}