using I2.Loc;
using Sirenix.OdinInspector;
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
        [SerializeField] private LocalizationParamsManager normalLevelParams;
        [SerializeField] private LocalizationParamsManager specailLevelParams;

        [Space]
        [SerializeField] private RectTransform undoBoosterParent;
        [SerializeField] private Text undoBoosterCountText;
        [SerializeField] private Text undoBoosterAdWatchText;

        [Space]
        [SerializeField] private RectTransform extraScrewBoosterParent;
        [SerializeField] private Text extraScrewBoosterCountText;
        [SerializeField] private Text extraScrewBoosterAdWatchCountText;
        [SerializeField] private GameObject developementObject;

        [ShowInInspector, ReadOnly] private float totalTimeSpentOnScreen;
        private Coroutine timeSpentCheckCo;
        #endregion

        #region PROPERTIES
        public int TotalTimeSpentOnScreen => (int)totalTimeSpentOnScreen;
        public RectTransform ExtraScrewBoosterParent => extraScrewBoosterParent;
        public RectTransform UndoBoosterParent => undoBoosterParent;
        #endregion

        #region UNITY_CALLBACKS
        private void OnEnable()
        {
            SetDevelopementObjects();
            LevelManager.Instance.RegisterOnLevelLoad(OnLevelLoad);
            GameManager.onBoosterPurchaseSuccess += GameManager_onBoosterPurchaseSuccess;
            GameManager.onRewardsClaimedUIRefresh += GameManager_onRewardsClaimedUIRefresh;

            StartTimeSpentCheckingCoroutine();
        }

        private void SetDevelopementObjects()
        {
            developementObject.SetActive(!DevProfileHandler.Instance.IsProductionBuild());
        }

        private void OnDisable()
        {
            LevelManager.Instance.DeRegisterOnLevelLoad(OnLevelLoad);
            GameManager.onBoosterPurchaseSuccess -= GameManager_onBoosterPurchaseSuccess;
            GameManager.onRewardsClaimedUIRefresh -= GameManager_onRewardsClaimedUIRefresh;

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
        public void SetView()
        {
            int currentLevel = DataManager.PlayerLevel;
            LevelDataSO currentLevelDataSO = LevelManager.Instance.CurrentLevelDataSO;
            bool isSpecialLevel = currentLevelDataSO == null ? false : currentLevelDataSO.levelType == LevelType.SPECIAL_LEVEL;

            if (isSpecialLevel && currentLevelDataSO != null)
            {
                currentLevel = currentLevelDataSO.level;
            }

            SetLevelText(isSpecialLevel, currentLevel);
            SetBoosterTexts();

            void SetLevelText(bool isSpecailLevel, int currentLevel)
            {
                if (isSpecailLevel)
                {
                    normalLevelParams.gameObject.SetActive(false);
                    specailLevelParams.gameObject.SetActive(true);
                    specailLevelParams.SetParameterValue(specailLevelParams._Params[0].Name, currentLevel.ToString());
                }
                else
                {
                    specailLevelParams.gameObject.SetActive(false);
                    normalLevelParams.gameObject.SetActive(true);
                    normalLevelParams.SetParameterValue(normalLevelParams._Params[0].Name, currentLevel.ToString());
                }

            }
        }
        #endregion

        #region PRIVATE_METHODS
        private void SetBoosterTexts()
        {
            bool canShowAd = AdManager.Instance.CanShowRewardedAd();

            BaseBooster undoBooster = BoosterManager.Instance.GetBooster(BoosterIdConstant.UNDO);
            int undoBoosterCount = undoBooster.GetBoosterCount();
            undoBoosterCountText.text = undoBoosterCount + "";
            undoBoosterAdWatchText.text = "+" + undoBooster.BoostersToAddOnAdWatch;
            undoBoosterCountText.transform.parent.gameObject.SetActive(undoBoosterCount != 0 || !canShowAd);
            undoBoosterAdWatchText.transform.parent.gameObject.SetActive(undoBoosterCount == 0 && canShowAd);

            BaseBooster extraScrewBooster = BoosterManager.Instance.GetBooster(BoosterIdConstant.EXTRA_SCREW);
            int extraScrewBoosterCount = extraScrewBooster.GetBoosterCount();
            extraScrewBoosterCountText.text = extraScrewBoosterCount + "";
            extraScrewBoosterAdWatchCountText.text = "+" + extraScrewBooster.BoostersToAddOnAdWatch;
            extraScrewBoosterCountText.transform.parent.gameObject.SetActive(extraScrewBoosterCount != 0 || !canShowAd);
            extraScrewBoosterAdWatchCountText.transform.parent.gameObject.SetActive(extraScrewBoosterCount == 0 && canShowAd);
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
        private void OnLevelLoad()
        {
            SetView();
        }

        private bool IsGameplayOngoing()
        {
            return GameplayManager.Instance.IsPlayingLevel;
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
            LevelManager.Instance.OnReloadCurrentLevel();
        }

        public void OnButtonClick_NoAdsPack()
        {
            if (!IsGameplayOngoing()) return;

            if (!DataManager.Instance.IsNoAdsPackPurchased())
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
                GameplayManager.Instance.EndGamePlay();
        }

        public void OnSolveButtonClick()
        {
            if (!IsGameplayOngoing())
                return;
            LevelManager.Instance.StartAISolver();
        }

        public void OnStopButtonClick()
        {
            if (!IsGameplayOngoing())
                return;
            LevelManager.Instance.StopAISolver();
        }
        #endregion
    }
}