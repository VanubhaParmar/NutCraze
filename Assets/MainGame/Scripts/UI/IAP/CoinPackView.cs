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
        [SerializeField] private CurrencyTopbarComponent coinTopBar;
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

            GameStatsCollector.Instance.OnGameCurrencyChanged(CurrencyConstant.COIN, coinReward.GetAmount(), GameCurrencyValueChangedReason.CURRENCY_EARNED_THROUGH_ADS_OR_IAP);

            GlobalUIManager.Instance.GetView<UserPromptView>().Show(UserPromptMessageConstants.PurchaseSuccessMessage, () => 
            {
                coinTopBar.SetCurrencyValue(DataManager.Instance.GetCurrency(coinReward.GetRewardId()).Value - coinReward.GetAmount());
                coinReward.ShowRewardAnimation(coinTopBar.CurrencyAnimation, transform.position);
            });
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