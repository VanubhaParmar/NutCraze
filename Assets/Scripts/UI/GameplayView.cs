using DG.Tweening;
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
        [SerializeField] private GameObject topContent;
        [SerializeField] private GameObject leftContent;
        [SerializeField] private GameObject levelFailBG;


        [ShowInInspector, ReadOnly] private float totalTimeSpentOnScreen;
        private Coroutine timeSpentCheckCo;

        [SerializeField] private RectTransform outOfMoveObject;
        [SerializeField] private CanvasGroup outOfMoveCG;
        [SerializeField] private RectTransform levelFailRetryButton;

        [SerializeField] private float outOfMoveAnimYOffset = 400f;
        [SerializeField] private float closeButtonAnimXOffset = 400f;
        [SerializeField] private Vector3 outOfMoveStartScale = new Vector3(0.5f, 0.5f, 1f);
        [SerializeField] private float outOfMoveStartAlpha = 0f;
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private Ease easeInPos = Ease.OutSine;
        [SerializeField] private Ease easeInScaleAlpha = Ease.OutQuad;
        [SerializeField] private Ease easeOutPos = Ease.InSine;
        [SerializeField] private Ease easeOutScaleAlpha = Ease.InQuad;

        [Space]
        [SerializeField] private ParticleSystem UndoBoosterEffect;
        [SerializeField] private ParticleSystem extraScrewBoosterEffect;
        [SerializeField] private AnimationCurve _scaleCurve;
        [SerializeField] private float _wait = 0.5f;
        [SerializeField] private float _idleDuration = 0.8f;

        private Vector2 outOfMoveTargetPosition;
        private Vector2 closeButtonTargetPosition;
        private Sequence _useBoosterSequence;
        private const string _useBoosterScaleSequenceId = "useBoosterScaleSequence";
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
        public override void Init()
        {
            base.Init();
            InitializeAnimationTargetPositions();
        }

        public override void Show(Action action = null, bool isForceShow = false)
        {
            base.Show(action, isForceShow);
            SetView();
            MainSceneUIManager.Instance.GetView<BannerAdsView>().Show(true);
            //SetLevelFailLayout(false);
        }

        public void SetView()
        {
            int currentLevel = DataManager.PlayerLevel;
            LevelSaveData levelSaveData = LevelProgressManager.Instance.LevelSaveData;
            bool isSpecialLevel = levelSaveData == null ? false : levelSaveData.levelType == LevelType.SPECIAL_LEVEL;

            if (isSpecialLevel && levelSaveData != null)
            {
                currentLevel = levelSaveData.level;
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

        public void SetLevelFailLayout(bool isActive, Action onRevive = null)
        {
            DOTween.Kill(outOfMoveObject);
            DOTween.Kill(levelFailRetryButton);
            DOTween.Kill(outOfMoveCG);
            PlayBoosterUseIdleAnimation();

            if (isActive)
            {
                BoosterManager.RegisterOnBoosterUse(OnBoosterUse);
                SetButtonActiveState(isActive);

                this.outOfMoveObject.gameObject.SetActive(true);
                this.levelFailRetryButton.gameObject.SetActive(true);

                Vector2 outOfMoveStartPos = outOfMoveTargetPosition + new Vector2(0, outOfMoveAnimYOffset);

                outOfMoveObject.anchoredPosition = outOfMoveStartPos;
                outOfMoveObject.localScale = outOfMoveStartScale;
                outOfMoveCG.alpha = outOfMoveStartAlpha;

                outOfMoveObject.DOAnchorPos(outOfMoveTargetPosition, duration).SetEase(easeInPos);
                outOfMoveObject.DOScale(Vector3.one, duration).SetEase(easeInScaleAlpha);
                outOfMoveCG.DOFade(1f, duration).SetEase(easeInScaleAlpha);

                Vector2 startpos = closeButtonTargetPosition + new Vector2(closeButtonAnimXOffset, 0);

                levelFailRetryButton.anchoredPosition = startpos;

                levelFailRetryButton.DOAnchorPos(closeButtonTargetPosition, duration).SetEase(easeInPos);
            }
            else
            {
                BoosterManager.DeRegisterOnBoosterUse(OnBoosterUse);
                Vector2 outOfMoveEndPos = outOfMoveTargetPosition + new Vector2(0, outOfMoveAnimYOffset);

                outOfMoveObject.DOAnchorPos(outOfMoveEndPos, duration).SetEase(easeOutPos);
                outOfMoveObject.DOScale(outOfMoveStartScale, duration).SetEase(easeOutScaleAlpha);
                outOfMoveCG.DOFade(outOfMoveStartAlpha, duration).SetEase(easeOutScaleAlpha)
                    .OnComplete(() => this.outOfMoveObject.gameObject.SetActive(false));


                Vector2 retryEndPos = closeButtonTargetPosition + new Vector2(closeButtonAnimXOffset, 0);

                levelFailRetryButton.DOAnchorPos(retryEndPos, duration).SetEase(easeOutPos)
                    .OnComplete(() =>
                    {
                        levelFailRetryButton.gameObject.SetActive(false);
                        SetButtonActiveState(isActive);
                        StopBoosterUseIdleAnimation();
                    });
            }

            void OnBoosterUse(int boosterId)
            {
                SetLevelFailLayout(false);
                onRevive?.Invoke();
            }

            void SetButtonActiveState(bool isActive)
            {
                levelFailBG.SetActive(isActive);
                topContent.SetActive(!isActive);
                leftContent.SetActive(!isActive);
            }
        }
        #endregion

        #region PRIVATE_METHODS  
        private void InitializeAnimationTargetPositions()
        {
            if (outOfMoveObject != null)
                outOfMoveTargetPosition = outOfMoveObject.anchoredPosition;
            if (levelFailRetryButton != null)
                closeButtonTargetPosition = levelFailRetryButton.anchoredPosition;
        }

        private void SetBoosterTexts()
        {
            bool canShowAd = AdManager.Instance.CanShowRewardedAd();

            BaseBooster undoBooster = BoosterManager.Instance.GetBooster(BoosterIdConstant.UNDO);
            int undoBoosterCount = undoBooster.GetBoosterCount();
            undoBoosterCountText.text = undoBoosterCount + "";
            undoBoosterAdWatchText.text = "+" + undoBooster.BoostersToAddOnAdWatch;
            undoBoosterCountText.transform.parent.gameObject.SetActive(undoBoosterCount != 0 || !canShowAd);
            undoBoosterAdWatchText.transform.parent.gameObject.SetActive(undoBoosterCount == 0 && canShowAd);

            BaseBooster extraScrewBooster = BoosterManager.Instance.GetBooster(BoosterIdConstant.EXTRASCREW);
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

        private void PlayBoosterUseIdleAnimation()
        {
            UndoBoosterEffect.Play();
            extraScrewBoosterEffect.Play();

            DOTween.Kill(_useBoosterScaleSequenceId);
            _useBoosterSequence = DOTween.Sequence().SetId(_useBoosterScaleSequenceId);
            DOScaleAnimation();
        }

        private void StopBoosterUseIdleAnimation()
        {
            UndoBoosterEffect.Stop();
            extraScrewBoosterEffect.Stop();
            DOTween.Kill(_useBoosterScaleSequenceId);

            undoBoosterParent.localScale = Vector3.one;
            extraScrewBoosterParent.localScale = Vector3.one;
        }

        private void DOScaleAnimation()
        {
            _useBoosterSequence.Join(
            DOTween.To(() => 0f, t =>
                {
                    float scaleValue = _scaleCurve.Evaluate(t);
                    undoBoosterParent.localScale = Vector3.one * scaleValue;
                }, 1f, _idleDuration)
            );

            _useBoosterSequence.Join(
                DOTween.To(() => 0f, t =>
                {
                    float scaleValue = _scaleCurve.Evaluate(t);
                    extraScrewBoosterParent.localScale = Vector3.one * scaleValue;
                }, 1f, _idleDuration)
            );

            _useBoosterSequence.AppendInterval(_wait);
            _useBoosterSequence.SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
        }
        #endregion

        #region COROUTINES
        IEnumerator TimeSpentCheckCoroutine()
        {
            while (true)
            {
                yield return WaitForUtils.OneSecond;

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
            if (!IsGameplayOngoing())
                return;
            AdManager.Instance.ShowInterstitial(InterstatialAdPlaceType.Reload_Level, AnalyticsConstants.GA_GameReloadInterstitialAdPlace);
            GameplayManager.Instance.RestartGamePlay();
        }

        public void OnRetryButtonClick()
        {
            if (!IsGameplayOngoing())
                return;
            SetLevelFailLayout(false);
            AdManager.Instance.ShowInterstitial(InterstatialAdPlaceType.Reload_Level, AnalyticsConstants.GA_GameReloadInterstitialAdPlace);
            GameplayManager.Instance.RestartGamePlay();
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

        public void OnLevelFailButtonClick()
        {
            if (!IsGameplayOngoing())
                return;
            LevelFailManager.Instance.OnLevelFail();
        }
        #endregion
    }
}