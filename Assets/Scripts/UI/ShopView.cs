using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class ShopView : BaseView
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private List<CoinPackView> coinPackViews;
        [SerializeField] private List<BoostersShopPurchaseView> boostersShopPurchaseViews;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public override void Show(Action action = null, bool isForceShow = false)
        {
            base.Show(action, isForceShow);
            InitPackViews();
        }

        public override void Hide()
        {
            MainSceneUIManager.Instance.GetView<GameplayView>().Show();
            MainSceneUIManager.Instance.GetView<BannerAdsView>().Show();

            base.Hide();
        }
        #endregion

        #region PRIVATE_METHODS
        private void InitPackViews()
        {
            coinPackViews.ForEach(x => x.InitView());
            boostersShopPurchaseViews.ForEach(x => x.InitView());
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}