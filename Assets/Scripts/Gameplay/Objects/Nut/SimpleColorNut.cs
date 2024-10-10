using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class SimpleColorNut : BaseNut
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public override void InitNut(BaseNutLevelDataInfo baseNutLevelDataInfo)
        {
            base.InitNut(baseNutLevelDataInfo);
            SetNutColorId();
        }
        #endregion

        #region PRIVATE_METHODS
        private void SetNutColorId()
        {
            var nutColorTheme = LevelManager.Instance.NutColorThemeTemplateDataSO.GetNutColorThemeInfoOfColor(_nutColorId);

            MaterialPropertyBlock props = new MaterialPropertyBlock();
            props.SetColor("_Color", nutColorTheme._mainColor);
            props.SetFloat("_SpecularIntensity", nutColorTheme._specularMapIntensity);
            props.SetFloat("_LightIntensity", nutColorTheme._lightIntensity);
            _nutRenderer.SetPropertyBlock(props);
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }

    public class SimpleColorNutLevelDataInfo : BaseNutLevelDataInfo
    {
    }
}