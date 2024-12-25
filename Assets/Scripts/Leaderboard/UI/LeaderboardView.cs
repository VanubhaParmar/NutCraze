using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort
{
    public class LeaderboardView : BaseView
    {
        #region PUBLIC_VARIABLES
        public List<Sprite> RankImages => rankImages;
        public List<Sprite> GiftboxImages => giftboxImages;
        public LeaderboardPlayerScoreInfoUITheme InactiveTheme => inactiveTheme;
        public LeaderboardPlayerScoreInfoUITheme ActiveTheme => activeTheme;
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private RectTransform timerParent;
        [SerializeField] private RectTransform timerAndMessageParent;
        [SerializeField] private Text leaderBoardTimerText;
        [SerializeField] private Text messageText;

        [Space]
        [SerializeField] private LeaderboardPlayerScoreInfoUITheme activeTheme;
        [SerializeField] private LeaderboardPlayerScoreInfoUITheme inactiveTheme;
        [SerializeField] private List<Sprite> rankImages;
        [SerializeField] private List<Sprite> giftboxImages;
        [SerializeField] private RectTransform topMostScrollPos;
        [SerializeField] private RectTransform botMostScrollPos;

        [Space]
        [SerializeField] private LeaderboardPlayerScoreInfo leaderboardPlayerScoreInfoPrefab;
        [SerializeField] private ScrollRect leaderboardPlayersListScroll;

        [SerializeField] private LeaderboardPlayerScoreInfo myPlayerScoreCardInfo;
        [SerializeField] private RectTransform followerScrollPos;
        [SerializeField] private ReusableVerticalScrollView reusableVerticalScrollView;

        [ShowInInspector, ReadOnly] private List<LeaderboardPlayerScoreInfo> leaderboardPlayerScoreInfosList = new List<LeaderboardPlayerScoreInfo>();
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        public override void Awake()
        {
            base.Awake();
            myPlayerScoreCardInfo.GetComponent<RectTransformFollower>().heightClamp = new Vector2(botMostScrollPos.transform.position.y, topMostScrollPos.transform.position.y);
        }

        private void OnEnable()
        {
            LeaderboardManager.onLeaderboardEventRunTimerOver += LeaderboardManager_onLeaderboardEventRunTimerOver;
        }

        private void OnDisable()
        {
            LeaderboardManager.onLeaderboardEventRunTimerOver -= LeaderboardManager_onLeaderboardEventRunTimerOver;
            UnregisterLeaderBoardTimer();
        }
        #endregion

        #region PUBLIC_METHODS
        public override void Show(Action action = null, bool isForceShow = false)
        {
            base.Show(action, isForceShow);
            RefreshView();

            MainSceneUIManager.Instance.GetView<BannerAdsView>().Hide();
        }

        public override void OnBackButtonPressed()
        {
            if (!EventSystemHelper.Instance.AreInputsBlocked)
                base.OnBackButtonPressed();
        }

        public override void Hide()
        {
            base.Hide();
            MainSceneUIManager.Instance.GetView<BannerAdsView>().Show(true);
        }
        #endregion

        #region PRIVATE_METHODS
        private void RefreshView()
        {
            bool isEventResult = !LeaderboardManager.Instance.IsCurrentLeaderboardEventActive() && LeaderboardManager.Instance.IsLastEventResultReadyToShow();

            ResetViewInfos();

            var data = LeaderboardManager.Instance.GetLeaderboardPlayerUIDatas();
            SetView(data);

            if (!isEventResult)
            {
                timerParent.gameObject.SetActive(true);

                RegisterLeaderBoardTimer();
                UpdateLeaderBoardTimer();
            }
            else
            {
                messageText.transform.parent.gameObject.SetActive(true);
                messageText.text = "Event Result !";
            }
            timerAndMessageParent.ForceUpdateRectTransforms();
            UIUtilityEvents.RaiseOnRefreshUIRects();

            ScrollToCurrentUser(data.Find(x => x.leaderboardPlayerType == LeaderboardPlayerType.UserPlayer).rank - 1);

            if (isEventResult)
                PlayRewardClaimAnimation();

            LeaderboardManager.Instance.OnLeaderboardViewVisited();
        }

        private void ScrollToCurrentUser(int index)
        {
            Vector3 scrollPos = reusableVerticalScrollView.GetItemWorldPosition(index);
            leaderboardPlayersListScroll.ScrollToRect(scrollPos);
            reusableVerticalScrollView.RefreshVisibility();
        }

        private int CompareFunction(LeaderBoardPlayerScoreInfoUIData a, LeaderBoardPlayerScoreInfoUIData b)
        {
            if (a.score > b.score) return -1;
            if (a.score < b.score) return 1;

            if (a.leaderboardPlayerType == LeaderboardPlayerType.UserPlayer) return -1;
            if (b.leaderboardPlayerType == LeaderboardPlayerType.UserPlayer) return 1;

            return 0;
        }

        private void ResetViewInfos()
        {
            leaderboardPlayerScoreInfosList.ForEach(x => x.ResetView());
            myPlayerScoreCardInfo.gameObject.SetActive(false);

            timerParent.gameObject.SetActive(false);
            messageText.transform.parent.gameObject.SetActive(false);
        }

        private void SetView(List<LeaderBoardPlayerScoreInfoUIData> leaderBoardPlayerScoreInfoUIDatas)
        {
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

        private void RegisterLeaderBoardTimer()
        {
            if (LeaderboardManager.Instance.IsSystemInitialized && LeaderboardManager.Instance.LeaderboardRunTimer != null)
                LeaderboardManager.Instance.LeaderboardRunTimer.RegisterTimerTickEvent(UpdateLeaderBoardTimer);
        }

        private void UpdateLeaderBoardTimer()
        {
            leaderBoardTimerText.text = LeaderboardManager.Instance.LeaderboardRunTimer.GetRemainingTimeSpan().ParseTimeSpan(2);
        }

        private void UnregisterLeaderBoardTimer()
        {
            if (LeaderboardManager.Instance.IsSystemInitialized && LeaderboardManager.Instance.LeaderboardRunTimer != null)
                LeaderboardManager.Instance.LeaderboardRunTimer.UnregisterTimerTickEvent(UpdateLeaderBoardTimer);
        }

        private LeaderboardPlayerScoreInfo GetInactivePlayerScoreInfo()
        {
            return leaderboardPlayerScoreInfosList.Find(x => !x.gameObject.activeInHierarchy);
        }

        private LeaderboardPlayerScoreInfo GenerateNewPlayerScoreInfoView()
        {
            LeaderboardPlayerScoreInfo newView = Instantiate(leaderboardPlayerScoreInfoPrefab, leaderboardPlayersListScroll.content);
            newView.gameObject.SetActive(false);
            leaderboardPlayerScoreInfosList.Add(newView);
            return newView;
        }

        private void LeaderboardManager_onLeaderboardEventRunTimerOver()
        {
            RefreshView();
        }

        private void PlayRewardClaimAnimation()
        {
            int playerRank = LeaderboardManager.Instance.GetPlayerRank();
            if (playerRank > LeaderboardManager.Max_Top_Rank)
                return;

            var itemInfo = reusableVerticalScrollView.GetItemIfVisible(playerRank - 1);
            if (itemInfo == null)
                return;

            EventSystemHelper.Instance.BlockInputs(true);

            Sequence animSequence = DOTween.Sequence();
            animSequence.AppendInterval(2f);
            animSequence.AppendCallback(() => {
                Vector3 giftBoxPosition = itemInfo.GetComponent<LeaderboardPlayerScoreInfo>().GiftBoxImage.transform.position;
                MainSceneUIManager.Instance.GetView<GiftboxRewardAnimationView>().PlayRewardAnimation(giftBoxPosition, 0.55f, playerRank - 1, LeaderboardManager.Instance.GetRankReward(playerRank),
                OnRewardClaimAnimationOver, () => {
                    myPlayerScoreCardInfo.GiftBoxParent.gameObject.SetActive(false);
                });
            });
        }

        private void OnRewardClaimAnimationOver()
        {
            EventSystemHelper.Instance.BlockInputs(false);
            Hide();
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        [Button]
        public void PlayRewardAnimation(int rank)
        {
            ScrollToCurrentUser(rank - 1);
            var itemInfo = reusableVerticalScrollView.GetItemIfVisible(rank - 1);
            if (itemInfo == null)
                return;

            Vector3 giftBoxPosition = itemInfo.GetComponent<LeaderboardPlayerScoreInfo>().GiftBoxImage.transform.position;

            EventSystemHelper.Instance.BlockInputs(true);

            Sequence animSequence = DOTween.Sequence();
            animSequence.AppendInterval(2f);
            animSequence.AppendCallback(() => {
                MainSceneUIManager.Instance.GetView<GiftboxRewardAnimationView>().PlayRewardAnimation(giftBoxPosition, 0.55f, rank - 1, LeaderboardManager.Instance.GetRankReward(rank),
                OnRewardClaimAnimationOver, () => {
                    itemInfo.GetComponent<LeaderboardPlayerScoreInfo>().GiftBoxParent.gameObject.SetActive(false);
                });
            });
        }
        #endregion
    }

    public class LeaderBoardPlayerScoreInfoUIData
    {
        public int rank;
        public int score;
        public string name;
        public LeaderboardPlayerType leaderboardPlayerType;
    }
}