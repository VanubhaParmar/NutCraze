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
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public override void Show(Action action = null, bool isForceShow = false)
        {
            base.Show(action, isForceShow);
            InitCoinPackViews();
        }

        public override void Hide()
        {
            MainSceneUIManager.Instance.GetView<GameplayView>().Show();
            MainSceneUIManager.Instance.GetView<BannerAdsView>().Show();

            base.Hide();
        }
        #endregion

        #region PRIVATE_METHODS
        private void InitCoinPackViews()
        {
            coinPackViews.ForEach(x => x.InitView());
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