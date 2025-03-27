using System.Collections.Generic;
using Tag.NutSort;

namespace tag.editor
{
    public class LevelEditorToastsView : BaseView
    {
        #region PUBLIC_VARIABLES
        public static LevelEditorToastsView Instance { get; private set; }
        public LevelEditorToast toastPrefab;
        #endregion

        #region PRIVATE_VARIABLES
        private List<LevelEditorToast> toastList = new List<LevelEditorToast>();
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        private void OnEnable()
        {
            if (Instance == null)
                Instance = this;
        }
        #endregion

        #region PUBLIC_METHODS
        public void ShowToastMessage(string message)
        {
            UnityEngine.Debug.Log("ShowToastMessage: " + message);
            var toast = GetUnusedToastView() ?? InstantiateNewToast();
            toast.ShowToast(message);
        }
        #endregion

        #region PRIVATE_METHODS
        private LevelEditorToast GetUnusedToastView()
        {
            return toastList.Find(x => !x.gameObject.activeInHierarchy);
        }

        private LevelEditorToast InstantiateNewToast()
        {
            LevelEditorToast levelEditorToast = Instantiate(toastPrefab, toastPrefab.transform.parent);
            levelEditorToast.gameObject.SetActive(false);
            toastList.Add(levelEditorToast);

            return levelEditorToast;
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}