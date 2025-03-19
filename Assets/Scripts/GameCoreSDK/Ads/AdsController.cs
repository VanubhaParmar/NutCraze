using System;
using Mediation.Runtime.Scripts;
using UnityEngine;

namespace GameCoreSDK.Ads
{
    public class AdsController
    {
        private static AdsController _instance;
        private bool _initialized = false;

        private AdsController()
        {
        }

        public static AdsController GetInstance()
        {
            return _instance ??= new AdsController();
        }
        
        public void OnAwake()
        {
            SdkManager.Instance.OnAwake();
            SdkManager.Instance.TrackIlrdInGameAnalytics(true);
        }

        public void OnPauseGame()
        {
            SdkManager.Instance.OnPause();
        }

        public void OnResumeGame()
        {
            SdkManager.Instance.OnResume();
        }

        public void ShowGDPRDialouge()
        {
            AdsNative.Instance.ShowGdprConsentDialog();
        }

        public void Initialize(long installTimeStamp, bool testMode, Action actionToCallOnInitialization = null)
        {
            AdsNativeCallbacks.Instance.OnMediationSdkInitialised += () =>
            {
                _initialized = true;
                actionToCallOnInitialization?.Invoke();
            };

            //_adsMediationCallbacks.OnAdRevenueReceived += (string platform, string source, string format, string adUnitName, double value, string currency) =>
            //{
            //    // FIREBASE EVENT
            //    // TODO: Use these params to log event in Firebase - Done
            //};

            // TODO: Subscribe to adType callbacks - look for actions defined in `AdsMediationCallbacks` class - Done

            //long installTimestamp = 0; // TODO: Install timestamp - Done 
            //const bool testMode = false; // TODO: Test flag - Done

#if UNITY_EDITOR
            _initialized = true;
            actionToCallOnInitialization?.Invoke();
#else
            AdsNative.Instance.InitialiseMediationSDK(installTimeStamp, testMode);
#endif
        }

        public void ShowBannerAd()
        {
            if (!_initialized)
            {
                return;
            }
            Debug.Log("Show Banner Ad");
            AdsNative.Instance.ShowBannerAd();
        }

        public void HideBannerAd()
        {
            if (!_initialized)
            {
                return;
            }
            Debug.Log("Hide Banner Ad");
            AdsNative.Instance.HideBannerAd();
        }

        public bool IsInterstitialAdAvailable()
        {
            if (!_initialized)
            {
                return false;
            }

            return AdsNative.Instance.IsInterstitialAdAvailable();
        }

        public void ShowInterstitialAd()
        {
            if (!_initialized)
            {
                return;
            }
            
            AdsNative.Instance.ShowInterstitialAd();
        }

        public bool IsVideoAdAvailable()
        {
            if (!_initialized)
            {
                return false;
            }

            return AdsNative.Instance.IsRewardedVideoAdAvailable();
        }

        public void ShowVideoAd()
        {
            if (!_initialized)
            {
                return;
            }

            AdsNative.Instance.ShowRewardedVideoAd();
        }
    }
}