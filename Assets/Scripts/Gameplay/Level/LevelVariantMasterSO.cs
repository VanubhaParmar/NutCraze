using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "LevelVariantMaster", menuName = Constant.GAME_NAME + "/ABTesting/Level Variant Master")]
    public class LevelVariantMasterSO : SerializedScriptableObject
    {
        #region PRIVATE_VARIBALES

        [Header("AB Variant to Level Variant Mapping")]
        [DictionaryDrawerSettings(KeyLabel = "AB Type", ValueLabel = "Level Variant")]
        [SerializeField, Required("Dictionary cannot be null")]
        [ValidateInput("ValidateDefaultVariant", "Must contain Default AB Type variant!", InfoMessageType.Warning)]
        private Dictionary<LevelABTestType, LevelVariantSO> abVariantDictionary = new Dictionary<LevelABTestType, LevelVariantSO>();
        #endregion

        #region PRIVATE_METHODS

        private bool ValidateDefaultVariant(Dictionary<LevelABTestType, LevelVariantSO> dict)
        {
            return dict != null && dict.ContainsKey(LevelABTestType.Default);
        }

        #endregion

        #region PUBLIC_METHODS
        public bool IsVariantExist(LevelABTestType type)
        {
            return abVariantDictionary.ContainsKey(type);
        }

        public List<LevelABTestType> GetAvailableABVariants()
        {
            return new List<LevelABTestType>(abVariantDictionary.Keys);
        }

        public LevelVariantSO GetLevelVariant(LevelABTestType currentAbType)
        {
            if (abVariantDictionary.ContainsKey(currentAbType))
                return abVariantDictionary[currentAbType];
            return abVariantDictionary[LevelABTestType.Default];
        }

        public bool TryGetLevelVariant(LevelABTestType currentAbType, out LevelVariantSO levelVariant)
        {
            return abVariantDictionary.TryGetValue(currentAbType, out levelVariant);
        }
        #endregion
    }


}