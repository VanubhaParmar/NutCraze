using UnityEngine;
using Balaso;
using System;

namespace Tag.NutSort
{
    public class IDFAManager : Manager<IDFAManager>
    {
        #region PUBLIC_VARS

        #endregion

        #region PRIVATE_VARS

        private Action onAllow;

        #endregion

        #region UNITY_CALLBACKS

        public override void Awake()
        {
            base.Awake();
#if !UNITY_IOS
            OnLoadingDone();
#endif
#if UNITY_IOS
            AppTrackingTransparency.RegisterAppForAdNetworkAttribution();
            AppTrackingTransparency.UpdateConversionValue(3);
            ShowIDFA(OnLoadingDone);
#endif
        }

        #endregion

        #region PUBLIC_FUNCTIONS

        public bool CanShowIDFA()
        {
            Debug.LogError("CanShowIDFA : " + AppTrackingTransparency.TrackingAuthorizationStatus + " nd->" + AppTrackingTransparency.AuthorizationStatus.NOT_DETERMINED);
            return AppTrackingTransparency.TrackingAuthorizationStatus == AppTrackingTransparency.AuthorizationStatus.NOT_DETERMINED;
            //return AppTrackingTransparency.TrackingAuthorizationStatus != AppTrackingTransparency.AuthorizationStatus.AUTHORIZED;
        }

        public void ShowIDFA(Action onAllow)
        {
            this.onAllow = onAllow;


#if UNITY_IOS
            if (!CanShowIDFA())
            {
                onAllow?.Invoke();
                return;
            }

            Debug.LogError("TShowIDFA Start");
            AppTrackingTransparency.OnAuthorizationRequestDone += OnAuthorizationRequestDone;

            AppTrackingTransparency.AuthorizationStatus currentStatus = AppTrackingTransparency.TrackingAuthorizationStatus;
            Debug.Log(string.Format("Current authorization status: {0}", currentStatus.ToString()));
            if (currentStatus != AppTrackingTransparency.AuthorizationStatus.AUTHORIZED)
            {
                Debug.Log("Requesting authorization...");
                AppTrackingTransparency.RequestTrackingAuthorization();
            }
#endif
        }

        #endregion

        #region PRIVATE_FUNCTIONS

#if UNITY_IOS

        /// <summary>
        /// Callback invoked with the user's decision
        /// </summary>
        /// <param name="status"></param>
        private void OnAuthorizationRequestDone(AppTrackingTransparency.AuthorizationStatus status)
        {
            switch (status)
            {
                case AppTrackingTransparency.AuthorizationStatus.NOT_DETERMINED:
                    Debug.Log("AuthorizationStatus: NOT_DETERMINED");
                    break;
                case AppTrackingTransparency.AuthorizationStatus.RESTRICTED:
                    Debug.Log("AuthorizationStatus: RESTRICTED");
                    break;
                case AppTrackingTransparency.AuthorizationStatus.DENIED:
                    onAllow?.Invoke();
                    break;
                case AppTrackingTransparency.AuthorizationStatus.AUTHORIZED:
                    onAllow?.Invoke();
                    break;
            }

            // Obtain IDFA
            Debug.Log(string.Format("IDFA: {0}", AppTrackingTransparency.IdentifierForAdvertising()));
        }
#endif

        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       

        #endregion
    }
}
