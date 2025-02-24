using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
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
            return LocalizationHelper.GetTranslate(taskDescriptionFormat);
        }
        public override void RegisterDailyGoalEvents()
        {
            GameplayManager.onExtraScrewBoosterUsed += GameplayManager_onExtraScrewBoosterUsed;
        }

        public override void UnregisterDailyGoalEvents()
        {
            GameplayManager.onExtraScrewBoosterUsed -= GameplayManager_onExtraScrewBoosterUsed;
        }
        #endregion

        #region PRIVATE_METHODS
        #endregion

        #region EVENT_HANDLERS
        private void GameplayManager_onExtraScrewBoosterUsed()
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