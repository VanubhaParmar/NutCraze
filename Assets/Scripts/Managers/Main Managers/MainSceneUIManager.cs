using UnityEngine.UI;

namespace Tag.NutSort
{
    public class MainSceneUIManager : UIManager<MainSceneUIManager>
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
        public void SetUIInteractable(bool state)
        {
            gameObject.GetComponent<GraphicRaycaster>().enabled = state;
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