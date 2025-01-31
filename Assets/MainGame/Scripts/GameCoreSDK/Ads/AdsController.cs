using System;
using System.Collections.Generic;
using Firebase.Analytics;
using UnityEngine;

namespace GameCoreSDK.Ads
{
    // TODOs:
    //
    // ADS INTEGRATION
    //
    // 1. Call `_adsNativeBridge.OnResume` and `_adsNativeBridge.OnPause` to send app's lifecycle status accurately to the SDK - Done
    // 2. Send Unix timestamp of install in seconds in `_adsNativeBridge.InitialiseMediationSDK` call to the SDK - Done
    // 3. Implement as many callbacks as needed from the provided ones to listen to any ad event for any adType
    // 4. Implement `_adsMediationCallbacks.OnVideoAdGrantReward` action for granting rewards from w2e
    // 5. Use `_adsMediationCallbacks.OnInterstitialAdHidden`, `_adsMediationCallbacks.OnInterstitialAdDisplayFailed` to understand that control is given back to the game,
    //    when interstitial show call was made. Can continue the game if stopped forcefully
    // 6. Use `_adsMediationCallbacks.OnVideoAdHidden`, `_adsMediationCallbacks.OnVideoAdDisplayFailed` to understand that control is given back to the game,
    //    when video show call was made. Can continue the game if stopped forcefully
    // 7. Best way to make show call for any full screen ad would be to first check if the ad is available (mark a flag when ad loads or make a call to SDk to check the status) and then make the show call
    // 8. Let me know if any other callbacks are needed


    // ADJUST INTEGRATION
    // Make few calls to the SDK as mentioned below for tracking events
    //
    // 1. IapController.cs
    //     Send IAP call to the SDK - Call `IapController.GetInstance().sendPurchaseInfo`. Send dollar(in $) value of the purchase and currency(as "USD") as arguments
    //    `_iapNativeBridge` - is an instance of `IapNativeBridge` class in the SDK

    // 2. PuzzleLevelController.cs
    //    Send a call to the SDK when level starts for a user for the first time - Call `PuzzleController.GetInstance().onLevelStart`. Send levelNumber as argument 
    //    Send a call to the SDK when level completes (user clears the level successfully) - Call `PuzzleController.GetInstance().onLevelComplete`. Send levelNumber and time to clear the level (only consider the time user has spent on the game screen in seconds, reset the time to 0 when user clears/fails the level)
    //    `_levelNativeBridge` - is an instance of `LevelNativeBridge` class in the SDK

    public class AdsController
    {
        private static AdsController _instance;
        private bool _initialized = false;

        // Class to listen to the ad events
        public readonly AdsMediationCallbacks _adsMediationCallbacks;

        // Bridge class to show ads in the game
        private readonly AdsNativeBridge _adsNativeBridge;

        private AdsController()
        {
            _adsMediationCallbacks = new AdsMediationCallbacks();
            _adsNativeBridge = new AdsNativeBridge();
        }

        public static AdsController GetInstance()
        {
            return _instance ??= new AdsController();
        }

        public void OnPauseGame()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            _adsNativeBridge.OnPause();
#endif
        }

        public void OnResumeGame()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            _adsNativeBridge.OnResume();
#endif
        }

        public void ShowGDPRDialouge()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            _adsNativeBridge.ShowGdprConsentDialog();
#endif
        }

        public void Initialize(long installTimeStamp, bool testMode, Action actionToCallOnInitialization = null)
        {
            _adsMediationCallbacks.OnMediationSdkInitialised += () =>
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
#elif UNITY_ANDROID && !UNITY_EDITOR
            _adsNativeBridge.InitialiseMediationSDK(_adsMediationCallbacks, installTimeStamp, testMode);
#endif
        }

        public void ShowBannerAd()
        {
            if (!_initialized)
            {
                return;
            }
            Debug.Log("Show Banner Ad");
#if UNITY_ANDROID && !UNITY_EDITOR
            _adsNativeBridge.ShowBannerAd();
#endif
        }

        public void HideBannerAd()
        {
            if (!_initialized)
            {
                return;
            }
            Debug.Log("Hide Banner Ad");
#if UNITY_ANDROID && !UNITY_EDITOR
            _adsNativeBridge.HideBannerAd();
#endif
        }

        public bool IsInterstitialAdAvailable()
        {
            if (!_initialized)
            {
                return false;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            return _adsNativeBridge.IsInterstitialAdAvailable();
#else
            return false;
#endif
        }

        public void ShowInterstitialAd()
        {
            if (!_initialized)
            {
                return;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            _adsNativeBridge.ShowInterstitialAd();
#endif
        }

        public bool IsVideoAdAvailable()
        {
            if (!_initialized)
            {
                return false;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            return _adsNativeBridge.IsRewardedVideoAdAvailable();
#else
            return false;
#endif
        }

        public void ShowVideoAd()
        {
            if (!_initialized)
            {
                return;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            _adsNativeBridge.ShowRewardedVideoAd();
#endif
        }
    }
}