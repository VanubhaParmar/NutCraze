using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private CurrencyTopbarComponents coinTopBar;

        [Space]
        [SerializeField] private Text packPurchasePriceText;
        [SerializeField] private Text packCoinAmountText;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        private void OnEnable()
        {
            MainSceneUIManager.Instance.GetView<VFXView>().CoinAnimation.RegisterObjectAnimationComplete(HideViewOnLastCoinCollect);
        }

        private void OnDisable()
        {
            MainSceneUIManager.Instance.GetView<VFXView>().CoinAnimation.DeregisterObjectAnimationComplete(HideViewOnLastCoinCollect);
        }
        #endregion

        #region PUBLIC_METHODS
        public void InitView()
        {
            var packData = IAPManager.Instance.IAPProducts.GetIAPPurchaseDataOf(coinPackId);

            packPurchasePriceText.text = IAPManager.Instance.GetIAPPrice(coinPackId);
            packCoinAmountText.text = packData.rewardsDataSO.rewards.First().GetAmount() + "";
        }
        #endregion

        #region PRIVATE_METHODS
        private void OnPackPurchaseSuccess(string productId)
        {
            var packData = IAPManager.Instance.IAPProducts.GetIAPPurchaseDataOf(coinPackId);

            var coinReward = packData.rewardsDataSO.rewards.First();
            coinReward.GiveReward();

            MainSceneUIManager.Instance.GetView<VFXView>().PlayCoinAnimation(transform.position, coinReward.GetAmount(), coinTopBar.CurrencyImage.transform);
        }

        private void OnPackPurchaseFailed(string productId)
        {
        }

        private void HideViewOnLastCoinCollect(int value, bool isLastCoin)
        {
            coinTopBar.SetCurrencyValue(true);
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