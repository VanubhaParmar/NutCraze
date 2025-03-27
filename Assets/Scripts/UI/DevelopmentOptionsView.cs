using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort
{
    public class DevelopmentOptionsView : BaseView
    {
        #region PUBLIC_VARIABLES
        public Dropdown levelAbTestDropdown;
        public InputField levelNumberInput;
        public Toggle levelTapWin;
        public InputField lbScoreInput;
        public InputField extraScrewBoosterField;
        public InputField undoBoosterField;
        public InputField addDaysInput;
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
            List<string> levelAbTestOptions = new List<string>();
            List<ABTestType> aBTestTypes = ResourceManager.Instance.GetAvailableLevelABVariants();
            foreach (ABTestType item in aBTestTypes)
                levelAbTestOptions.Add(item.ToString());
            levelAbTestDropdown.ClearOptions();
            levelAbTestDropdown.AddOptions(levelAbTestOptions);
            levelAbTestDropdown.SetValueWithoutNotify((int)ABTestManager.Instance.GetAbTestType(ABTestSystemType.Level));
            levelNumberInput.text = DataManager.PlayerLevel + "";
            levelTapWin.SetIsOnWithoutNotify(DevelopmentProfileDataSO.winOnLevelNumberTap);
            lbScoreInput.text = "0";
            addDaysInput.text = "0";
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
                    DataManager.Instance.SetplayerLevel(levelNumber);
                    LevelManager.Instance.OnReloadCurrentLevel();
                    GlobalUIManager.Instance.GetView<UserPromptView>().Show("Level Set Success !", canLocalize: false);
                }
                else
                    GlobalUIManager.Instance.GetView<UserPromptView>().Show("Level Out Of Range !", canLocalize: false);
            }
            else
                GlobalUIManager.Instance.GetView<UserPromptView>().Show("Enter Valid Value !", canLocalize: false);
        }

        public void OnValueChanged_ToggleLevelTapWin()
        {
            DevelopmentProfileDataSO.winOnLevelNumberTap = levelTapWin.isOn;
        }

        public void OnButtonClick_SetLeaderboardScore()
        {
            if (int.TryParse(lbScoreInput.text, out int score))
            {
                LeaderboardManager.Instance.Editor_SetScore(score);
                GlobalUIManager.Instance.GetView<UserPromptView>().Show("Success !");
            }
            else
                GlobalUIManager.Instance.GetView<UserPromptView>().Show("Enter Valid Value !");
        }

        public void OnButtonClick_AddDailyRewardsDays()
        {
            if (int.TryParse(addDaysInput.text, out int days))
            {
                DailyRewardManager.Instance.Editor_ForwardDays(days);
                GlobalUIManager.Instance.GetView<UserPromptView>().Show("Success !");
            }
            else
                GlobalUIManager.Instance.GetView<UserPromptView>().Show("Enter Valid Value !");
        }

        public void OnButtonClick_OpenMaxTestSuite()
        {
            // MaxSdk.ShowMediationDebugger();
        }

        public void OnLevelAbTesSetButtonClick()
        {
            ABTestType abTestType = (ABTestType)levelAbTestDropdown.value;
            ABTestManager.Instance.SetABTestType(ABTestSystemType.Level, abTestType);
            Application.Quit();
        }

        public void OnExtraScrewButtonAddClick()
        {
            if (int.TryParse(extraScrewBoosterField.text, out int score))
            {
                DataManager.Instance.AddBoosters(BoosterIdConstant.EXTRA_SCREW, score);
                GlobalUIManager.Instance.GetView<UserPromptView>().Show("Success !",canLocalize: false);
                MainSceneUIManager.Instance.GetView<GameplayView>().SetView();
            }
            else
                GlobalUIManager.Instance.GetView<UserPromptView>().Show("Enter Valid Value !", canLocalize: false);
        }

        public void OnUndoButtonAddClick()
        {
            if (int.TryParse(undoBoosterField.text, out int score))
            {
                DataManager.Instance.AddBoosters(BoosterIdConstant.UNDO, score);
                GlobalUIManager.Instance.GetView<UserPromptView>().Show("Success !", canLocalize: false);
                MainSceneUIManager.Instance.GetView<GameplayView>().SetView();
            }
            else
                GlobalUIManager.Instance.GetView<UserPromptView>().Show("Enter Valid Value !", canLocalize: false);
        }
        #endregion
    }
}