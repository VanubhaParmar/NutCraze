using System;

namespace Tag.NutSort
{
    public class RateUsPopupAutoOpenChecker : BaseAutoOpenPopupChecker
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
            if (GameManager.Instance.GameMainDataSO.CanShowRateUsPopUp() && !RateUsView.IsRated && GameStatsCollector.Instance.CurrentPlayedLevelsInThisSession > 0)
                MainSceneUIManager.Instance.GetView<RateUsView>().ShowWithHideAction(OnAutoOpenCheckComplete);
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