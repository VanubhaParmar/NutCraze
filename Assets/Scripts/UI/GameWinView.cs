using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private CanvasGroup bottomButtonsViewCanvasGroup;
        [SerializeField] private GameObject dailyTaskCompleteParent;

        [Space]
        [SerializeField] private Animator animator;
        [SerializeField, AnimatorStateName(animatorField: "animator")]
        private string dailyBonusGiftAnimation;
        [SerializeField, AnimatorStateName(animatorField: "animator")]
        private string dailyBonusGiftOutAnimation;
        [SerializeField, AnimatorStateName(animatorField: "animator")]
        private string dailyBonusInAnimation;
        [SerializeField] private Animation claimButtonAnimation;
        [SerializeField, AnimationName(animationField: "claimButtonAnimation")]
        private string bottomButtonsInAnimation;

        [Space, Header("Leaderboard Progress View")]
        [SerializeField] private CanvasGroup leaderboardViewCanvasGroup;
        [SerializeField] private LeaderboardPlayerScoreInfo leaderboardPlayerScoreInfoPrefab;
        [SerializeField] private ScrollRect leaderboardPlayersListScroll;

        [SerializeField] private LeaderboardPlayerScoreInfo myPlayerScoreCardInfo;
        [SerializeField] private RectTransform followerScrollPos;
        [SerializeField] private ReusableVerticalScrollView reusableVerticalScrollView;

        [Space]
        [SerializeField] private AnimationCurve leaderboardTranslateAnimationCurve;
        [SerializeField] private AnimationCurve leaderboardViewScaleDownAnimationCurve;
        [SerializeField] private float leaderboardViewScaleDownAnimationTime;
        [SerializeField] private float leaderboardTranslateAnimationTimePerView;
        [SerializeField] private Vector2 leaderboardTranslateAnimationTimeRange;
        [SerializeField] private float leaderboardViewScaleAnimationTime;
        [SerializeField] private float leaderboardViewTranslateAnimationTimePerUnit;

        private Action actionToCallOnClaim;

        private int coinTarget;
        private bool areAllDailyTasksCompleted;
        private BaseReward levelCompleteReward;
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
            this.levelCompleteReward = GameManager.Instance.GameMainDataSO.levelCompleteReward;
            GameStatsCollector.Instance.OnPopUpTriggered(GameStatPopUpTriggerType.SYSTEM_TRIGGERED);
            Show();
            SetView();

            MainSceneUIManager.Instance.GetView<VFXView>().CoinAnimation.RegisterObjectAnimationComplete(UpdateCoinText);
            MainSceneUIManager.Instance.GetView<BannerAdsView>().Show(false);
        }

        public override void Hide()
        {
            MainSceneUIManager.Instance.GetView<VFXView>().CoinAnimation.DeregisterObjectAnimationComplete(UpdateCoinText);
            MainSceneUIManager.Instance.GetView<VFXView>().CoinAnimation.DeregisterObjectAnimationComplete(HideViewOnLastCoinCollect);
            base.Hide();
        }

        public override void OnViewShowDone()
        {
            base.OnViewShowDone();
            OnGameWinAnimationDone();
        }
        #endregion

        #region PRIVATE_METHODS
        private void OnGameWinAnimationDone()
        {
            CheckForLeaderboardProgressAnimation();
        }

        private void AnimationDailyGoalsIn()
        {
            EventSystemHelper.Instance.BlockInputs(true);

            float claimWaitTime = animator.GetAnimatorClipLength(dailyBonusInAnimation) * (DailyGoalsProgressHelper.IsAnyProgress() ? 1f : 0.5f);

            Sequence inSequence = DOTween.Sequence();
            inSequence.AppendCallback(() => { animator.Play(dailyBonusInAnimation); });
            inSequence.AppendInterval(claimWaitTime);
            inSequence.onComplete += OnDailyGoalsInAnimationsDone;
        }

        private void OnDailyGoalsInAnimationsDone()
        {
            EventSystemHelper.Instance.BlockInputs(false);
            if (DailyGoalsProgressHelper.IsAnyProgress())
                PlayDailyTaskProgressAnimation();
            else
                AnimationBottomClaimButton();
        }

        private void AnimationBottomClaimButton()
        {
            EventSystemHelper.Instance.BlockInputs(true);
            Sequence inSequence = DOTween.Sequence();
            inSequence.AppendCallback(() => { claimButtonAnimation.Play(bottomButtonsInAnimation); });
            inSequence.AppendInterval(claimButtonAnimation.GetAnimationClipLength(bottomButtonsInAnimation));
            inSequence.InsertCallback(0.05f, () => { bottomButtonsViewCanvasGroup.alpha = 1f; });
            inSequence.onComplete += OnAllAnimationsDone;
        }

        private void OnAllAnimationsDone()
        {
            claimButton.interactable = true;
            EventSystemHelper.Instance.BlockInputs(false);
        }

        private void SetView()
        {
            int currentCoins = DataManager.Instance.GetCurrency(CurrencyConstant.COIN).Value;

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
            coinTarget = currentCoins - totalCoinsRewards;

            dailyGoalsCoinRewardText.text = "+" + DailyGoalsManager.Instance.DailyGoalsSystemDataSO.allTaskCompleteReward.GetAmount();
            bottomButtonsViewCanvasGroup.alpha = 0f;
        }

        private void CheckForLeaderboardProgressAnimation()
        {
            var leaderboardTracker = LeaderboardManager.Instance.LeaderBoardProgressTracker;
            if (leaderboardTracker != null && leaderboardTracker.HasMadeProgress())
                OnPlayLeaderboardProgressAnimation();
            else
                OnLeaderboardProgressAnimationDone();
        }

        private void OnPlayLeaderboardProgressAnimation()
        {
            var leaderboardTracker = LeaderboardManager.Instance.LeaderBoardProgressTracker;
            var datas = LeaderboardManager.Instance.GetLeaderboardPlayerUIDatas();

            // Pre-allocate list with capacity to avoid resizing
            var oldData = new List<LeaderBoardPlayerScoreInfoUIData>(datas.Count);

            // Use foreach instead of LINQ to reduce allocations
            foreach (var data in datas)
            {
                oldData.Add(data.DeepCopy());
            }

            // Update ranks
            for (int i = leaderboardTracker.currentLeaderboardKnownPosition - 1;
                 i <= leaderboardTracker.lastLeaderboardKnownPosition - 1; i++)
            {
                if (oldData[i].leaderboardPlayerType != LeaderboardPlayerType.UserPlayer)
                    oldData[i].rank--;
                else
                    oldData[i].rank = leaderboardTracker.lastLeaderboardKnownPosition;
            }

            // Manual sort to avoid LINQ allocations
            oldData.Sort(CompareLeaderboardData);

            SetLeaderboardView(oldData);

            // Cache user player index to avoid multiple searches
            int userPlayerIndex = -1;
            for (int i = 0; i < oldData.Count; i++)
            {
                if (oldData[i].leaderboardPlayerType == LeaderboardPlayerType.UserPlayer)
                {
                    userPlayerIndex = i;
                    break;
                }
            }

            Vector3 scrollPos = reusableVerticalScrollView.GetItemWorldPosition(userPlayerIndex);
            leaderboardPlayersListScroll.ScrollToRect(scrollPos);
            reusableVerticalScrollView.RefreshVisibility();

            myPlayerScoreCardInfo.transform.position = followerScrollPos.transform.position;
            myPlayerScoreCardInfo.transform.localScale = Vector3.one;

            PlayLeaderboardAnimationSequence(leaderboardTracker);
        }

        // Separate comparison method to avoid allocations from lambda
        private int CompareLeaderboardData(LeaderBoardPlayerScoreInfoUIData x, LeaderBoardPlayerScoreInfoUIData y)
        {
            if (x.rank > y.rank) return 1;
            if (y.rank > x.rank) return -1;

            if (x.leaderboardPlayerType == LeaderboardPlayerType.UserPlayer) return -1;
            if (y.leaderboardPlayerType == LeaderboardPlayerType.UserPlayer) return 1;

            return 0;
        }

        private void PlayLeaderboardAnimationSequence(LeaderBoardProgressTracker leaderboardTracker)
        {
            EventSystemHelper.Instance.BlockInputs(true);
            Sequence inSequence = DOTween.Sequence();

            int viewTraslateDifference = Mathf.Abs(leaderboardTracker.currentLeaderboardKnownPosition -
                                                 leaderboardTracker.lastLeaderboardKnownPosition);

            float scrollViewTranslateTime = Mathf.Clamp(
                leaderboardTranslateAnimationTimePerView * viewTraslateDifference,
                leaderboardTranslateAnimationTimeRange.x,
                leaderboardTranslateAnimationTimeRange.y
            );

            Vector3 scrollPosEnd = reusableVerticalScrollView.GetItemWorldPosition(
                leaderboardTracker.currentLeaderboardKnownPosition - 1
            );

            var targetPos = leaderboardPlayersListScroll.GetTargetPositionScrollToRect(scrollPosEnd);

            scrollViewTranslateTime *= Mathf.Abs(targetPos.y -
                leaderboardPlayersListScroll.content.anchoredPosition.y) > 0.05f ? 1 : 0;

            leaderboardViewCanvasGroup.alpha = 0f;

            inSequence.AppendInterval(0.2f)
                      .Append(leaderboardViewCanvasGroup.DOFade(1f, 0.5f))
                      .AppendInterval(0.4f)
                      .Join(myPlayerScoreCardInfo.transform.GetChild(0).DOScale(Vector3.one * 1.2f, leaderboardViewScaleAnimationTime))
                      .AppendCallback(() => StartScrollAnimation(scrollViewTranslateTime, targetPos, leaderboardTracker))
                      .AppendInterval(scrollViewTranslateTime)
                      .onComplete += PlayViewMoveAndPlaceSequence;
        }

        private void StartScrollAnimation(float scrollViewTranslateTime, Vector2 targetPos,
            LeaderBoardProgressTracker leaderboardTracker)
        {
            Vector3DotweenerAnimation scrollAnimation = new Vector3DotweenerAnimation(
                leaderboardPlayersListScroll.content.anchoredPosition,
                targetPos,
                scrollViewTranslateTime,
                leaderboardTranslateAnimationCurve
            );

            scrollAnimation.StartDotweenAnimation(
                leaderboardPlayersListScroll.content,
                (x) => { leaderboardPlayersListScroll.content.anchoredPosition = x; }
            );

            myPlayerScoreCardInfo.AnimateRank(
                leaderboardTracker.lastLeaderboardKnownPosition,
                leaderboardTracker.currentLeaderboardKnownPosition,
                scrollViewTranslateTime
            );
        }

        private void PlayViewMoveAndPlaceSequence()
        {
            var datas = LeaderboardManager.Instance.GetLeaderboardPlayerUIDatas();
            var leaderboardTracker = LeaderboardManager.Instance.LeaderBoardProgressTracker;
            var translateRect = myPlayerScoreCardInfo.GetComponent<RectTransform>();

            Vector3 scrollPosEnd = reusableVerticalScrollView.GetItemWorldPosition(leaderboardTracker.currentLeaderboardKnownPosition - 1);
            float playerDataViewTranslateTime = Mathf.CeilToInt(Vector3.Distance(scrollPosEnd, translateRect.position)) * leaderboardViewTranslateAnimationTimePerUnit;
            playerDataViewTranslateTime *= Vector3.Distance(scrollPosEnd, translateRect.position) > 0.05f ? 1 : 0;

            //Debug.Log("Player View Translate Time : " + playerDataViewTranslateTime);

            Sequence viewMoveAndPlaceSequence = DOTween.Sequence();
            viewMoveAndPlaceSequence.AppendCallback(() =>
            {
                //Debug.Log("Player view Anim Start : " + Time.time);
                translateRect.DOMove(scrollPosEnd, playerDataViewTranslateTime);
            });
            viewMoveAndPlaceSequence.AppendInterval(playerDataViewTranslateTime);
            viewMoveAndPlaceSequence.AppendCallback(() =>
            {
                //Debug.Log("Other views Anim Start : " + Time.time);
                for (int i = leaderboardTracker.currentLeaderboardKnownPosition - 1; i < leaderboardTracker.lastLeaderboardKnownPosition - 1; i++)
                {
                    int index = i;
                    var currentViewOnIndex = reusableVerticalScrollView.GetItemIfVisible(i);
                    if (currentViewOnIndex != null)
                    {
                        var targetPos = reusableVerticalScrollView.GetItemPosition(i + 1);

                        Sequence moveSequence = DOTween.Sequence();
                        moveSequence.Append(currentViewOnIndex.DOAnchorPos(targetPos, leaderboardViewTranslateAnimationTimePerUnit));
                        moveSequence.InsertCallback(leaderboardViewTranslateAnimationTimePerUnit * 0.7f, () =>
                        {
                            currentViewOnIndex.GetComponent<LeaderboardPlayerScoreInfo>().SetUI(datas[index + 1]);
                        });
                    }
                    else
                        break;
                }
            });
            viewMoveAndPlaceSequence.AppendInterval(0.2f);
            viewMoveAndPlaceSequence.AppendCallback(() =>
            {
                //Debug.Log("Player view Scale Anim Start : " + Time.time);
                var mainAnimTransform = myPlayerScoreCardInfo.transform.GetChild(0);
                Vector3DotweenerAnimation scaleAnimation = new Vector3DotweenerAnimation(mainAnimTransform.localScale, Vector3.one, leaderboardViewScaleDownAnimationTime, leaderboardViewScaleDownAnimationCurve);
                scaleAnimation.StartDotweenAnimation(myPlayerScoreCardInfo.transform, (x) => { mainAnimTransform.localScale = x; });
                myPlayerScoreCardInfo.SetUI(datas.Find(x => x.leaderboardPlayerType == LeaderboardPlayerType.UserPlayer));
            });
            viewMoveAndPlaceSequence.AppendInterval(0.5f + leaderboardViewScaleDownAnimationTime);
            viewMoveAndPlaceSequence.Append(leaderboardViewCanvasGroup.DOFade(0f, 0.5f));
            viewMoveAndPlaceSequence.onComplete += OnLeaderboardProgressAnimationDone;
        }

        private void SetLeaderboardView(List<LeaderBoardPlayerScoreInfoUIData> leaderBoardPlayerScoreInfoUIDatas)
        {
            var leaderboardTracker = LeaderboardManager.Instance.LeaderBoardProgressTracker;
            Debug.Log("Leaderboard Rank UP : " + leaderboardTracker.lastLeaderboardKnownPosition + " - " + leaderboardTracker.currentLeaderboardKnownPosition);

            reusableVerticalScrollView.Initialize(leaderBoardPlayerScoreInfoUIDatas.Count, (gameObject, index) =>
            {
                LeaderboardPlayerScoreInfo scoreInfo = gameObject.GetComponent<LeaderboardPlayerScoreInfo>();
                scoreInfo.SetUI(leaderBoardPlayerScoreInfoUIDatas[index]);

                if (leaderBoardPlayerScoreInfoUIDatas[index].leaderboardPlayerType == LeaderboardPlayerType.UserPlayer)
                    scoreInfo.MainParent.gameObject.SetActive(false);
            });

            var myViewData = leaderBoardPlayerScoreInfoUIDatas.Find(x => x.leaderboardPlayerType == LeaderboardPlayerType.UserPlayer);
            Vector3 positionOfMyView = reusableVerticalScrollView.GetItemPosition(leaderBoardPlayerScoreInfoUIDatas.IndexOf(myViewData));
            followerScrollPos.anchoredPosition = positionOfMyView;

            myPlayerScoreCardInfo.SetUI(myViewData);
        }

        private void OnLeaderboardProgressAnimationDone()
        {
            AnimationDailyGoalsIn();
        }

        private void SetInitialDailyTaskView()
        {
            areAllDailyTasksCompleted = false;

            float totalTarget = 0f;
            float totalCurrentProgress = 0f;

            float totalRealProgress = 0f;

            for (int i = 0; i < DailyGoalsManager.Instance.DailyGoals.Count; i++)
            {
                int initialProgress = Mathf.Max(0, DailyGoalsManager.Instance.DailyGoals[i].dailyGoalCurrentProgress - DailyGoalsProgressHelper.GetTaskProgress(DailyGoalsManager.Instance.DailyGoals[i].dailyGoalsTaskType));
                totalRealProgress += DailyGoalsManager.Instance.DailyGoals[i].dailyGoalCurrentProgress;

                dailyGoalTaskUIViews[i].InitializeDailyGoalTaskView(DailyGoalsManager.Instance.DailyGoals[i]);
                dailyGoalTaskUIViews[i].SetViewProgress(initialProgress, initialProgress == DailyGoalsManager.Instance.DailyGoals[i].dailyGoalTargetCount);

                totalTarget += DailyGoalsManager.Instance.DailyGoals[i].dailyGoalTargetCount;
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
            dailyTaskCompleteParent.SetActive(true);

            Sequence completeSeq = DOTween.Sequence();
            completeSeq.AppendCallback(() => {
                animator.Play(dailyBonusGiftAnimation);
                dailyTaskGiftboxImageParent.gameObject.SetActive(false);
            });
            completeSeq.InsertCallback(animator.GetAnimationLength(dailyBonusGiftAnimation) * 0.5f, () => {
                SoundHandler.Instance.PlaySound(SoundType.GiftboxOpen);
            });
            completeSeq.AppendInterval(animator.GetAnimationLength(dailyBonusGiftAnimation));
            completeSeq.AppendCallback(() => {
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
        }

        private void OnDailyTaskAllAnimationCompleted()
        {
            dailyTaskCompleteParent.SetActive(false);
            EventSystemHelper.Instance.BlockInputs(false);
            DailyGoalsProgressHelper.ResetProgress();

            dailyTaskGiftboxImageParent.gameObject.SetActive(true);
            if (areAllDailyTasksCompleted)
                dailyTaskGiftboxClaimedParent.gameObject.SetActive(true);

            AnimationBottomClaimButton();
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

        private void HideViewOnLastCoinCollect(int value, bool isLastCoin)
        {
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

            coinTarget = -1; // sets current value of coins
            MainSceneUIManager.Instance.GetView<VFXView>().CoinAnimation.RegisterObjectAnimationComplete(HideViewOnLastCoinCollect);
            MainSceneUIManager.Instance.GetView<VFXView>().PlayCoinAnimation(gameplayWinCoinText.transform.position, levelCompleteReward.GetAmount(), coinTopBar.CurrencyImage.transform);
        }
        #endregion
    }
}