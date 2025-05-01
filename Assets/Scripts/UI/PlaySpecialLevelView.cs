using I2.Loc;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort
{
    public class PlaySpecialLevelView : BaseView
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private LocalizationParamsManager specialLevelTextParam;
        private int levelNumber;
        private Action actionToCallOnPlayRejected;
        private Action<int> actionToCallOnPlayAcceped;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void Show(int levelNumber, Action<int> actionToCallOnPlayAcceped, Action actionToCallOnPlayRejected)
        {
            GameStatsCollector.Instance.OnPopUpTriggered(GameStatPopUpTriggerType.SYSTEM_TRIGGERED);
            this.levelNumber = levelNumber;
            this.actionToCallOnPlayRejected = actionToCallOnPlayRejected;
            this.actionToCallOnPlayAcceped = actionToCallOnPlayAcceped;

            specialLevelTextParam.SetParameterValue(specialLevelTextParam._Params[0].Name, levelNumber.ToString());
            Show();
        }

        public override void OnBackButtonPressed()
        {
            OnButtonClick_DontPlay();
        }
        #endregion

        #region PRIVATE_METHODS
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        public void OnButtonClick_Play()
        {
            Hide();
            actionToCallOnPlayAcceped?.Invoke(levelNumber);
            AnalyticsManager.Instance.LogSpecialLevelDataEvent(AnalyticsConstants.LevelData_StartTrigger, levelNumber);
        }

        public void OnButtonClick_DontPlay()
        {
            Hide();
            actionToCallOnPlayRejected?.Invoke();
            AnalyticsManager.Instance.LogSpecialLevelDataEvent(AnalyticsConstants.SpecialLevelData_SkipTrigger, levelNumber);
        }
        #endregion
    }
}