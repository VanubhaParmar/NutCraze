using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.tag.nut_sort {
    [CreateAssetMenu(fileName = "CollectColorNutsDailyGoalTaskDataSO", menuName = Constant.GAME_NAME + "/Daily Goals/Task/CollectColorNutsDailyGoalTaskDataSO")]
    public class CollectColorNutsDailyGoalTaskDataSO : BaseDailyGoalTaskSystemDataSO
    {
        #region PUBLIC_VARIABLES
        [NutColorId] public List<int> assignableColorIds;
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
            int assignedNutColorId = int.Parse(dailyGoalPlayerData.GetGoalDataOfKey(DailyGoalsPersistantDataKeys.Collect_Nut_Goal_ColorType_Key));

            var themeInfo = LevelManager.Instance.NutColorThemeTemplateDataSO.GetNutColorThemeInfoOfColor(assignedNutColorId);
            string colorName = $"<color=#{ColorUtility.ToHtmlStringRGBA(themeInfo._mainColor)}>" + themeInfo.colorName + "</color>";

            return string.Format(taskDescriptionFormat, colorName);
        }

        public override DailyGoalPlayerData OnAssignThisTask(int taskLevel)
        {
            var data = base.OnAssignThisTask(taskLevel);
            data.AddToGoalDataKey(DailyGoalsPersistantDataKeys.Collect_Nut_Goal_ColorType_Key, assignableColorIds.GetRandomItemFromList().ToString());
            return data;
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
            var taskData = DailyGoalsManager.Instance.DailyGoals.Find(x => x.dailyGoalsTaskType == dailyGoalsTaskType);
            if (taskData != null)
            {
                int targetColorId = int.Parse(taskData.GetGoalDataOfKey(DailyGoalsPersistantDataKeys.Collect_Nut_Goal_ColorType_Key));

                int targetNutsCount = GameplayManager.Instance.GameplayStateData.GetTotalNutCountOfColor(targetColorId);
                if (targetNutsCount > 0)
                    DailyGoalsManager.Instance.AddDailyGoalTaskProgress(dailyGoalsTaskType, targetNutsCount);
            }
        }
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}