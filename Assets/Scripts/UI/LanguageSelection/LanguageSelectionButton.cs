using I2.Loc;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort
{
    public class LanguageSelectionButton : MonoBehaviour
    {
        #region PRIVATE_VARS
        [SerializeField] private Text languageNameText;
        [SerializeField] private GameObject selectedGO;
        [SerializeField] private SetLanguage setLanguage;
        private bool isEnable;
        public SetLanguage SetLanguage { get => setLanguage; set => setLanguage = value; }

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public void OnButtonClick()
        {
            if (!isEnable)
            {
                gameObject.SetActive(false);
                return;
            }

            if (LocalizationManager.CurrentLanguage == setLanguage._Language)
            {
                GlobalUIManager.Instance.HideView<LanguageSelectionView>();
            }
            else
            {
                setLanguage.ApplyLanguage();
                GlobalUIManager.Instance.GetView<LanguageSelectionView>().SetButtons();

                string message = $"{LocalizationHelper.GetTranslate("Language")} {languageNameText.text} {LocalizationHelper.GetTranslate("Set Successfully !")}";

                GlobalUIManager.Instance.GetView<UserPromptView>().Show(message, GlobalUIManager.Instance.HideView<LanguageSelectionView>, false);
            }

        }

        public void SetButtonImage(bool isSelcted, bool isEnable)
        {
            this.isEnable = isEnable;
            gameObject.SetActive(isEnable);
            selectedGO.SetActive(isSelcted); ;
        }

        [Button]
        public void SetButton()
        {
            SetLanguage = gameObject.GetComponent<SetLanguage>();
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
