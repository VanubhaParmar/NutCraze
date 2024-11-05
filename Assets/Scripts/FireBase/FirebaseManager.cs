using Firebase;
using Firebase.Extensions;
using System;
using System.Collections;
using UnityEngine;

namespace Tag.NutSort
{
    public class FirebaseManager : Manager<FirebaseManager>
    {
        #region PUBLIC_VARS
        public FirebaseRemoteConfigManager FirebaseRC => _firebaseRemoteConfigManager;

        [SerializeField] private FirebaseAnalyticsManager _firebaseAnalyticsManager;
        [SerializeField] private FirebaseCrashlyticsManager _firebaseCrashlyticsManager;
        [SerializeField] private FirebaseRemoteConfigManager _firebaseRemoteConfigManager;
        #endregion

        #region PRIVATE_VARS

        private FirebaseApp app;
        private bool isInit = false;
        private DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;

        #endregion

        #region UNITY_CALLBACKS
        public override void Awake()
        {
            base.Awake();
            Init(OnLoadingDone);
        }
        #endregion

        #region PUBLIC_FUNCTIONS
        public void Init(Action onCompleteInit)
        {
            StartCoroutine(InitWaitCo(onCompleteInit));
            try
            {
                //ContinueWithOnMainThread for fixing 'Input Dispatch Timed out ANR'
                FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
                {
                    dependencyStatus = task.Result;
                    if (dependencyStatus == DependencyStatus.Available)
                    {
                        app = FirebaseApp.DefaultInstance;

                        isInit = true;
                    }
                    else
                    {
                        Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
                    }
                });
            }
            catch (Exception e)
            {
                Debug.LogError(e + "Async task failed");
            }
        }

     
        #endregion

        #region PRIVATE_FUNCTIONS

        private void Initialize(Action onCompleteInit)
        {
            FirebaseApp.LogLevel = LogLevel.Error;
            _firebaseAnalyticsManager.Init();
            _firebaseCrashlyticsManager.Init();
            _firebaseRemoteConfigManager.Init(onCompleteInit);
        }

        #endregion

        #region CO-ROUTINES

        private IEnumerator InitWaitCo(Action onCompleteInit)
        {
            while (!isInit)
            {
                yield return null;
            }
            Initialize(onCompleteInit);
        }

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       

        #endregion
    }
}