using UnityEngine;

namespace Tag.NutSort
{
    public abstract class BaseReward
    {
        public virtual RewardType GetRewardType()
        {
            return RewardType.Currency;
        }

        public virtual int GetRewardId() { return 0; }

        public virtual int GetAmount() { return 0; }

        public virtual void GiveReward() { }

        public virtual bool IsEnoughItem() { return false; }

        public virtual string GetName() { return ""; }

        public virtual Sprite GetRewardImageSprite() { return null; }

        public abstract BaseReward Clone();
    }

    public enum RewardType
    {
        None,
        Currency,
        Boosters
    }
}
