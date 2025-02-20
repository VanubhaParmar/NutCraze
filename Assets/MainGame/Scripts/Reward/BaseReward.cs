using UnityEngine;

namespace Tag.NutSort
{
    public abstract class BaseReward
    {
        public abstract RewardType GetRewardType();

        public abstract int GetRewardId();

        public abstract int GetAmount();

        public abstract void GiveReward();

        public abstract bool IsEnoughItem();

        public abstract string GetName();

        public abstract Sprite GetRewardImageSprite();

        public abstract BaseReward Clone();

        public virtual void ShowRewardAnimation(CurrencyAnimation animation, Vector3 pos, Transform endPos = null, bool isUiAnimation = true, Sprite itemSprite = null)
        {
        }
    }

    public enum RewardType
    {
        None,
        Currency,
        Boosters
    }
}
