using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "AdsDataRemoteConfig", menuName = Constant.GAME_NAME + "/Remote Config Data/AdsDataRemoteConfig")]
    public class AdsDataRemoteConfig : BaseConfig
    {
        #region PUBLIC_VARIABLES
        public AdManagerDataSO adManagerDataSO;
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
            return SerializeUtility.SerializeObject(adManagerDataSO.GetDefaultAdConfigData());
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