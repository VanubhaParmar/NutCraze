using UnityEngine;

namespace Tag.NutSort
{
    public class CurrencyReward : BaseReward
    {
        [CurrencyId] public int currencyID;
        public int currencyValue;

        public override void GiveReward()
        {
            DataManager.Instance.GetCurrency(currencyID).Add(currencyValue);
        }

        public override int GetAmount()
        {
            return currencyValue;
        }

        public override RewardType GetRewardType()
        {
            return RewardType.Currency;
        }

        public override int GetRewardId()
        {
            return currencyID;
        }

        public override bool IsEnoughItem()
        {
            return GetAmount() <= DataManager.Instance.GetCurrency(currencyID).Value;
        }

        public override Sprite GetRewardImageSprite()
        {
            return ResourceManager.Instance.GetCurrencySprite(currencyID);
        }

        public override string GetName()
        {
            return DataManager.Instance.GetCurrency(currencyID).currencyName;
        }

        public override BaseReward Clone()
        {
            return new CurrencyReward { currencyID = currencyID, currencyValue = currencyValue };
        }

        public override void ShowRewardAnimation(CurrencyAnimation animation,Vector3 pos, Transform endPos = null, bool isUiAnimation = true, Sprite itemSprite = null)
        {
            if (animation == null)
                return;

            if (isUiAnimation)
            {
                animation.UIStartAnimation(pos, endPos, currencyValue, false);
            }
            else
            {
                if (itemSprite == null)
                    animation.StartAnimation(pos, currencyValue, null);
                else
                    animation.StartAnimation(pos, currencyValue, itemSprite);
            }
        }
    }
}
