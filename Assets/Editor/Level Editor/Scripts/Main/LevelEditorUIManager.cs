using com.tag.nut_sort;
using UnityEngine;

namespace com.tag.editor
{
    public class LevelEditorUIManager : UIManager<LevelEditorUIManager>
    {
        #region PUBLIC_VARIABLES
        public GameObject inputBlocker;
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void SetGameplayInputBlocker(bool state)
        {
            inputBlocker.gameObject.SetActive(state);
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