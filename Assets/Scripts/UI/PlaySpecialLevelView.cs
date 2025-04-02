using I2.Loc;
using System;
using UnityEngine;

namespace Tag.NutSort
{
    public class PlaySpecialLevelView : BaseView
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private LocalizationParamsManager specialLevelTextParam;
        private LevelData levelData;
        private Action actionToCallOnPlayRejected;
        private Action<LevelData> actionToCallOnPlayAcceped;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void Show(LevelData levelData, Action<LevelData> actionToCallOnPlayAcceped, Action actionToCallOnPlayRejected)
        {
            GameStatsCollector.Instance.OnPopUpTriggered(GameStatPopUpTriggerType.SYSTEM_TRIGGERED);

            this.levelData = levelData;
            this.actionToCallOnPlayRejected = actionToCallOnPlayRejected;
            this.actionToCallOnPlayAcceped = actionToCallOnPlayAcceped;

            specialLevelTextParam.SetParameterValue(specialLevelTextParam._Params[0].Name, levelData.level.ToString());
            Show();
        } 
        

        public override void OnBackButtonPressed()
        {
            OnButtonClick_DontPlay();
        }
        #endregion

        #region PRIVATE_METHODS
        public void LogSpecialLevelStartEvent()
        {
            AnalyticsManager.Instance.LogSpecialLevelDataEvent(AnalyticsConstants.LevelData_StartTrigger, levelData.level);
        }

        public void LogSpecialLevelSkipEvent()
        {
            AnalyticsManager.Instance.LogSpecialLevelDataEvent(AnalyticsConstants.SpecialLevelData_SkipTrigger, levelData.level);
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        public void OnButtonClick_Play()
        {
            Hide();
            actionToCallOnPlayAcceped?.Invoke(levelData);
            LogSpecialLevelStartEvent();
        }

        public void OnButtonClick_DontPlay()
        {
            actionToCallOnPlayRejected?.Invoke();
            Hide();

            LogSpecialLevelSkipEvent();
        }
        #endregion
    }
}