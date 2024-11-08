using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "DevelopmentProfileDataSO", menuName = Constant.GAME_NAME + "/Development Profile/DevelopmentProfileDataSO")]
    public class DevelopmentProfileDataSO : SerializedScriptableObject
    {
        #region PUBLIC_VARIABLES
        public DevelopmentProfileType developmentProfileType;

        [Space]
        public bool canShowReporter;
        public bool canShowFPS;
        public LogType systemLogType;

        [Space]
        public ConfigType firebaseRemoteConfigType;
        [ShowIf("developmentProfileType", DevelopmentProfileType.TEST)] public bool canDirectPurchaseInTestingBuild;
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

    public enum DevelopmentProfileType
    {
        TEST,
        PRODUCTION
    }
}