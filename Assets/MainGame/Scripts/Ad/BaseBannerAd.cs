using UnityEngine;

namespace com.tag.nut_sort {
    public class BaseBannerAd : MonoBehaviour
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public virtual void Init()
        {
        }

        public virtual void LoadBanner()
        {
        }

        public virtual void HideBanner()
        {
        }

        public virtual void ForceStopBannerAds()
        {
        }

        public virtual void StartBannerAdsAutoRefresh()
        {
        }

        public virtual void ShowBanner()
        {
        }

        public virtual Rect GetBannerRect()
        {
            return default;
        }
        public virtual bool IsBannerAdLoaded()
        {
            return false;
        }
        public virtual bool CanShowBannerAd()
        {
            return false;
        }
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
}