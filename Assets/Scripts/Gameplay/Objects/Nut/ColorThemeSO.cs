using Sirenix.OdinInspector;
using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "ColorThemeSO", menuName = Constant.GAME_NAME + "/ScriptableObject/ColorThemeSO")]
    public class ColorThemeSO : SerializedScriptableObject
    {
        #region PRIVATE_VARS
        [SerializeField] private ColorThemeConfig[] colorThemeConfigs;
        #endregion

        #region PUBLIC_VARS
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_FUNCTIONS
        public ColorThemeConfig GetColorTheme(int colorId)
        {
            return colorThemeConfigs[colorId];
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

    public class ColorThemeConfig
    {
        [ColorId] public int nutColorId;
        public Color _mainColor;
        public string colorName;
        public float _specularMapIntensity = 0.5f;
        public float _lightIntensity = 1.5f;
    }
}
