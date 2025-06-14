using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class SurpriseColorNut : BaseNut
    {
        #region PUBLIC_VARIABLES
        public SurpriseColorNutState SurpriseColorNutState => _surpriseColorNutState;
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField, NutColorId] private int surpriseColorId;
        [SerializeField, ReadOnly] private SurpriseColorNutState _surpriseColorNutState;
        [SerializeField] private GameObject undefinedObjectTransform;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public override void InitNut(BaseNutLevelDataInfo baseNutLevelDataInfo)
        {
            //base.InitNut(baseNutLevelDataInfo);

            this.baseNutLevelDataInfo = baseNutLevelDataInfo;
            _nutColorId = baseNutLevelDataInfo.nutColorTypeId;

            SetNutColorId(surpriseColorId);
            _surpriseColorNutState = SurpriseColorNutState.COLOR_NOT_REVEALED;
            undefinedObjectTransform.gameObject.SetActive(true);
        }

        public void OnRevealColorOfNut()
        {
            _surpriseColorNutState = SurpriseColorNutState.COLOR_REVEALED;
            SetNutColorId(_nutColorId);
            undefinedObjectTransform.gameObject.SetActive(false);
        }

        public override int GetNutColorType()
        {
            return _surpriseColorNutState == SurpriseColorNutState.COLOR_REVEALED ? base.GetNutColorType() : surpriseColorId;
        }

        public int GetRealNutColorType()
        {
            return base.GetNutColorType();
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

    public enum SurpriseColorNutState
    {
        COLOR_NOT_REVEALED,
        COLOR_REVEALED
    }
}