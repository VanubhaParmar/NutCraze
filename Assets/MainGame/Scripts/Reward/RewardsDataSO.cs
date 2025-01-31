using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace com.tag.nut_sort
{
    [CreateAssetMenu(fileName = "RewardsDataSO", menuName = Constant.GAME_NAME + "/Rewards/RewardsDataSO")]
    public class RewardsDataSO : SerializedScriptableObject
    {
        #region PUBLIC_VARIABLES
        public List<BaseReward> rewards;
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void GiveRewards()
        {
            rewards.ForEach(x => x.GiveReward());
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
}