using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityForge.PropertyDrawers;

namespace Tag.NutSort
{
    public class GameWinView : BaseView
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private CurrencyTopbarComponents coinTopBar;

        [SerializeField] private RectTransform dailyGoalsParentRect;
        [SerializeField] private Text dailyGoalRefreshTimerText;
        [SerializeField] private List<DailyGoalTaskUIView> dailyGoalTaskUIViews;

        [Space]
        [SerializeField] private Text totalDailyGoalsProgressText;
        [SerializeField] private RectFillBar totalDailyGoalsProgressFillBar;

        [Space]
        [SerializeField] private Text gameplayWinCoinText;
        [SerializeField] private Text dailyGoalsCoinRewardText;
        [SerializeField] private Image dailyTaskComplete;
        [SerializeField] private RectTransform dailyTaskGiftboxImageParent;
        [SerializeField] private RectTransform dailyTaskGiftboxClaimedParent;

        [Space]
        [SerializeField] private Button claimButton;
        [SerializeField] private GameObject dailyTaskCompleteParent;

        [Space]
        [SerializeField] private Animator animator;
        [SerializeField, AnimatorStateName("animator")] private string dailyBonusGiftAnimation;
        [SerializeField, AnimatorStateName("animator")] private string dailyBonusGiftOutAnimation;

        private Action actionToCallOnClaim;
        private int coinTarget;
        private bool areAllDailyTasksCompleted;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        private void OnDisable()
        {
            UnregisterDailyGoalsTimer();
        }
        #endregion

        #region PUBLIC_METHODS
        public void ShowWinView(Action actionToCallOnClaim = null)
        {
            GameStatsCollector.Instance.OnPopUpTriggered(GameStatPopUpTriggerType.SYSTEM_TRIGGERED);
            this.actionToCallOnClaim = actionToCallOnClaim;
            SetView();
            Show();
            MainSceneUIManager.Instance.GetView<VFXView>().CoinAnimation.RegisterObjectAnimationComplete(UpdateCoinText);
            MainSceneUIManager.Instance.GetView<BannerAdsView>().Show(false);
        }

        public override void Hide()
        {
            MainSceneUIManager.Instance.GetView<VFXView>().CoinAnimation.DeregisterObjectAnimationComplete(UpdateCoinText);
            base.Hide();
        }

        public override void OnViewShowDone()
        {
            base.OnViewShowDone();
            if (DailyGoalsManager.Instance.CanShowDailyGoalsOnWinScreen() && DailyGoalsProgressHelper.IsAnyProgress())
                PlayDailyTaskProgressAnimation();
            else
                OnAllAnimationsDone();

        }
        #endregion

        #region PRIVATE_METHODS

        private void OnAllAnimationsDone()
        {
            claimButton.interactable = true;
            EventSystemHelper.Instance.BlockInputs(false);
        }

        private void SetView()
        {
            if (DailyGoalsManager.Instance.CanShowDailyGoalsOnWinScreen())
            {
                dailyGoalsParentRect.gameObject.SetActive(true);
                SetInitialDailyTaskView();
                if (DailyGoalsManager.Timer != null)
                {
                    DailyGoalsManager.Timer.RegisterTimerTickEvent(UpdateTaskTimer);
                    DailyGoalsManager.Timer.RegisterTimerOverEvent(OnTaskTimerOver);
                    UpdateTaskTimer();
                }
            }
            else
            {
                dailyGoalsParentRect.gameObject.SetActive(false);
            }

            int currentCoins = DataManager.Instance.GetCurrency(CurrencyConstant.COINS).Value;
            int levelCompleteRewardAmount = GameManager.Instance.GameMainDataSO.levelCompleteReward.GetAmount();
            gameplayWinCoinText.text = "+" + levelCompleteRewardAmount;

            int totalCoinsRewards = levelCompleteRewardAmount;
            if (DailyGoalsProgressHelper.AreAllTaskCompleted)
                totalCoinsRewards += DailyGoalsManager.Instance.GetAllTaskCompleteReward().GetAmount();

            coinTopBar.SetCurrencyValue(currentCoins - totalCoinsRewards);
            coinTarget = currentCoins - totalCoinsRewards;

            dailyGoalsCoinRewardText.text = "+" + DailyGoalsManager.Instance.DailyGoalsSystemDataSO.allTaskCompleteReward.GetAmount();
        }

        private void SetInitialDailyTaskView()
        {
            areAllDailyTasksCompleted = false;

            float totalTarget = 0f;
            float totalCurrentProgress = 0f;

            float totalRealProgress = 0f;

            List<DailyGoalPlayerData> dailyGoals = DailyGoalsManager.Instance.DailyGoals;
            for (int i = 0; i < dailyGoals.Count; i++)
            {
                DailyGoalPlayerData dailyGoal = dailyGoals[i];
                int initialProgress = Mathf.Max(0, dailyGoal.dailyGoalCurrentProgress - DailyGoalsProgressHelper.GetTaskProgress(dailyGoal.dailyGoalsTaskType));
                totalRealProgress += dailyGoal.dailyGoalCurrentProgress;

                dailyGoalTaskUIViews[i].Init(dailyGoal);
                dailyGoalTaskUIViews[i].SetViewProgress(initialProgress, initialProgress == dailyGoal.dailyGoalTargetCount);

                totalTarget += dailyGoal.dailyGoalTargetCount;
                totalCurrentProgress += initialProgress;
            }

            totalDailyGoalsProgressText.text = Mathf.FloorToInt((totalCurrentProgress * 100f) / totalTarget) + "%";
            totalDailyGoalsProgressFillBar.Fill(Mathf.InverseLerp(0f, totalTarget, totalCurrentProgress));

            dailyTaskGiftboxImageParent.gameObject.SetActive(true);
            dailyTaskGiftboxClaimedParent.gameObject.SetActive(false);

            if (Mathf.InverseLerp(0f, totalTarget, totalRealProgress) >= 1f)
            {
                areAllDailyTasksCompleted = true;

                if (!DailyGoalsProgressHelper.IsAnyProgress())
                    dailyTaskGiftboxClaimedParent.gameObject.SetActive(true);
            }
        }

        private void PlayDailyTaskProgressAnimation()
        {
            EventSystemHelper.Instance.BlockInputs(true);

            int totalTarget = 0;
            int totalCurrentProgress = 0;
            int totalInitialProgress = 0;

            for (int i = 0; i < DailyGoalsManager.Instance.DailyGoals.Count; i++)
            {
                int initialProgress = Mathf.Max(0, DailyGoalsManager.Instance.DailyGoals[i].dailyGoalCurrentProgress - DailyGoalsProgressHelper.GetTaskProgress(DailyGoalsManager.Instance.DailyGoals[i].dailyGoalsTaskType));

                totalInitialProgress += initialProgress;
                totalTarget += DailyGoalsManager.Instance.DailyGoals[i].dailyGoalTargetCount;
                totalCurrentProgress += DailyGoalsManager.Instance.DailyGoals[i].dailyGoalCurrentProgress;
            }

            Sequence animSeq = DOTween.Sequence();
            float fillTimeDifference = 0.2f;

            float timeVal = 0f;
            animSeq.InsertCallback(timeVal, () => { PlayProgressAnimation(totalInitialProgress, totalCurrentProgress, totalTarget); });
            timeVal += fillTimeDifference;

            for (int i = 0; i < dailyGoalTaskUIViews.Count; i++)
            {
                if (dailyGoalTaskUIViews[i].CanPlayProgressAnimation())
                {
                    int index = i;
                    animSeq.InsertCallback(timeVal, () => { dailyGoalTaskUIViews[index].PlayProgressAnimation(); });
                    timeVal += fillTimeDifference;
                }
            }

            animSeq.AppendInterval(0.5f);
            animSeq.onComplete += OnDailyTaskProgressAnimationCompleted;
        }

        private void PlayProgressAnimation(int currentProgress, int finalProgress, int totalTarget, float animationTime = 0.5f)
        {
            float fillTarget = Mathf.InverseLerp(0f, totalTarget, finalProgress);

            totalDailyGoalsProgressFillBar.Fill(fillTarget, animationTime);
            totalDailyGoalsProgressText.DoNumberAnimation(Mathf.FloorToInt((currentProgress * 100f) / totalTarget), Mathf.FloorToInt((finalProgress * 100f) / totalTarget), animationTime, "{0}%");
        }

        private void OnDailyTaskProgressAnimationCompleted()
        {
            if (DailyGoalsProgressHelper.AreAllTaskCompleted)
                PlayDailyTaskCompleteAnimation();
            else
                OnDailyTaskAllAnimationCompleted();
        }

        [Button]
        private void PlayDailyTaskCompleteAnimation()
        {
            DailyGoalsManager.Instance.IsGoalCompleteAnimationDone = true;

            dailyTaskCompleteParent.SetActive(true);

            Sequence completeSeq = DOTween.Sequence();
            completeSeq.AppendCallback(() =>
            {
                animator.Play(dailyBonusGiftAnimation);
                dailyTaskGiftboxImageParent.gameObject.SetActive(false);
            });
            completeSeq.InsertCallback(animator.GetAnimationLength(dailyBonusGiftAnimation) * 0.5f, () =>
            {
                SoundHandler.Instance.PlaySound(SoundType.GiftboxOpen);
            });
            completeSeq.AppendInterval(animator.GetAnimationLength(dailyBonusGiftAnimation));
            completeSeq.AppendCallback(() =>
            {
                animator.Play(dailyBonusGiftOutAnimation);
                GiftBoxClaimCoinReward();
            });
            completeSeq.AppendInterval(animator.GetAnimationLength(dailyBonusGiftOutAnimation));
            completeSeq.AppendCallback(OnDailyTaskAllAnimationCompleted);
        }


        public void GiftBoxClaimCoinReward()
        {
            int dailyGoalsReward = DailyGoalsManager.Instance.DailyGoalsSystemDataSO.allTaskCompleteReward.GetAmount();

            coinTarget += dailyGoalsReward;
            MainSceneUIManager.Instance.GetView<VFXView>().PlayCoinAnimation(dailyTaskComplete.transform.position, dailyGoalsReward, coinTopBar.CurrencyImage.transform);
        }

        private void UpdateCoinText(int value, bool isLastCoin)
        {
            SoundHandler.Instance.PlaySound(SoundType.CoinPlace);
            coinTopBar.SetCurrencyValue(true, target: coinTarget);

            if (isLastCoin)
            {
                CoroutineRunner.Instance.Wait(0.5f, () =>
                {
                    Hide();
                    actionToCallOnClaim?.Invoke();
                });
            }
        }

        private void OnDailyTaskAllAnimationCompleted()
        {
            dailyTaskCompleteParent.SetActive(false);
            EventSystemHelper.Instance.BlockInputs(false);
            DailyGoalsProgressHelper.ResetProgress();

            dailyTaskGiftboxImageParent.gameObject.SetActive(true);
            if (areAllDailyTasksCompleted)
                dailyTaskGiftboxClaimedParent.gameObject.SetActive(true);

            OnAllAnimationsDone();
        }

        private void UpdateTaskTimer()
        {
            dailyGoalRefreshTimerText.text = DailyGoalsManager.Timer.GetRemainingTimeSpan().ParseTimeSpan(2);
        }

        private void UnregisterDailyGoalsTimer()
        {
            if (DailyGoalsManager.Instance.IsSytemInitialized && DailyGoalsManager.Timer != null)
            {
                DailyGoalsManager.Timer.UnregisterTimerTickEvent(UpdateTaskTimer);
                DailyGoalsManager.Timer.UnregisterTimerOverEvent(OnTaskTimerOver);
            }
        }

        private void OnTaskTimerOver()
        {
            UnregisterDailyGoalsTimer();
            SetView();
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES

        #endregion

        #region UI_CALLBACKS
        public void OnButtonClick_Claim()
        {
            claimButton.interactable = false;
            AdManager.Instance.ShowInterstitial(InterstatialAdPlaceType.Game_Win_Screen, AnalyticsConstants.GA_GameWinInterstitialAdPlace);

            coinTarget = -1; // sets current value of coins
            MainSceneUIManager.Instance.GetView<VFXView>().PlayCoinAnimation(gameplayWinCoinText.transform.position, GameManager.Instance.GameMainDataSO.levelCompleteReward.GetAmount(), coinTopBar.CurrencyImage.transform);
        }
        #endregion
    }
}