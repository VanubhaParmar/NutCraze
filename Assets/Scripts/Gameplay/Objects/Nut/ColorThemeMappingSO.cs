using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "ColorThemeMappingSO", menuName = Constant.GAME_NAME + "/ScriptableObject/ColorThemeMappingSO")]
    public class ColorThemeMappingSO : SerializedScriptableObject
    {
        #region PRIVATE_VARS
        [SerializeField] private Dictionary<ABTestType, ColorThemeSO> colorThemeABMapping = new Dictionary<ABTestType, ColorThemeSO>();
        #endregion

        #region PUBLIC_VARS
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_FUNCTIONS
        public ColorThemeSO GetColorTheme(ABTestType abTestType)
        {
            if (colorThemeABMapping.ContainsKey(abTestType))
            {
                return colorThemeABMapping[abTestType];
            }
            return colorThemeABMapping[ABTestType.Default];
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

        #region EDITOR
#if UNITY_EDITOR
#endif
        #endregion
    }
}
