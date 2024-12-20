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
        public RectTransform GiftBoxParent => giftBoxImage.rectTransform;
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
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
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
                rankText.text = leaderBoardPlayerScoreInfoUIData.rank + ".";
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
        #endregion

        #region PRIVATE_METHODS
        private void ResetAllObjects()
        {
            topRankParent.gameObject.SetActive(false);
            belowRankParent.gameObject.SetActive(false);
            mainParent.gameObject.SetActive(true);

            giftBoxParent.gameObject.SetActive(false);
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
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