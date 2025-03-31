using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "NutColorThemeTemplateDataSO", menuName = Constant.GAME_NAME + "/Gameplay/Nuts/NutColorThemeTemplateDataSO")]
    public class NutColorThemeTemplateDataSO : SerializedScriptableObject
    {
        #region PUBLIC_VARIABLES
        public List<NutColorThemeInfo> nutColorThemeInfos;
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public NutColorThemeInfo GetNutTheme(int colorId)
        {
            return nutColorThemeInfos.Find(x => x.nutColorId == colorId);
        }
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

    public class NutColorThemeInfo
    {
        [ColorId] public int nutColorId;
        public Color _mainColor;
        public string colorName;
        public float _specularMapIntensity = 0.5f;
        public float _lightIntensity = 1.5f;
    }
}