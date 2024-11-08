using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

        [Space]
        [SerializeField] private Button claimButton;

        private Action actionToCallOnClaim;
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
            this.actionToCallOnClaim = actionToCallOnClaim;
            MainSceneUIManager.Instance.GetView<VFXView>().CoinAnimation.RegisterObjectAnimationComplete(HideViewOnLastCoinCollect);
            Show();
            SetView();

            claimButton.interactable = true;
            MainSceneUIManager.Instance.GetView<BannerAdsView>().Show(false);
        }

        public override void Hide()
        {
            MainSceneUIManager.Instance.GetView<VFXView>().CoinAnimation.DeregisterObjectAnimationComplete(HideViewOnLastCoinCollect);
            base.Hide();
        }

        public override void OnViewShowDone()
        {
            base.OnViewShowDone();

            if (DailyGoalsProgressHelper.IsAnyProgress())
                PlayDailyTaskProgressAnimation();
        }
        #endregion

        #region PRIVATE_METHODS
        private void SetView()
        {
            int currentCoins = DataManager.Instance.GetCurrency(CurrencyConstant.COINS).Value;

            dailyGoalsParentRect.gameObject.SetActive(false);
            if (DailyGoalsManager.Instance.IsSytemInitialized)
            {
                dailyGoalsParentRect.gameObject.SetActive(true);
                SetInitialDailyTaskView();

                if (DailyGoalsManager.Instance.DailyGoalsResetTimer != null)
                {
                    DailyGoalsManager.Instance.DailyGoalsResetTimer.RegisterTimerTickEvent(UpdateTaskTimer);
                    DailyGoalsManager.Instance.DailyGoalsResetTimer.RegisterTimerOverEvent(OnTaskTimerOver);

                    UpdateTaskTimer();
                }
            }

            int levelCompleteRewardAmount = GameManager.Instance.GameMainDataSO.levelCompleteReward.GetAmount();
            gameplayWinCoinText.text = "+" + levelCompleteRewardAmount;

            int totalCoinsRewards = levelCompleteRewardAmount;
            if (DailyGoalsProgressHelper.AreAllTaskCompleted)
                totalCoinsRewards += DailyGoalsManager.Instance.GetAllTaskCompleteReward().GetAmount();

            coinTopBar.SetCurrencyValue(currentCoins - totalCoinsRewards);
        }

        private void SetInitialDailyTaskView()
        {
            float totalTarget = 0f;
            float totalCurrentProgress = 0f;

            for (int i = 0; i < DailyGoalsManager.Instance.DailyGoals.Count; i++)
            {
                int initialProgress = Mathf.Max(0, DailyGoalsManager.Instance.DailyGoals[i].dailyGoalCurrentProgress - DailyGoalsProgressHelper.GetTaskProgress(DailyGoalsManager.Instance.DailyGoals[i].dailyGoalsTaskType));

                dailyGoalTaskUIViews[i].InitializeDailyGoalTaskView(DailyGoalsManager.Instance.DailyGoals[i]);
                dailyGoalTaskUIViews[i].SetViewProgress(initialProgress, initialProgress == DailyGoalsManager.Instance.DailyGoals[i].dailyGoalTargetCount);

                totalTarget += DailyGoalsManager.Instance.DailyGoals[i].dailyGoalTargetCount;
                totalCurrentProgress += initialProgress;
            }

            totalDailyGoalsProgressText.text = Mathf.FloorToInt((totalCurrentProgress * 100f) / totalTarget) + "%";
            totalDailyGoalsProgressFillBar.Fill(Mathf.InverseLerp(0f, totalTarget, totalCurrentProgress));
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

        private void PlayDailyTaskCompleteAnimation()
        {
            OnDailyTaskAllAnimationCompleted();
        }

        private void OnDailyTaskAllAnimationCompleted()
        {
            EventSystemHelper.Instance.BlockInputs(false);
            DailyGoalsProgressHelper.ResetProgress();
        }

        private void UpdateTaskTimer()
        {
            dailyGoalRefreshTimerText.text = DailyGoalsManager.Instance.DailyGoalsResetTimer.GetRemainingTimeSpan().ParseTimeSpan(2);
        }

        private void UnregisterDailyGoalsTimer()
        {
            if (DailyGoalsManager.Instance.IsSytemInitialized && DailyGoalsManager.Instance.DailyGoalsResetTimer != null)
            {
                DailyGoalsManager.Instance.DailyGoalsResetTimer.UnregisterTimerTickEvent(UpdateTaskTimer);
                DailyGoalsManager.Instance.DailyGoalsResetTimer.UnregisterTimerOverEvent(OnTaskTimerOver);
            }
        }

        private void OnTaskTimerOver()
        {
            UnregisterDailyGoalsTimer();
            SetView();
        }

        private void HideViewOnLastCoinCollect(int value,bool isLastCoin)
        {
            coinTopBar.SetCurrencyValue(true);

            if (isLastCoin)
            {
                StartCoroutine(WaitAndCall(0.5f, () =>
                {
                    Hide();
                    actionToCallOnClaim?.Invoke();
                }));
            }
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        IEnumerator WaitAndCall(float waitTime, Action actionToCall)
        {
            yield return new WaitForSecondsRealtime(waitTime);
            actionToCall?.Invoke();
        }
        #endregion

        #region UI_CALLBACKS
        public void OnButtonClick_Claim()
        {
            //Hide();
            claimButton.interactable = false;
            AdManager.Instance.ShowInterstitial(InterstatialAdPlaceType.Game_Win_Screen, AnalyticsConstants.GA_GameWinInterstitialAdPlace);
            MainSceneUIManager.Instance.GetView<VFXView>().PlayCoinAnimation(gameplayWinCoinText.transform.position, GameManager.Instance.GameMainDataSO.levelCompleteReward.GetAmount(), coinTopBar.CurrencyImage.transform);
        }
        #endregion
    }
}