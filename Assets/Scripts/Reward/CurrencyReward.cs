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

        //public override void RemoveReward()
        //{
        //    DataManager.Instance.GetCurrency(currencyID).Add(-curruncyValue);
        //}

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
            return CommonSpriteHandler.Instance.GetCurrencySprite(currencyID);
        }

        //public override Sprite GetBackGroundImageSprite()
        //{
        //    return CommonSpriteHandler.Instance.GetCurrencyBackgroundSprite(currencyID);
        //}

        public override string GetName()
        {
            return DataManager.Instance.GetCurrency(currencyID).currencyName;
        }

        //public override BaseReward MultiplyReward(int multiplier)
        //{
        //    return new CurrencyReward { currencyID = currencyID, curruncyValue = curruncyValue * multiplier };
        //}

        //public override void ShowRewardAnimation(CurrencyAnimation animation, Vector3 pos, bool isUiAnimation, Sprite itemSprite = null)
        //{
        //    if (animation == null)
        //        return;
        //    if (isUiAnimation)
        //    {
        //        animation.UIStartAnimation(pos, curruncyValue, false);
        //    }
        //    else
        //    {
        //        if (itemSprite == null)
        //            animation.StartAnimation(pos, curruncyValue);
        //        else
        //            animation.StartAnimation(pos, curruncyValue, itemSprite);
        //    }
        //}

        public override BaseReward Clone()
        {
            return new CurrencyReward { currencyID = currencyID, currencyValue = currencyValue };
        }
    }
}
