using UnityEngine;

namespace Tag.NutSort
{
    public class BoosterReward : BaseReward
    {
        #region PUBLIC_VARIABLES
        [BoosterId]public int rewardBoosterType;
        public int rewardAmount;
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public override void GiveReward()
        {
            DataManager.Instance.AddBoosters(rewardBoosterType, rewardAmount);
        }

        public override Sprite GetRewardImageSprite()
        {
            return ResourceManager.Instance.GetBoosterSprite(rewardBoosterType);
        }

        public override int GetAmount()
        {
            return rewardAmount;
        }

        public override RewardType GetRewardType()
        {
            return RewardType.Boosters;
        }

        public override int GetRewardId()
        {
            return (int)rewardBoosterType;
        }

        public override bool IsEnoughItem()
        {
            return GetAmount() <= DataManager.Instance.GetBooster(rewardBoosterType).Value;
        }

        public override string GetName()
        {
            return rewardBoosterType.ToString();
        }

        public override BaseReward Clone()
        {
            return new BoosterReward() { rewardBoosterType = rewardBoosterType, rewardAmount = rewardAmount };
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