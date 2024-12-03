using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort
{
    public class DevelopmentOptionsView : BaseView
    {
        #region PUBLIC_VARIABLES
        public InputField levelNumberInput;
        public Toggle levelTapWin;
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public override void Show(Action action = null, bool isForceShow = false)
        {
            base.Show(action, isForceShow);
            InitView();
        }

        public override void Hide()
        {
            base.Hide();
        }
        #endregion

        #region PRIVATE_METHODS
        private void InitView()
        {
            levelNumberInput.text = PlayerPersistantData.GetMainPlayerProgressData().playerGameplayLevel + "";
            levelTapWin.SetIsOnWithoutNotify(DevelopmentProfileDataSO.winOnLevelNumberTap);
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        public void OnButtonClick_SetLevel()
        {
            if (int.TryParse(levelNumberInput.text, out int levelNumber))
            {
                bool result = levelNumber > 0;// && LevelManager.Instance.DoesLevelExist(levelNumber);
                if (result)
                {
                    var playerData = PlayerPersistantData.GetMainPlayerProgressData();
                    playerData.playerGameplayLevel = levelNumber;
                    PlayerPersistantData.SetMainPlayerProgressData(playerData);

                    GameplayManager.Instance.OnReloadCurrentLevel();
                    GlobalUIManager.Instance.GetView<UserPromptView>().Show("Level Set Success !");
                }
                else
                    GlobalUIManager.Instance.GetView<UserPromptView>().Show("Level Out Of Range !");
            }
            else
                GlobalUIManager.Instance.GetView<UserPromptView>().Show("Enter Valid Value !");
        }

        public void OnValueChanged_ToggleLevelTapWin()
        {
            DevelopmentProfileDataSO.winOnLevelNumberTap = levelTapWin.isOn;
        }

        public void OnButtonClick_OpenMaxTestSuite()
        {
            // MaxSdk.ShowMediationDebugger();
        }
        #endregion
    }
}