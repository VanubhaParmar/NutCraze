using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort
{
    public class LevelFailView : BaseView
    {
        #region PRIVATE_VARIABLES
        [Header("Buttons")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button watchAdButton;
        [SerializeField] private Button spendCoinsButton;
        [SerializeField] private Button closeButton;

        [Header("Button Text")]
        [SerializeField] private Text spendCoinsButtonText;
        [SerializeField] private Text screwCapcityWithCoinsText;
        [SerializeField] private Text screwCapcityWithCoinsAds;

        private Action onRestartClicked;
        private Action onWatchAdClicked;
        private Action onSpendCoinsClicked;
        #endregion

        #region PUBLIC_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        private void OnEnable()
        {
            RegisterButtonEvents();
        }

        private void OnDisable()
        {
            DeregisterButtonEvents();
        }
        #endregion

        #region PUBLIC_METHODS
        public void Show(Action onRestartClicked,
            bool canReviveWithAds = false,
            bool canReviveWithCoins = false,
            int coinAmount = 0,
            int screwCapacityWithAds = 0,
            int screwCapacityWithCoins = 0,
            Action onWatchAdClicked = null,
            Action onSpendCoinsClicked = null)
        {
            this.onRestartClicked = onRestartClicked;
            this.onWatchAdClicked = onWatchAdClicked;
            this.onSpendCoinsClicked = onSpendCoinsClicked;


            watchAdButton.gameObject.SetActive(canReviveWithAds);
            spendCoinsButton.gameObject.SetActive(canReviveWithCoins);

            spendCoinsButtonText.text = $"{coinAmount}";
            screwCapcityWithCoinsText.text = $"+{screwCapacityWithCoins}";
            screwCapcityWithCoinsAds.text = $"+{screwCapacityWithAds}";
            base.Show();
        }
        #endregion

        #region PRIVATE_METHODS
        private void RegisterButtonEvents()
        {
            if (retryButton != null)
                retryButton.onClick.AddListener(OnRestartButtonClicked);

            if (watchAdButton != null)
                watchAdButton.onClick.AddListener(OnWatchAdButtonClicked);

            if (spendCoinsButton != null)
                spendCoinsButton.onClick.AddListener(OnSpendCoinsButtonClicked);

            if (closeButton != null)
                closeButton.onClick.AddListener(OnRestartButtonClicked);
        }

        private void DeregisterButtonEvents()
        {
            if (retryButton != null)
                retryButton.onClick.RemoveListener(OnRestartButtonClicked);

            if (watchAdButton != null)
                watchAdButton.onClick.RemoveListener(OnWatchAdButtonClicked);

            if (spendCoinsButton != null)
                spendCoinsButton.onClick.RemoveListener(OnSpendCoinsButtonClicked);

            if (closeButton != null)
                closeButton.onClick.RemoveListener(OnRestartButtonClicked);
        }
        #endregion

        #region UI_CALLBACKS
        private void OnRestartButtonClicked()
        {
            onRestartClicked?.Invoke();
        }

        private void OnWatchAdButtonClicked()
        {
            onWatchAdClicked?.Invoke();
        }

        private void OnSpendCoinsButtonClicked()
        {
            onSpendCoinsClicked?.Invoke();
        }
        #endregion
    }
}