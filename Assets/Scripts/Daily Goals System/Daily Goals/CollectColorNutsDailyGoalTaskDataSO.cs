using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
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

            return string.Format(taskDescriptionFormat, assignedNutColorId);
        }

        public override DailyGoalPlayerData OnAssignThisTask(int taskLevel)
        {
            var data = base.OnAssignThisTask(taskLevel);
            data.AddToGoalDataKey(DailyGoalsPersistantDataKeys.Collect_Nut_Goal_ColorType_Key, assignableColorIds.GetRandomItemFromList().ToString());
            return data;
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