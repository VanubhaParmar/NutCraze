using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class BaseLeaderBoardPlayer
    {
        #region PUBLIC_VARS
        public LeaderboardPlayerType leaderboardPlayerType;
        #endregion

        #region PRIVATE_VARS
        [ShowInInspector, ReadOnly] protected string playerName;
        private LeaderBoardPlayerScoreInfoUIData leaderBoardPlayerScoreInfoUIData;
        #endregion

        #region KEY
        #endregion

        #region Propertices
        #endregion

        #region Overrided_Method
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_FUNCTIONS
        public virtual void Init(string playerName, LeaderboardPlayerType leaderboardPlayerType = LeaderboardPlayerType.BotPlayer)
        {
            this.playerName = playerName;
            this.leaderboardPlayerType = leaderboardPlayerType;

            InitializeLeaderboardPlayerScoreInfoUIData();
        }
        public virtual string GetPlayerName()
        {
            return playerName;
        }
        //public virtual Sprite GetPlayerProfile()
        //{
        //    return null;
        //}
        public virtual int GetCurrentPoints()
        {
            return 0;
        }
        public virtual bool IsUserPlayer()
        {
            return leaderboardPlayerType == LeaderboardPlayerType.UserPlayer;
        }
        public virtual LeaderBoardPlayerScoreInfoUIData GetLeaderboardPlayerScoreInfoUIData()
        {
            if (leaderBoardPlayerScoreInfoUIData == null)
                InitializeLeaderboardPlayerScoreInfoUIData();

            leaderBoardPlayerScoreInfoUIData.score = GetCurrentPoints();
            return leaderBoardPlayerScoreInfoUIData;
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        private void InitializeLeaderboardPlayerScoreInfoUIData()
        {
            leaderBoardPlayerScoreInfoUIData = new LeaderBoardPlayerScoreInfoUIData();
            leaderBoardPlayerScoreInfoUIData.leaderboardPlayerType = leaderboardPlayerType;
            leaderBoardPlayerScoreInfoUIData.name = playerName;
            leaderBoardPlayerScoreInfoUIData.score = GetCurrentPoints();
        }
        #endregion

        #region CO-ROUTINES
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
    public enum LeaderboardPlayerType
    {
        UserPlayer,
        BotPlayer
    }
}