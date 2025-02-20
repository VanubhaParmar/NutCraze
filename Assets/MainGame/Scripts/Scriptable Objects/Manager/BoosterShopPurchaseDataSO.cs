using Sirenix.OdinInspector;
using UnityEngine;

namespace Tag.NutSort {
    [CreateAssetMenu(fileName = "BoosterShopPurchaseDataSO", menuName = Constant.GAME_NAME + "/Boosters/BoosterShopPurchaseDataSO")]
    public class BoosterShopPurchaseDataSO : SerializedScriptableObject
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [BoosterId] public int shopBoosterType;
        public int boosterCount;
        [CurrencyId] public int requiredCurrency;
        public int requiredCurrencyAmount;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public bool CanPurchaseThis()
        {
            var currency = DataManager.Instance.GetCurrency(requiredCurrency);
            return (currency != null && currency.HasEnoughValue(requiredCurrencyAmount));
        }

        public BoosterReward GetPurchaseReward()
        {
            return new BoosterReward() { rewardBoosterType = shopBoosterType, rewardAmount = boosterCount };
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