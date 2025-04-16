using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "LevelABTestRemoteConfig", menuName = Constant.GAME_NAME + "/Remote Config Data/LevelABTestRemoteConfig")]

    public class LevelABTestRemoteConfig : BaseConfig
    {
        #region PRIVATE_VARS
        [SerializeField] private LevelABTestType levelABTestDefaultType;
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
            return SerializeUtility.SerializeObject((int)levelABTestDefaultType);
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
