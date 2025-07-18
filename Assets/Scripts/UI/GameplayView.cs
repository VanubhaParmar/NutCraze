using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort
{
    public class GameplayView : BaseView
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private Text levelNumberText;

        [Space]
        [SerializeField] private RectTransform undoBoosterParent;
        [SerializeField] private Text undoBoosterCountText;
        [SerializeField] private Text undoBoosterAdWatchText;

        [Space]
        [SerializeField] private RectTransform extraScrewBoosterParent;
        [SerializeField] private Text extraScrewBoosterCountText;
        [SerializeField] private Text extraScrewBoosterAdWatchCountText;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        private void OnEnable()
        {
            GameplayManager.onGameplayLevelLoadComplete += GameplayManager_onGameplayLevelLoadComplete;
            GameManager.onBoosterPurchaseSuccess += GameManager_onBoosterPurchaseSuccess;
            GameManager.onRewardsClaimedUIRefresh += GameManager_onRewardsClaimedUIRefresh;

            TimeManager.Instance.RegisterTimerTickEvent(TimeManager_onTimerTick);
        }

        private void OnDisable()
        {
            GameplayManager.onGameplayLevelLoadComplete -= GameplayManager_onGameplayLevelLoadComplete;
            GameManager.onBoosterPurchaseSuccess -= GameManager_onBoosterPurchaseSuccess;
            GameManager.onRewardsClaimedUIRefresh -= GameManager_onRewardsClaimedUIRefresh;

            TimeManager.Instance.DeRegisterTimerTickEvent(TimeManager_onTimerTick);
        }
        #endregion

        #region PUBLIC_METHODS
        public override void Show(Action action = null, bool isForceShow = false)
        {
            base.Show(action, isForceShow);
            SetView();

            MainSceneUIManager.Instance.GetView<BannerAdsView>().Show(true);
        }
        #endregion

        #region PRIVATE_METHODS
        private void SetView()
        {
            var playerData = PlayerPersistantData.GetMainPlayerProgressData();

            bool isSpecialLevel = LevelManager.Instance.CurrentLevelDataSO.levelType == LevelType.SPECIAL_LEVEL;

            levelNumberText.text = isSpecialLevel ? $"Special Level {LevelManager.Instance.CurrentLevelDataSO.level}" : $"Level {playerData.playerGameplayLevel}";

            undoBoosterCountText.text = playerData.undoBoostersCount + "";
            undoBoosterAdWatchText.text = "+" + GameManager.Instance.GameMainDataSO.undoBoostersCountToAddOnAdWatch;
            undoBoosterCountText.transform.parent.gameObject.SetActive(playerData.undoBoostersCount != 0 || !AdManager.Instance.CanShowRewardedAd());
            undoBoosterAdWatchText.transform.parent.gameObject.SetActive(playerData.undoBoostersCount == 0 && AdManager.Instance.CanShowRewardedAd());

            extraScrewBoosterCountText.text = playerData.extraScrewBoostersCount + "";
            extraScrewBoosterAdWatchCountText.text = "+" + GameManager.Instance.GameMainDataSO.extraScrewBoostersCountToAddOnAdWatch;
            extraScrewBoosterCountText.transform.parent.gameObject.SetActive(playerData.extraScrewBoostersCount != 0 || !AdManager.Instance.CanShowRewardedAd());
            extraScrewBoosterAdWatchCountText.transform.parent.gameObject.SetActive(playerData.extraScrewBoostersCount == 0 && AdManager.Instance.CanShowRewardedAd());
        }

        private void GameManager_onBoosterPurchaseSuccess()
        {
            SetView();
        }

        private void GameManager_onRewardsClaimedUIRefresh()
        {
            SetView();
        }

        private void OnUndoBoostersWatchAdSuccess()
        {
            GameManager.Instance.AddWatchAdRewardUndoBoosters();
            SetView();

            FireBoosterAdWatchEvent(RewardAdShowCallType.Undo_Booster_Ad);

            MainSceneUIManager.Instance.GetView<VFXView>().PlayBoosterClaimAnimation(BoosterType.UNDO, GameManager.Instance.GameMainDataSO.undoBoostersCountToAddOnAdWatch, undoBoosterParent.position);
        }

        private void OnExtraBoostersWatchAdSuccess()
        {
            GameManager.Instance.AddWatchAdRewardExtraScrewBoosters();
            SetView();

            FireBoosterAdWatchEvent(RewardAdShowCallType.Extra_Booster_Ad);

            MainSceneUIManager.Instance.GetView<VFXView>().PlayBoosterClaimAnimation(BoosterType.EXTRA_BOLT, GameManager.Instance.GameMainDataSO.extraScrewBoostersCountToAddOnAdWatch, extraScrewBoosterParent.position);
        }

        private void FireBoosterAdWatchEvent(RewardAdShowCallType rewardAdShowCallType)
        {
            string boosterName = rewardAdShowCallType == RewardAdShowCallType.Undo_Booster_Ad ? AnalyticsConstants.AdsData_UndoBoosterName : AnalyticsConstants.AdsData_ExtraBoltBoosterName;
            AnalyticsManager.Instance.LogAdsDataEvent(boosterName);
        }
        #endregion

        #region EVENT_HANDLERS
        private void GameplayManager_onGameplayLevelLoadComplete()
        {
            SetView();
        }

        private void TimeManager_onTimerTick(DateTime currentDateTime)
        {
            GameplayManager.Instance.IncreaseLevelRunTime();
        }

        private bool IsGameplayOngoing()
        {
            return GameplayManager.Instance.GameplayStateData.gameplayStateType == GameplayStateType.PLAYING_LEVEL;
        }
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        public void OnButtonClick_ReloadLevel()
        {
            if (!IsGameplayOngoing()) return;

            AdManager.Instance.ShowInterstitial(InterstatialAdPlaceType.Reload_Level, AnalyticsConstants.GA_GameReloadInterstitialAdPlace);
            GameplayManager.Instance.OnReloadCurrentLevel();
        }

        public void OnButtonClick_NoAdsPack()
        {
            if (!IsGameplayOngoing()) return;

            if (DataManager.Instance.CanPurchaseNoAdsPack())
                MainSceneUIManager.Instance.GetView<NoAdsPurchaseView>().Show();
            else
                GlobalUIManager.Instance.GetView<UserPromptView>().Show(UserPromptMessageConstants.NoAdsAlreadyPurchase);
        }

        public void OnButtonClick_Settings()
        {
            if (!IsGameplayOngoing()) return;

            MainSceneUIManager.Instance.GetView<SettingsView>().Show();
        }

        public void OnButtonClick_Shop()
        {
            if (!IsGameplayOngoing()) return;

            MainSceneUIManager.Instance.GetView<ShopView>().Show();
        }

        public void OnButtonClick_UndoBooster()
        {
            if (!IsGameplayOngoing()) return;

            if (GameplayManager.Instance.CanUseUndoBooster())
                GameplayManager.Instance.UseUndoBooster();
            else if (!DataManager.Instance.CanUseUndoBooster())
            {
                if (AdManager.Instance.CanShowRewardedAd())
                    AdManager.Instance.ShowRewardedAd(OnUndoBoostersWatchAdSuccess, RewardAdShowCallType.Undo_Booster_Ad, AnalyticsConstants.GA_UndoRewardedBoosterAdPlace);
                else
                    OnButtonClick_Shop();
            }
            else
                ToastMessageView.Instance.ShowMessage(UserPromptMessageConstants.CantUseUndoBoosterMessage);


            SetView();
        }

        public void OnButtonClick_ExtraScrewBooster()
        {
            if (!IsGameplayOngoing()) return;

            if (GameplayManager.Instance.CanUseExtraScrewBooster())
                GameplayManager.Instance.UseExtraScrewBooster();
            else if (!DataManager.Instance.CanUseExtraScrewBooster())
            {
                if (AdManager.Instance.CanShowRewardedAd())
                    AdManager.Instance.ShowRewardedAd(OnExtraBoostersWatchAdSuccess, RewardAdShowCallType.Extra_Booster_Ad, AnalyticsConstants.GA_ExtraBoltRewardedBoosterAdPlace);
                else
                    OnButtonClick_Shop();
            }
            else
                ToastMessageView.Instance.ShowMessage(UserPromptMessageConstants.CantUseExtraBoltBoosterMessage);

            SetView();
        }

        public void OnButtonClick_LevelNumberTap()
        {
            if (DevelopmentProfileDataSO.winOnLevelNumberTap)
                GameplayManager.Instance.OnEditor_FinishLevel();
        }
        #endregion
    }
}