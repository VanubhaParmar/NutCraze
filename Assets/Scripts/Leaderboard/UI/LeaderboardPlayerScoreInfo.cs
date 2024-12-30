using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort
{
    public class LeaderboardPlayerScoreInfo : SerializedMonoBehaviour
    {
        #region PUBLIC_VARIABLES
        public RectTransform MainParent => mainParent;
        public RectTransform GiftBoxParent => giftBoxParent;
        public RectTransform GiftBoxImage => giftBoxImage.rectTransform;
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private RectTransform mainParent;
        [SerializeField] private RectTransform topRankParent;
        [SerializeField] private RectTransform belowRankParent;

        [SerializeField] private Text rankText;
        [SerializeField] private Image rankImage;

        [Space]
        [SerializeField] private Text playerName;
        [SerializeField] private Text playerScore;
        [SerializeField] private RectTransform giftBoxParent;
        [SerializeField] private Image giftBoxImage;

        [Space]
        [SerializeField] private Image mainBGImage;
        [SerializeField] private Image mainScoreBGImage;
        [SerializeField] private Image mainRankBGImage;
        [SerializeField] private List<Text> themeTexts;

        private LeaderboardView LeaderboardView => MainSceneUIManager.Instance.GetView<LeaderboardView>();
        private LeaderBoardPlayerScoreInfoUIData leaderBoardPlayerScoreInfoUIData;

        protected Coroutine rankSetCoroutine;
        private const string Rank_Format = "{0}.";
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        private void OnDisable()
        {
            rankSetCoroutine = null;
        }
        #endregion

        #region PUBLIC_METHODS
        public void SetUI(LeaderBoardPlayerScoreInfoUIData leaderBoardPlayerScoreInfoUIData)
        {
            ResetAllObjects();

            this.leaderBoardPlayerScoreInfoUIData = leaderBoardPlayerScoreInfoUIData;

            if (leaderBoardPlayerScoreInfoUIData.rank <= LeaderboardManager.Max_Top_Rank)
            {
                topRankParent.gameObject.SetActive(true);
                rankImage.sprite = LeaderboardView.RankImages[leaderBoardPlayerScoreInfoUIData.rank - 1];
                giftBoxParent.gameObject.SetActive(true);
                giftBoxImage.sprite = LeaderboardView.GiftboxImages[leaderBoardPlayerScoreInfoUIData.rank - 1];
            }
            else
            {
                belowRankParent.gameObject.SetActive(true);
                rankText.text = string.Format(Rank_Format, leaderBoardPlayerScoreInfoUIData.rank);
            }

            playerName.text = leaderBoardPlayerScoreInfoUIData.name;
            playerScore.text = leaderBoardPlayerScoreInfoUIData.score + "";

            bool isActiveUI = leaderBoardPlayerScoreInfoUIData.leaderboardPlayerType == LeaderboardPlayerType.UserPlayer;

            mainBGImage.sprite = (isActiveUI ? LeaderboardView.ActiveTheme : LeaderboardView.InactiveTheme).mainBGSprite;
            mainScoreBGImage.sprite = (isActiveUI ? LeaderboardView.ActiveTheme : LeaderboardView.InactiveTheme).mainScoreBGSprite;
            mainRankBGImage.sprite = (isActiveUI ? LeaderboardView.ActiveTheme : LeaderboardView.InactiveTheme).mainRankBGSprite;

            themeTexts.ForEach(x => x.color = (isActiveUI ? LeaderboardView.ActiveTheme : LeaderboardView.InactiveTheme).themeColor);

            gameObject.SetActive(true);
        }

        public void ResetView()
        {
            ResetAllObjects();
            gameObject.SetActive(false);
        }

        public void AnimateRank(int from, int to, float animTime = 0.65f)
        {
            if (rankSetCoroutine == null)
                rankSetCoroutine = StartCoroutine(DoAnimateRankValueChange(animTime, from, to, rankText, Rank_Format));
        }
        #endregion

        #region PRIVATE_METHODS
        private void ResetAllObjects()
        {
            topRankParent.gameObject.SetActive(false);
            belowRankParent.gameObject.SetActive(false);
            mainParent.gameObject.SetActive(true);

            giftBoxParent.gameObject.SetActive(false);
        }

        private void SetRankView(int rank)
        {
            topRankParent.gameObject.SetActive(false);
            belowRankParent.gameObject.SetActive(false);
            giftBoxParent.gameObject.SetActive(false);

            if (rank <= LeaderboardManager.Max_Top_Rank)
            {
                topRankParent.gameObject.SetActive(true);
                rankImage.sprite = LeaderboardView.RankImages[rank - 1];
                giftBoxParent.gameObject.SetActive(true);
                giftBoxImage.sprite = LeaderboardView.GiftboxImages[rank - 1];
            }
            else
            {
                belowRankParent.gameObject.SetActive(true);
                rankText.text = string.Format(Rank_Format, rank);
            }
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        private IEnumerator DoAnimateRankValueChange(float time, int startValue, int targetValue, Text textComponent, string format = "{0}")
        {
            float i = 0;
            float rate = 1 / time;

            while (i < 1)
            {
                i += Time.deltaTime * rate;

                SetRankView((int)Mathf.Lerp(startValue, targetValue, i));
                yield return null;
            }
            SetRankView(targetValue);

            rankSetCoroutine = null;
        }
        #endregion

        #region UI_CALLBACKS
        #endregion
    }

    public class LeaderboardPlayerScoreInfoUITheme
    {
        public Color themeColor;

        public Sprite mainBGSprite;
        public Sprite mainScoreBGSprite;
        public Sprite mainRankBGSprite;
    }
}