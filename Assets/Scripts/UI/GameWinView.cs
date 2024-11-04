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
        [SerializeField] private RectTransform dailyGoalsParentRect;
        [SerializeField] private Text dailyGoalRefreshTimerText;
        [SerializeField] private List<DailyGoalTaskUIView> dailyGoalTaskUIViews;

        [Space]
        [SerializeField] private Text totalDailyGoalsProgressText;
        [SerializeField] private RectFillBar totalDailyGoalsProgressFillBar;

        [Space]
        [SerializeField] private Text gameplayWinCoinText;

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

            Show();
            SetView();

            MainSceneUIManager.Instance.GetView<BannerAdsView>().Show(false);
        }
        #endregion

        #region PRIVATE_METHODS
        private void SetView()
        {
            dailyGoalsParentRect.gameObject.SetActive(false);
            if (DailyGoalsManager.Instance.IsSytemInitialized)
            {
                dailyGoalsParentRect.gameObject.SetActive(true);

                float totalTarget = 0f;
                float totalCurrentProgress = 0f;

                for (int i = 0; i < DailyGoalsManager.Instance.DailyGoals.Count; i++)
                {
                    dailyGoalTaskUIViews[i].InitializeDailyGoalTaskView(DailyGoalsManager.Instance.DailyGoals[i]);

                    totalTarget += DailyGoalsManager.Instance.DailyGoals[i].dailyGoalTargetCount;
                    totalCurrentProgress += DailyGoalsManager.Instance.DailyGoals[i].dailyGoalCurrentProgress;
                }

                totalDailyGoalsProgressText.text = Mathf.FloorToInt((totalCurrentProgress * 100f) / totalTarget) + "%";
                totalDailyGoalsProgressFillBar.Fill(Mathf.InverseLerp(0f, totalTarget, totalCurrentProgress));

                if (DailyGoalsManager.Instance.DailyGoalsResetTimer != null)
                {
                    DailyGoalsManager.Instance.DailyGoalsResetTimer.RegisterTimerTickEvent(UpdateTaskTimer);
                    DailyGoalsManager.Instance.DailyGoalsResetTimer.RegisterTimerOverEvent(OnTaskTimerOver);

                    UpdateTaskTimer();
                }
            }

            gameplayWinCoinText.text = "+" + GameManager.Instance.GameMainDataSO.levelCompleteReward.GetAmount();
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
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        public void OnButtonClick_Claim()
        {
            Hide();

            AdManager.Instance.ShowInterstitial(InterstatialAdPlaceType.Game_Win_Screen);
            actionToCallOnClaim?.Invoke();
        }
        #endregion
    }
}