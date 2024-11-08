using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "IAPProductsDataSO", menuName = Constant.GAME_NAME + "/IAP/IAPProductsDataSO")]
    public class IAPProductsDataSO : SerializedScriptableObject
    {
        #region PUBLIC_VARIABLES
        public List<IAPPurchaseData> IAPProducts;
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
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
}