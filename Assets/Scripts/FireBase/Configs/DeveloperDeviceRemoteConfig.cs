using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "DeveloperDeviceRemoteConfig", menuName = Constant.GAME_NAME + "/Remote Config Data/DeveloperDeviceRemoteConfig")]

    public class DeveloperDeviceRemoteConfig : BaseConfig
    {
        #region PRIVATE_VARS
        [SerializeField] private List<string> defaultDeveloperDeviceIds = new List<string>();
        #endregion

        #region PUBLIC_VARS
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_FUNCTIONS
        public override string GetDefaultString()
        {
            return SerializeUtility.SerializeObject(defaultDeveloperDeviceIds);
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
