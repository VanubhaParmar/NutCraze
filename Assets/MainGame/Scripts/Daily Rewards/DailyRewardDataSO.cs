using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace com.tag.nut_sort {
    [CreateAssetMenu(fileName = "DailyRewardDataSO", menuName = Constant.GAME_NAME + "/Daily Rewards/DailyRewardDataSO")]
    public class DailyRewardDataSO : SerializedScriptableObject
    {
        #region PUBLIC_VARS
        public int unlockLevel = 5;
        public List<RewardsDataSO> rewardDataSets;
        #endregion
    }
}
