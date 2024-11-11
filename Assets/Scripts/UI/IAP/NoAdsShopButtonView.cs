using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort
{
    public class NoAdsShopButtonView : MonoBehaviour
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField, IAPProductId] private string iapProductId;
        [SerializeField] private Text purchaseCostText;
        [SerializeField] private Text undoBoosterCountText;
        [SerializeField] private Text extraBoltBoosterText;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        private void OnEnable()
        {
            SetView();
        }
        #endregion

        #region PUBLIC_METHODS
        #endregion

        #region PRIVATE_METHODS
        private void SetView()
        {
            purchaseCostText.text = IAPManager.Instance.GetIAPPrice(iapProductId);

            var noAdsRewards = IAPManager.Instance.IAPProducts.GetIAPPurchaseDataOf(iapProductId);
            undoBoosterCountText.text = "x" + noAdsRewards.rewardsDataSO.rewards.Find(x => x.GetRewardType() == RewardType.Boosters && x.GetRewardId() == (int)BoosterType.UNDO).GetAmount();
            extraBoltBoosterText.text = "x" + noAdsRewards.rewardsDataSO.rewards.Find(x => x.GetRewardType() == RewardType.Boosters && x.GetRewardId() == (int)BoosterType.EXTRA_BOLT).GetAmount();
        }

        private void OnPackPurchaseSuccess(string packId)
        {
            var noAdsRewards = IAPManager.Instance.IAPProducts.GetIAPPurchaseDataOf(iapProductId);
            DataManager.Instance.OnPurchaseNoAdsPack(noAdsRewards.rewardsDataSO.rewards);
            GlobalUIManager.Instance.GetView<UserPromptView>().Show(UserPromptMessageConstants.NoAdsPurchaseSuccess);
        }

        private void OnPackPurchaseFailed(string packId)
        {
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        public void OnButtonClick_BuyProduct()
        {
            if (DataManager.Instance.CanPurchaseNoAdsPack())
                IAPManager.Instance.PurchaseProduct(iapProductId, OnPackPurchaseSuccess, OnPackPurchaseFailed);
            else
                GlobalUIManager.Instance.GetView<UserPromptView>().Show(UserPromptMessageConstants.NoAdsAlreadyPurchase);
        }
        #endregion
    }
}