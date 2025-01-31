using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.tag.nut_sort {
    public class MixInterstitialAdApplovinMax : BaseInterstitialAd
    {
        #region PUBLIC_VARS

        public MixAdHandlerApplovinMax mixAdHandlerApplovinMax;

        #endregion

        #region PRIVATE_VARS

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public override void Init()
        {
            base.Init();
            mixAdHandlerApplovinMax.InitSimpleInterstitialAd();
        }

        public override bool IsAdLoaded()
        {
            return mixAdHandlerApplovinMax.IsSimpleInterstitialAdLoaded();
        }

        public override void ShowAd()
        {
            mixAdHandlerApplovinMax.ShowSimpleInterstitialAd();
        }

        public override void LoadAd()
        {
            base.LoadAd();
            mixAdHandlerApplovinMax.LoadSimpleInterstitialAd();
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
    }
}
