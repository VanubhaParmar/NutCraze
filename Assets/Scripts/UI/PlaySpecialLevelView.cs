using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort
{
    public class PlaySpecialLevelView : BaseView
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private Text specialLevelText;
        private int showSpecialLevelPopup;
        private Action actionToCallOnPlayRejected;
        private Action actionToCallOnPlayAcceped;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void Show(int levelNumber, Action actionToCallOnPlayAcceped, Action actionToCallOnPlayRejected)
        {
            showSpecialLevelPopup = levelNumber;
            this.actionToCallOnPlayRejected = actionToCallOnPlayRejected;
            this.actionToCallOnPlayAcceped = actionToCallOnPlayAcceped;

            specialLevelText.text = "Special Level " + showSpecialLevelPopup;
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
            AnalyticsManager.Instance.LogSpecialLevelDataEvent(AnalyticsConstants.LevelData_StartTrigger);
        }

        public void LogSpecialLevelSkipEvent()
        {
            AnalyticsManager.Instance.LogSpecialLevelDataEvent(AnalyticsConstants.SpecialLevelData_SkipTrigger);
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
            actionToCallOnPlayAcceped?.Invoke();

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