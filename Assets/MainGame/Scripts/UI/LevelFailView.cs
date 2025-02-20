using System;

namespace Tag.NutSort
{
    public class LevelFailView : BaseView
    {
        #region PRIVATE_VARS
        #endregion

        #region PUBLIC_VARS
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_FUNCTIONS
        #endregion

        #region PRIVATE_FUNCTIONS
        #endregion

        #region CO-ROUTINES
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region UI_CALLBACKS
        public void OnReviveButtonClick()
        {
            int reviveCost = GameManager.Instance.GameMainDataSO.LevelFailReviveCoinCost;
            DataManager.Instance.AddCurrency(CurrencyConstant.COIN, -reviveCost, () => 
            {
            });
        }
        public void OnGiveUpButtonClick()
        {

        }
        #endregion

        #region EDITOR
#if UNITY_EDITOR
#endif
        #endregion
    }
}
