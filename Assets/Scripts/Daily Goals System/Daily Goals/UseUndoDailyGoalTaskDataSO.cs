using System.Collections;
using System.Collections.Generic;
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
            GameplayManager.onUndoBoosterUsed += GameplayManager_onUndoBoosterUsed;
        }
        public override void UnregisterDailyGoalEvents()
        {
            GameplayManager.onUndoBoosterUsed -= GameplayManager_onUndoBoosterUsed;
        }
        #endregion

        #region PRIVATE_METHODS
        private void GameplayManager_onUndoBoosterUsed()
        {
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