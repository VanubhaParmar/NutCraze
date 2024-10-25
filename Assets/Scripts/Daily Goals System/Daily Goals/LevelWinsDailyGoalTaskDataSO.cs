using System.Collections;
using System.Collections.Generic;
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
            return taskDescriptionFormat;
        }
        public override void RegisterDailyGoalEvents()
        {
            GameplayManager.onGameplayLevelOver += GameplayManager_onGameplayLevelOver;
        }

        public override void UnregisterDailyGoalEvents()
        {
            GameplayManager.onGameplayLevelOver -= GameplayManager_onGameplayLevelOver;
        }
        #endregion

        #region PRIVATE_METHODS
        #endregion

        #region EVENT_HANDLERS
        private void GameplayManager_onGameplayLevelOver()
        {
            DailyGoalsManager.Instance.AddDailyGoalTaskProgress(dailyGoalsTaskType, 1);
        }
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}