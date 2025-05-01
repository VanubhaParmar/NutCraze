using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "LevelFailABTestRemoteConfig", menuName = Constant.GAME_NAME + "/Remote Config Data/LevelFailABTestRemoteConfig")]

    public class LevelFailABTestRemoteConfig : BaseConfig
    {
        #region PRIVATE_VARS
        [SerializeField] private LevelFailABTestType levelFailABTestDefaultType;
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
            return SerializeUtility.SerializeObject((int)levelFailABTestDefaultType);
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
