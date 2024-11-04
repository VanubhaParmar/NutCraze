using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort
{
    public class NoAdsPurchaseView : BaseView
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private Button purchaseButton;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public override void Show(Action action = null, bool isForceShow = false)
        {
            base.Show(action, isForceShow);
            purchaseButton.interactable = !DataManager.Instance.IsNoAdsPackPurchased();

            MainSceneUIManager.Instance.GetView<BannerAdsView>().Hide();
        }

        public override void Hide()
        {
            base.Hide();
            MainSceneUIManager.Instance.GetView<BannerAdsView>().Show(true);
        }
        #endregion

        #region PRIVATE_METHODS
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        public void OnButtonClick_Purchase()
        {
            DataManager.Instance.OnPurchaseNoAdsPack();
            Hide();
        }
        #endregion
    }
}