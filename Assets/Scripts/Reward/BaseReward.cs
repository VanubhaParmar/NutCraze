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

        //public virtual void RemoveReward() { }

        public virtual bool IsEnoughItem() { return false; }

        //public virtual int GetRarity() { return 0; }

        //public virtual void ShowRewardAnimation(CurrencyAnimation animation, Vector3 pos, bool isUiAnimation, Sprite itemSprite = null) { }

        //public virtual BaseReward MultiplyReward(int multiplier) { return new BaseReward(); }

        public virtual string GetName() { return ""; }

        public virtual Sprite GetRewardImageSprite() { return null; }

        //public virtual Sprite GetBackGroundImageSprite() { return null; }

        //public virtual string GetDescription() { return ""; }

        public abstract BaseReward Clone();
    }

    public enum RewardType
    {
        None,
        Currency,
        Boosters
    }
}
