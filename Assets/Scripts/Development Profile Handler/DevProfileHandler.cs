using FMOD;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Tag.NutSort
{
    public class DevProfileHandler : Manager<DevProfileHandler>
    {
        #region PUBLIC_VARIABLES
        public DevelopmentProfileDataSO CurrentDevelopmentProfile => currentDevelopmentProfile;
        public MainBuildSettingsDataSO MainBuildSettingsDataSO => mainBuildSettingsDataSO;
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private MainBuildSettingsDataSO mainBuildSettingsDataSO;
        [SerializeField] private List<DevelopmentProfileDataSO> developmentProfiles;

        [ShowInInspector, ReadOnly] private DevelopmentProfileDataSO currentDevelopmentProfile;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        public override void Awake()
        {
            base.Awake();
            OnInitializeDevProfileHandler();
            OnLoadingDone();
        }
        #endregion

        #region PUBLIC_METHODS
        public void OnInitializeDevProfileHandler()
        {
            currentDevelopmentProfile = developmentProfiles.Find(x => x.developmentProfileType == mainBuildSettingsDataSO.currentBuildDevelopmentProfileType);
            OnDevProfileInitialized();
        }

        public bool IsProductionBuild()
        {
            return CurrentDevelopmentProfile.developmentProfileType == DevelopmentProfileType.PRODUCTION;
        }

        public bool CanDirectPurchaseInTestingBuild()
        {
            return !IsProductionBuild() && CurrentDevelopmentProfile.canDirectPurchaseInTestingBuild;
        }
        #endregion

        #region PRIVATE_METHODS
        private void OnDevProfileInitialized()
        {
            var reporter = FindObjectOfType<Reporter>(true);
            if (reporter != null)
                reporter.gameObject.SetActive(currentDevelopmentProfile.canShowReporter);

            var hudFPS = FindObjectOfType<HUDFPS>(true);
            if (hudFPS != null)
                hudFPS.gameObject.SetActive(currentDevelopmentProfile.canShowFPS);

            var firebaseRemoteConfig = FindObjectOfType<FirebaseRemoteConfigManager>(true);
            if (firebaseRemoteConfig != null)
                firebaseRemoteConfig.configType = currentDevelopmentProfile.firebaseRemoteConfigType;

            Debug.unityLogger.filterLogType = currentDevelopmentProfile.systemLogType;
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}