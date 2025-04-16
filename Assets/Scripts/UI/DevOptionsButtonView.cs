using UnityEngine;

namespace Tag.NutSort
{
    public class DevOptionsButtonView : MonoBehaviour
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private RectTransform mainTransform;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        private void OnEnable()
        {
            CheckForView();
        }
        #endregion

        #region PUBLIC_METHODS
        #endregion

        #region PRIVATE_METHODS
        private void CheckForView()
        {
            mainTransform.gameObject.SetActive(DevProfileHandler.Instance.CurrentDevelopmentProfile.canShowBuildDevOptionsButton);
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        public void OnButtonClick_OpenView()
        {
            MainSceneUIManager.Instance.GetView<DevelopmentOptionsView>().Show();
        }
        #endregion
    }
}