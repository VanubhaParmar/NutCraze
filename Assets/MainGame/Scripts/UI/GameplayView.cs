using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.tag.nut_sort {
    public class GameplayView : BaseView
    {
        #region PUBLIC_VARIABLES
        public int TotalTimeSpentOnScreen => (int)totalTimeSpentOnScreen;
        public RectTransform UndoBoosterParent => undoBoosterParent;
        public RectTransform ExtraScrewBoosterParent => extraScrewBoosterParent;

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

        [ShowInInspector, ReadOnly] private float totalTimeSpentOnScreen;
        private Coroutine timeSpentCheckCo;
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
            StartTimeSpentCheckingCoroutine();
        }

        private void OnDisable()
        {
            GameplayManager.onGameplayLevelLoadComplete -= GameplayManager_onGameplayLevelLoadComplete;
            GameManager.onBoosterPurchaseSuccess -= GameManager_onBoosterPurchaseSuccess;
            GameManager.onRewardsClaimedUIRefresh -= GameManager_onRewardsClaimedUIRefresh;

            TimeManager.Instance.DeRegisterTimerTickEvent(TimeManager_onTimerTick);
            StopTimeSpentCheckingCoroutine();
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
        public void SetView()
        {
            var playerData = PlayerPersistantData.GetMainPlayerProgressData();

            int currentLevel = LevelManager.Instance.CurrentLevelDataSO == null ? playerData.playerGameplayLevel : LevelManager.Instance.CurrentLevelDataSO.level;
            bool isSpecialLevel = LevelManager.Instance.CurrentLevelDataSO == null ? false : LevelManager.Instance.CurrentLevelDataSO.levelType == LevelType.SPECIAL_LEVEL;

            levelNumberText.text = isSpecialLevel ? $"Special Level {currentLevel}" : $"Level {currentLevel}";

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

        private void StartTimeSpentCheckingCoroutine()
        {
            if (timeSpentCheckCo == null)
                timeSpentCheckCo = StartCoroutine(TimeSpentCheckCoroutine());
        }

        private void StopTimeSpentCheckingCoroutine()
        {
            if (timeSpentCheckCo != null)
                StopCoroutine(timeSpentCheckCo);
            timeSpentCheckCo = null;
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
        IEnumerator TimeSpentCheckCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);

                var openViews = new List<BaseView>();
                openViews.AddRange(openView);

                openViews.Remove(MainSceneUIManager.Instance.GetView<BannerAdsView>()); // remove banner ads view from calculation

                int myViewIndex = openViews.IndexOf(this);
                if (myViewIndex >= 0 && myViewIndex == openViews.Count - 1)
                    totalTimeSpentOnScreen += 1f;
            }
        }
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
            if (!IsGameplayOngoing())
                return;
            MainSceneUIManager.Instance.GetView<ShopView>().Show();
        }

        public void OnButtonClick_UndoBooster()
        {
            if (!IsGameplayOngoing())
                return;
            BoosterManager.Instance.OnUndoButtonClick();
            SetView();
        }

        public void OnButtonClick_ExtraScrewBooster()
        {
            if (!IsGameplayOngoing())
                return;
            BoosterManager.Instance.OnExtraScrewButtonClick();
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