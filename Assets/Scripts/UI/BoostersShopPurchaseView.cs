using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort
{
    public class BoostersShopPurchaseView : MonoBehaviour
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private BoosterShopPurchaseDataSO boosterShopPurchaseDataSO;
        [SerializeField] private Text boostersCountText;
        [SerializeField] private Text purchaseCurrencyAmountText;
        [SerializeField] private Image purchaseCurrencyImage;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void InitView()
        {
            boostersCountText.text = "x" + boosterShopPurchaseDataSO.boosterCount;
            purchaseCurrencyAmountText.text = boosterShopPurchaseDataSO.requiredCurrencyAmount + "";
            purchaseCurrencyImage.sprite = CommonSpriteHandler.Instance.GetCurrencySprite(boosterShopPurchaseDataSO.requiredCurrency);
        }
        #endregion

        #region PRIVATE_METHODS
        private void OnPurchaseSuccess()
        {
            var reward = boosterShopPurchaseDataSO.GetPurchaseReward();
            reward.GiveReward();
            DataManager.Instance.GetCurrency(boosterShopPurchaseDataSO.requiredCurrency).Add(-boosterShopPurchaseDataSO.requiredCurrencyAmount);

            MainSceneUIManager.Instance.GetView<VFXView>().PlayBoosterClaimAnimation(boosterShopPurchaseDataSO.shopBoosterType, reward.GetAmount(), transform.position);

            GameManager.RaiseOnBoosterPurchaseSuccess();
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        public void OnButtonClick_PurchaseBooster()
        {
            if (boosterShopPurchaseDataSO.CanPurchaseThis())
                OnPurchaseSuccess();
            else
                GlobalUIManager.Instance.GetView<UserPromptView>().Show(UserPromptMessageConstants.NotEnoughCoins);
        }
        #endregion
    }
}