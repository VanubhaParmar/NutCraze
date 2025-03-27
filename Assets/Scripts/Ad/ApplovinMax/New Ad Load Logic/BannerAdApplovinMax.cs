using UnityEngine;

namespace Tag.NutSort
{
    public class BannerAdApplovinMax : BaseBannerAd
    {
        #region PUBLIC_VARIABLES
        public MixAdHandlerApplovinMax mixAdHandlerApplovinMax;

        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public override void Init()
        {
            base.Init();
            mixAdHandlerApplovinMax.InitBannerAd();
        }
        public override void LoadBanner()
        {
            base.LoadBanner();
            mixAdHandlerApplovinMax.LoadBanner();
        }

        public override void ShowBanner()
        {
            base.ShowBanner();
            mixAdHandlerApplovinMax.ShowBanner();
        }
        public override void StartBannerAdsAutoRefresh()
        {
            base.StartBannerAdsAutoRefresh();
            mixAdHandlerApplovinMax.StartBannerAdsAutoRefresh();
        }

        public override void HideBanner()
        {
            base.HideBanner();
            mixAdHandlerApplovinMax.HideBanner();
        }

        public override void ForceStopBannerAds()
        {
            base.ForceStopBannerAds();
            mixAdHandlerApplovinMax.ForceStopBannerAds();
        }

        public override Rect GetBannerRect()
        {
            return mixAdHandlerApplovinMax.GetBannerAdRect();
        }
        public override bool IsBannerAdLoaded()
        {
            return mixAdHandlerApplovinMax.IsBannerAdLoaded();
        }
        public override bool CanShowBannerAd()
        {
            return base.CanShowBannerAd();
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