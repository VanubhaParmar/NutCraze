using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class ResourceManager : SerializedManager<ResourceManager>
    {
        #region PRIVATE_VARS
        [SerializeField] private LevelVariantMasterSO levelVariantMasterSO;
        [SerializeField] private ScrewContainer screwContainer;
        [SerializeField] private NutContainer nutContainer;
        [SerializeField] private GameObject bigConfettiPsPrefab;
        [SerializeField] private ParticleSystem _stackFullIdlePS;
        [SerializeField] private ParticleSystem screwEndParticle;
        [SerializeField] private List<GiftBoxMapping> giftBoxMappings;
        [SerializeField] private Dictionary<int, Sprite> currencySprite = new Dictionary<int, Sprite>();
        [SerializeField] private Dictionary<int, Sprite> boosterSprite = new Dictionary<int, Sprite>();
        [SerializeField] private List<ScrewParticalSystemConfig> _screwParticleSystemsConfig;
        #endregion

        #region PUBLIC_VARS
        #endregion

        #region PROPERTIES
        public static GameObject BigConfettiPsPrefab => Instance.bigConfettiPsPrefab;
        public static ParticleSystem StackFullIdlePsPrefab => Instance._stackFullIdlePS;
        public static ParticleSystem ScrewEndParticle => Instance.screwEndParticle;
        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS
        public bool IsVariantExist(ABTestType aBTestType)
        {
            return levelVariantMasterSO.IsVariantExist(aBTestType);
        }

        public void GetLevelVariant(ABTestType currentAbType, out ABTestType resultAbType, out LevelVariantSO levelVariant)
        {
            levelVariantMasterSO.GetLevelVariant(currentAbType, out resultAbType, out levelVariant);
        }

        public Sprite GetCurrencySprite(int currencyID)
        {
            if (currencySprite.ContainsKey(currencyID))
            {
                return currencySprite[currencyID];
            }
            return null;
        }

        public Sprite GetBoosterSprite(int boosterType)
        {
            if (boosterSprite.ContainsKey(boosterType))
            {
                return boosterSprite[boosterType];
            }
            return null;
        }

        public GiftBoxMapping GetGiftBoxSprites(int giftboxIndex)
        {
            if (giftboxIndex < 0 || giftboxIndex >= giftBoxMappings.Count)
                return giftBoxMappings[0];

            return giftBoxMappings[giftboxIndex];
        }

        public BaseScrew GetScrew(int screwType)
        {
            return screwContainer.GetScrew(screwType);
        }

        public BaseNut GetNut(int nutType)
        {
            return nutContainer.GetNut(nutType);
        }

        public GameObject GetStackFullParticlesByID(int colorId)
        {
            return _screwParticleSystemsConfig.Find(x => x.nutColorId == colorId).particleSystem;
        }
        #endregion

        #region PRIVATE_FUNCTIONS

        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       

        #endregion
    }

    public class GiftBoxMapping
    {
        public Sprite giftboxFullSprite;
        public Sprite giftboxBotSprite;
        public Sprite giftboxTopSprite;
    }

    [Serializable]
    public class ScrewParticalSystemConfig
    {
        [ColorId] public int nutColorId;
        public GameObject particleSystem;
    }
}
