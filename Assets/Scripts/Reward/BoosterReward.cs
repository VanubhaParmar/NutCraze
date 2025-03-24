using UnityEngine;

namespace Tag.NutSort
{
    public class BoosterReward : BaseReward
    {
        #region PUBLIC_VARIABLES
        [BoosterId] public int boosterId;
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
            DataManager.Instance.AddBoosters(boosterId, rewardAmount);
        }

        public override Sprite GetRewardImageSprite()
        {
            return ResourceManager.Instance.GetBoosterSprite(boosterId);
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
            return boosterId;
        }

        public override bool IsEnoughItem()
        {
            return GetAmount() <= DataManager.Instance.GetBoostersCount(boosterId);
        }

        public override BaseReward Clone()
        {
            return new BoosterReward() { boosterId = boosterId, rewardAmount = rewardAmount };
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