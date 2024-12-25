using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class CommonSpriteHandler : SerializedManager<CommonSpriteHandler>
    {
        #region PUBLIC_VARIABLES

        public List<CurrencySpritesMapping> currencySpritesMappings;
        public List<BoosterSpritesMapping> boosterSpritesMappings;
        public List<GiftBoxMapping> giftBoxMappings;
        //public Dictionary<int, Sprite> commonRewardBGs = new Dictionary<int, Sprite>();

        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS

        public Sprite GetCurrencySprite(int currencyId)
        {
            return currencySpritesMappings.Find(x => x.currencyId == currencyId).currencySprite;
        }

        public Sprite GetBoosterSprite(BoosterType boosterType)
        {
            return boosterSpritesMappings.Find(x => x.boosterType == boosterType).boosterSprite;
        }

        public GiftBoxMapping GetGiftBoxSprites(int giftboxIndex)
        {
            if (giftboxIndex < 0 || giftboxIndex >= giftBoxMappings.Count)
                return giftBoxMappings[0];

            return giftBoxMappings[giftboxIndex];
        }

        //public Sprite GetCurrencyBackgroundSprite(int currencyId)
        //{
        //    return currencySpritesMappings.Find(x => x.currencyId == currencyId).currencyBGSprite;
        //}

        //public Sprite GetCommonRewardBgSprite(int classIndex)
        //{
        //    if (commonRewardBGs.ContainsKey(classIndex))
        //        return commonRewardBGs[classIndex];
        //    return commonRewardBGs[1];
        //}

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

    public enum GiftboxId
    {
        PURPLE = 0,
        SKY_BLUE = 1,
        GREEN = 2,
        RED = 3
    }

    public class GiftBoxMapping
    {
        public Sprite giftboxFullSprite;
        public Sprite giftboxBotSprite;
        public Sprite giftboxTopSprite;
    }

    public class CurrencySpritesMapping
    {
        [CurrencyId] public int currencyId;
        public Sprite currencySprite;
        //public Sprite currencyBGSprite;
    }

    public class BoosterSpritesMapping
    {
        public BoosterType boosterType;   
        public Sprite boosterSprite;
    }
}