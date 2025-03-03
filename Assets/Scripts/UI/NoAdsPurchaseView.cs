using System;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort
{
    public class NoAdsPurchaseView : BaseView
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField, IAPProductId] private string iapProductId;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private Text purchaseCostText;

        [SerializeField] private Text undoBoosterCountText;
        [SerializeField] private Text extraBoltBoosterCountText;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public override void Show(Action action = null, bool isForceShow = false)
        {
            GameStatsCollector.Instance.OnPopUpTriggered();

            base.Show(action, isForceShow);
            purchaseButton.interactable = !DataManager.Instance.IsNoAdsPackPurchased();

            MainSceneUIManager.Instance.GetView<BannerAdsView>().Hide();

            var noAdsRewards = IAPManager.Instance.IAPProducts.GetIAPPurchaseDataOf(iapProductId);
            purchaseCostText.text = IAPManager.Instance.GetIAPPrice(iapProductId);

            undoBoosterCountText.text = "x" + noAdsRewards.rewardsDataSO.rewards.Find(x => x.GetRewardType() == RewardType.Boosters && x.GetRewardId() == BoosterIdConstant.UNDO).GetAmount();
            extraBoltBoosterCountText.text = "x" + noAdsRewards.rewardsDataSO.rewards.Find(x => x.GetRewardType() == RewardType.Boosters && x.GetRewardId() == BoosterIdConstant.EXTRA_SCREW).GetAmount();
        }

        public override void Hide()
        {
            base.Hide();
            MainSceneUIManager.Instance.GetView<BannerAdsView>().Show(true);
        }
        #endregion

        #region PRIVATE_METHODS
        private void OnPackPurchaseSuccess(string packId)
        {
            var noAdsRewards = IAPManager.Instance.IAPProducts.GetIAPPurchaseDataOf(iapProductId);
            DataManager.Instance.OnPurchaseNoAdsPack(noAdsRewards.rewardsDataSO.rewards);

            Hide();
            GlobalUIManager.Instance.GetView<UserPromptView>().Show(UserPromptMessageConstants.NoAdsPurchaseSuccess, PlayRewardsAnimation);

            GameManager.RaiseOnBoosterPurchaseSuccess();
        }

        private void PlayRewardsAnimation()
        {
            var noAdsRewards = IAPManager.Instance.IAPProducts.GetIAPPurchaseDataOf(iapProductId);
            foreach (var item in noAdsRewards.rewardsDataSO.rewards)
            {
                if (item.GetRewardType() == RewardType.Boosters)
                {
                    MainSceneUIManager.Instance.GetView<VFXView>().PlayBoosterClaimAnimation(item.GetRewardId(), item.GetAmount(), GetTargetTransform(item.GetRewardId()).position);
                }
            }
        }

        private Transform GetTargetTransform(int boosterType)
        {
            return boosterType == BoosterIdConstant.UNDO ? undoBoosterCountText.transform : extraBoltBoosterCountText.transform;
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
        public void OnButtonClick_Purchase()
        {
            if (!DataManager.Instance.IsNoAdsPackPurchased())
            {
                IAPManager.Instance.PurchaseProduct(iapProductId, OnPackPurchaseSuccess, OnPackPurchaseFailed);
            }
            else
            {
                GlobalUIManager.Instance.GetView<UserPromptView>().Show(UserPromptMessageConstants.NoAdsAlreadyPurchase);
                Hide();
            }
        }
        #endregion
    }
}