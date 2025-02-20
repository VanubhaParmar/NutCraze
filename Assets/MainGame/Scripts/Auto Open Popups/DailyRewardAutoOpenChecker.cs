using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort {
    public class DailyRewardAutoOpenChecker : BaseAutoOpenPopupChecker
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
        public override void InitializeChecker()
        {
        }

        public override void CheckForAutoOpenPopup(Action actionToCallOnAutoOpenComplete)
        {
            base.CheckForAutoOpenPopup(actionToCallOnAutoOpenComplete);
            if (DailyRewardManager.Instance.CanClaimTodayReward())
                DailyRewardManager.Instance.ShowDailyRewardsViewAndClaimTodayRewards(OnAutoOpenCheckComplete);
            else
                OnAutoOpenCheckComplete();
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