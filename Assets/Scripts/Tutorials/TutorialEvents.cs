using UnityEngine;

namespace Tag.NutSort
{
    public class TutorialEvents : MonoBehaviour
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
        public void LockAllScrewInputs()
        {
            var allScrews = LevelManager.Instance.LevelScrews;
            allScrews.ForEach(x => x.SetScrewInteractableState(ScrewState.Locked));
        }

        public void LockMainSceneUI()
        {
            MainSceneUIManager.Instance.SetUIInteractable(false);
        }

        public void UnlockMainSceneUI()
        {
            MainSceneUIManager.Instance.SetUIInteractable(true);
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