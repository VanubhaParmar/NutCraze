using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace com.tag.nut_sort {
    public class LeaderBoardUserPlayer : BaseLeaderBoardPlayer
    {
        #region PUBLIC_VARS
        #endregion

        #region PRIVATE_VARS
        #endregion

        #region KEY
        #endregion

        #region Propertices
        #endregion

        #region Overrided_Method
        public override int GetCurrentPoints()
        {
            return LeaderboardManager.Instance.GetPlayerCurrentScore();
        }
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_FUNCTIONS
        #endregion

        #region PRIVATE_FUNCTIONS
        #endregion

        #region CO-ROUTINES
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}