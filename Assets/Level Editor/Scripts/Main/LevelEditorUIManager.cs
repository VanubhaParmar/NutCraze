using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort.LevelEditor
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