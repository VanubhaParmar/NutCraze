using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "AdjustRemoteConfigDataSO", menuName = Constant.GAME_NAME + "/Remote Config Data/AdjustRemoteConfigDataSO")]
    public class AdjustRemoteConfigDataSO : BaseConfig
    {
        #region PUBLIC_VARIABLES
        public AdJustRemoteConfig adJustRemoteConfig;
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public override string GetDefaultString()
        {
            return SerializeUtility.SerializeObject(adJustRemoteConfig);
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
}