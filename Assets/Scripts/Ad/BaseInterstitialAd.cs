using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.Ad
{
    public class BaseInterstitialAd : MonoBehaviour
    {
        #region PUBLIC_VARS

        #endregion

        #region PRIVATE_VARS

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public virtual void Init()
        {
        }

        public virtual void LoadAd()
        {
        }

        public virtual void ShowAd()
        {
        }

        public virtual bool IsAdLoaded()
        {
            return true;
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        #endregion

        #region EVENT_CALLBACKS

        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       

        #endregion
    }
}
