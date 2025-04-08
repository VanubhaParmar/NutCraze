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
        private Dictionary<ABTestType, LevelVariantSO> abVariantDictionary = new Dictionary<ABTestType, LevelVariantSO>();
        #endregion

        #region PRIVATE_METHODS

        private bool ValidateDefaultVariant(Dictionary<ABTestType, LevelVariantSO> dict)
        {
            return dict != null && dict.ContainsKey(ABTestType.Default);
        }

        #endregion

        #region PUBLIC_METHODS
        public bool IsVariantExist(ABTestType type)
        {
            return abVariantDictionary.ContainsKey(type);
        }

        public List<ABTestType> GetAvailableABVariants()
        {
            return new List<ABTestType>(abVariantDictionary.Keys);
        }

        public bool TryGetLevelVariant(ABTestType currentAbType, out LevelVariantSO levelVariant)
        {
            return abVariantDictionary.TryGetValue(currentAbType, out levelVariant);
        }
        #endregion
    }


}