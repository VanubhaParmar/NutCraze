//using GameAnalyticsSDK;
using System;
using System.Collections.Generic;
using UnityEngine;
using Tag.NutSort;
using GameAnalyticsSDK;

namespace Tag.Ad
{
    public class ApplovinMaxAd : BaseAd
    {
        #region PUBLIC_VARS
        public List<string> adTestDevices;
        #endregion

        #region PRIVATE_VARS

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public override void Init(Action actionToCallOnInitSuccess = null)
        {
            base.Init(actionToCallOnInitSuccess);
            MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
            {
                if (AdManager.Instance.isCMPOn)
                {
                    if (!IsCMPDone)
                    {
                        var cmpService = MaxSdk.CmpService;
                        cmpService.ShowCmpForExistingUser(error =>
                        {
                            if (null == error)
                            {
                                IsCMPDone = true;
                                Debug.Log("<APPLOVIN MAX> CMP Shown Successfully!");
                            }
                        });
                    }
                }

                OnApplovinMaxInitialized(true);
                GameAnalyticsILRD.SubscribeMaxImpressions();

                Debug.Log("<APPLOVIN MAX> Country ! " + sdkConfiguration.CountryCode);
                //if (!Constants.IsProdBuild)
                //{
                //    MaxSdk.ShowMediationDebugger();
                //}
                //if (DeviceManager.Instance.IsPackageIdSame())

                //GameAnalyticsILRD.SubscribeMaxImpressions();
            };

            //MaxSdk.SetSdkKey("PSI2cbZzMTdIM_hEPedK6OrHxpb4uWJVS4XxlT18SgTELdRGGpUPhJnMMFvezrqCspuB6RNiVK8eTZ8HqyTW0n");
            MaxSdk.SetUserId("USER_ID");
            MaxSdk.SetHasUserConsent(true);
            if (adTestDevices.Count > 0)
                MaxSdk.SetTestDeviceAdvertisingIdentifiers(adTestDevices.ToArray());

            MaxSdk.InitializeSdk();
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        private void OnApplovinMaxInitialized(bool success, string error = "")
        {
            if (success)
            {
                Debug.Log("<APPLOVIN MAX> Initialized Successfully! ");
                baseRewardedAdHandler.Init();
                baseInterstitialAd.Init();
                baseBannerAd.Init();

                OnInitSuccess();
            }
            else
            {
                Debug.Log("<APPLOVIN MAX> Initialized failed! with error : " + error);
            }
        }

        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       

        #endregion
    }
}
