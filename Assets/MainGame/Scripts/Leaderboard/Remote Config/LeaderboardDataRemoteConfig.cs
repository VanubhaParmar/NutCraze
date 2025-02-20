using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort {
    [CreateAssetMenu(fileName = "LeaderboardDataRemoteConfig", menuName = Constant.GAME_NAME + "/Remote Config Data/LeaderboardDataRemoteConfig")]
    public class LeaderboardDataRemoteConfig : BaseConfig
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private LeaderboardData leaderboardData;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public override string GetDefaultString()
        {
            LeaderBoardRemoteConfigInfo leaderBoardRemoteConfigData = new LeaderBoardRemoteConfigInfo();
            leaderBoardRemoteConfigData.startAtLevel = leaderboardData.startAtLevel;
            leaderBoardRemoteConfigData.startDay = (int)leaderboardData.startDay;
            leaderBoardRemoteConfigData.leaderboardRunTimeInDays = leaderboardData.leaderboardRunTimeInDays;

            return SerializeUtility.SerializeObject(leaderBoardRemoteConfigData);
        }
        #endregion

        #region PRIVATE_METHODS
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }

    public class LeaderBoardRemoteConfigInfo
    {
        public int startAtLevel;
        public int startDay;
        public int leaderboardRunTimeInDays;
    }
}