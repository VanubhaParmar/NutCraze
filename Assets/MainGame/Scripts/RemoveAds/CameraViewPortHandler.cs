using Sirenix.OdinInspector;
using UnityEngine;

namespace Tag.NutSort
{
    public class CameraViewPortHandler : MonoBehaviour
    {
        #region PUBLIC_VARS

        #endregion

        #region PRIVATE_VARS

        [SerializeField] private Camera camera;

        #endregion

        #region FIND_PROPERTIES

        private AdManager AdManager { get { return AdManager.Instance; } }

        #endregion

        #region UNITY_CALLBACKS

        private void Start()
        {
            //ApplySafeArea();
        }

        #endregion

        #region PUBLIC_FUNCTIONS

        [ContextMenu("Apply Safe Area")]
        public void ApplySafeArea()
        {
            float x = camera.rect.x;
            float y = (SafeAreaUtility.GetSafeArea().y + AdManager.GetBannerAdHeight()) / SafeAreaUtility.GetResolution().y;
            float width = camera.rect.width;
            float height = (SafeAreaUtility.GetSafeArea().height - AdManager.GetBannerAdHeight()) / SafeAreaUtility.GetResolution().y;
            camera.rect = new Rect(x, y, width, height);
        }

        [Button("Apply Safe Area For Meta Setup")]
        public void ApplySafeAreaForMetaSetup()
        {
            float x = camera.rect.x;
            float y = (SafeAreaUtility.GetSafeArea().y + 180) / SafeAreaUtility.GetResolution().y;
            float width = camera.rect.width;
            float height = (SafeAreaUtility.GetSafeArea().height - 180) / SafeAreaUtility.GetResolution().y;
            camera.rect = new Rect(x, y, width, height);
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
