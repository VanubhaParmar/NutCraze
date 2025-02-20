using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort {
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
            undoBoosterCountText.text = "x" + noAdsRewards.rewardsDataSO.rewards.Find(x => x.GetRewardType() == RewardType.Boosters && x.GetRewardId() == BoosterIdConstant.UNDO).GetAmount();
            extraBoltBoosterText.text = "x" + noAdsRewards.rewardsDataSO.rewards.Find(x => x.GetRewardType() == RewardType.Boosters && x.GetRewardId() == BoosterIdConstant.EXTRA_SCREW).GetAmount();
        }

        private void OnPackPurchaseSuccess(string packId)
        {
            IAPPurchaseData iAPPurchaseData = IAPManager.Instance.IAPProducts.GetIAPPurchaseDataOf(iapProductId);
            List<BaseReward> rewards = iAPPurchaseData.rewardsDataSO.rewards;
            if (rewards != null)
                rewards.ForEach(x => x.GiveReward());

            DataManager.Instance.PurchaseNoAdsPack();
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
            return boosterType == BoosterIdConstant.UNDO ? undoBoosterCountText.transform : extraBoltBoosterText.transform;
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