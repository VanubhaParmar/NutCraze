using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.tag.nut_sort {
    public class BannerAdsView : BaseView
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private RectTransform noAdsPurchaseButton;

        private bool isShowCalled;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        private void OnEnable()
        {
            DataManager.onNoAdsPackPurchased += DataManager_onNoAdsPackPurchased;
        }

        private void OnDisable()
        {
            DataManager.onNoAdsPackPurchased -= DataManager_onNoAdsPackPurchased;
        }
        #endregion

        #region PUBLIC_METHODS
        public void Show(bool canPurchaseNoAds)
        {
            if (DataManager.Instance.IsNoAdsPackPurchased())
                return;

            base.Show();
            noAdsPurchaseButton.gameObject.SetActive(canPurchaseNoAds);

            if (!isShowCalled)
                AdManager.Instance.ShowBannerAd(out isShowCalled);
        }

        public override void Hide()
        {
            base.Hide();

            if (isShowCalled)
                AdManager.Instance.HideBannerAd();

            isShowCalled = false;
        }
        #endregion

        #region PRIVATE_METHODS
        private bool IsGameplayOngoing()
        {
            return GameplayManager.Instance.GameplayStateData.gameplayStateType == GameplayStateType.PLAYING_LEVEL;
        }
        #endregion

        #region EVENT_HANDLERS
        private void DataManager_onNoAdsPackPurchased()
        {
            Hide();
        }
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        public void OnButtonClick_RemoveAds()
        {
            if (!IsGameplayOngoing()) return;

            MainSceneUIManager.Instance.GetView<NoAdsPurchaseView>().Show();
        }
        #endregion
    }
}