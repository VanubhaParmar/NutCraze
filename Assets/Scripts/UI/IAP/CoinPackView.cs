using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort
{
    public class CoinPackView : MonoBehaviour
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField, IAPProductId] private string coinPackId;

        [Space]
        [SerializeField] private Text packPurchasePriceText;
        [SerializeField] private Text packCoinAmountText;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void InitView()
        {
            packPurchasePriceText.text = IAPManager.Instance.GetIAPPrice(coinPackId);
        }
        #endregion

        #region PRIVATE_METHODS
        private void OnPackPurchaseSuccess(string productId)
        {

        }

        private void OnPackPurchaseFailed(string productId)
        {

        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        public void OnButtonClick_BuyPack()
        {
            IAPManager.Instance.PurchaseProduct(coinPackId, OnPackPurchaseSuccess, OnPackPurchaseFailed);
        }
        #endregion
    }
}