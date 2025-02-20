using Sirenix.OdinInspector;
using UnityEngine;

namespace Tag.NutSort
{
    public class SafeAreaBGHandler : Manager<SafeAreaBGHandler>
    {
        #region PRIVATE_VARS

        #endregion

        #region PUBLIC_VARS

        public RectTransform bannerAdBgPanelBottom;
        public RectTransform bannerAdBgPanelTop;
        public GameObject closeButton;

        #endregion

        #region FIND_PROPERTIES

        private AdManager AdManager { get { return AdManager.Instance; } }

        #endregion

        #region UNITY_CALLBACKS

        [Button]
        public void SetView()
        {
            float bottomHeight = SafeAreaUtility.GetSafeArea().y;
            float topHeight = SafeAreaUtility.GetResolution().y - SafeAreaUtility.GetSafeArea().height - SafeAreaUtility.GetSafeArea().y;
            bannerAdBgPanelBottom.sizeDelta = new Vector2(bannerAdBgPanelBottom.sizeDelta.x, AdManager.GetBannerAdHeight() + bottomHeight);
            bannerAdBgPanelTop.sizeDelta = new Vector2(bannerAdBgPanelTop.sizeDelta.x, topHeight);
            if (closeButton != null)
                closeButton.SetActive(bannerAdBgPanelBottom.sizeDelta.y > 0);
        }

        #endregion

        #region PUBLIC_FUNCTIONS

        [Button]
        public void Test()
        {
            Debug.Log(SafeAreaUtility.GetSafeArea().y);
            Debug.Log(SafeAreaUtility.GetSafeArea().height);
            Debug.Log(SafeAreaUtility.GetResolution().y);
            Debug.Log(SafeAreaUtility.GetResolution().y - SafeAreaUtility.GetSafeArea().height - SafeAreaUtility.GetSafeArea().y);
            Debug.Log(SafeAreaUtility.GetSafeArea().y);
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
