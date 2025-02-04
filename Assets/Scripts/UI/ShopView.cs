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
            GameStatsCollector.Instance.OnPopUpTriggered();

            base.Show(action, isForceShow);
            InitPackViews();
            MainSceneUIManager.Instance.GetView<BannerAdsView>().Hide();
        }

        public override void Hide()
        {
            MainSceneUIManager.Instance.GetView<BannerAdsView>().Show(true);
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