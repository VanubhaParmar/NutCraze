using I2.Loc;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class LanguageSelectionView : BaseView
    {
        #region PRIVATE_VARS

        [SerializeField] private List<LanguageSelectionButton> languageSelectionButtons;
        [SerializeField] public Dictionary<string, bool> languageSelectionButtonConfig = new Dictionary<string, bool>();
        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        [Button]
        public override void Show(Action action = null, bool isForceShow = false)
        {
            base.Show();
            SetButtons();
        }

        public void SetButtons()
        {
            for (int i = 0; i < languageSelectionButtons.Count; i++)
            {
                if (languageSelectionButtonConfig.ContainsKey(languageSelectionButtons[i].SetLanguage._Language))
                {
                    if (languageSelectionButtons[i].SetLanguage._Language == LocalizationManager.CurrentLanguage)
                        languageSelectionButtons[i].SetButtonImage(true, true);
                    else
                        languageSelectionButtons[i].SetButtonImage(false, true);
                }
            }
        }
        #endregion

        #region PRIVATE_FUNCTIONS

        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS

        public void OnClose()
        {
            Hide();
        }

        #endregion

#if UNITY_EDITOR
        [Button]
        public void SetLanguageButtonConfig()
        {
            List<string> languages = LocalizationManager.GetAllLanguages();
            for (int i = 0; i < languages.Count; i++)
            {
                if (!languageSelectionButtonConfig.ContainsKey(languages[i]))
                {
                    languageSelectionButtonConfig.Add(languages[i], true);
                }
            }
        }

        [Button]
        public string GetJson()
        {
            return JsonConvert.SerializeObject(languageSelectionButtonConfig);
        }

#endif
    }
}
