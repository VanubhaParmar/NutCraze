using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace Tag.NutSort
{
    public class DevelopmentOptionsView : BaseView
    {
        #region PUBLIC_VARIABLES
        public Dropdown levelAbTestDropdown;
        public Dropdown levelFailAbTestDropdown;
        public InputField levelNumberInput;
        public InputField specailLevelNumberInput;
        public Toggle levelTapWin;
        public InputField lbScoreInput;
        public InputField addCoinField;
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
            levelAbTestDropdown.ClearOptions();
            List<LevelABTestType> aBTestTypes = ResourceManager.Instance.GetAvailableLevelABVariants();
            levelAbTestDropdown.AddOptions(aBTestTypes.Select(x => x.ToString()).ToList());
            int initialIndex = aBTestTypes.IndexOf(LevelManager.Instance.CurrentTestingType);
            levelAbTestDropdown.SetValueWithoutNotify(initialIndex);

            levelAbTestDropdown.onValueChanged.AddListener(delegate (int index)
            {
                LevelManager.Instance.CurrentTestingType = aBTestTypes[index];
            });

            levelFailAbTestDropdown.ClearOptions();
            List<LevelFailABTestType> levelFailABTestTypes = Enum.GetValues(typeof(LevelFailABTestType)).Cast<LevelFailABTestType>().ToList();
            levelFailAbTestDropdown.AddOptions(levelFailABTestTypes.Select(x => x.ToString()).ToList());
            int initialLevelFailIndex = levelFailABTestTypes.IndexOf(LevelFailManager.Instance.CurrentTestingType);
            levelFailAbTestDropdown.SetValueWithoutNotify(initialLevelFailIndex);

            levelFailAbTestDropdown.onValueChanged.AddListener(delegate (int index)
            {
                LevelFailManager.Instance.CurrentTestingType = levelFailABTestTypes[index];
            });


            levelNumberInput.text = DataManager.PlayerLevel + "";
            specailLevelNumberInput.text = DataManager.PlayerSpecialLevel + "";
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
                    GameplayManager.Instance.RestartGamePlay();
                    GlobalUIManager.Instance.GetView<UserPromptView>().Show("Level Set Success !", canLocalize: false);
                }
                else
                    GlobalUIManager.Instance.GetView<UserPromptView>().Show("Level Out Of Range !", canLocalize: false);
            }
            else
                GlobalUIManager.Instance.GetView<UserPromptView>().Show("Enter Valid Value !", canLocalize: false);
        }
        
        public void OnButtonClick_SetSpecialLevel()
        {
            if (int.TryParse(specailLevelNumberInput.text, out int levelNumber))
            {
                bool result = levelNumber > 0;// && LevelManager.Instance.DoesLevelExist(levelNumber);
                if (result)
                {
                    DataManager.Instance.SetPlayerSpecialLevel(levelNumber);
                    GameplayManager.Instance.RestartGamePlay();
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

        public void OnAddCoinButtonClick()
        {
            if (int.TryParse(addCoinField.text, out int score))
            {
                Currency currency = DataManager.Instance.GetCurrency(CurrencyConstant.COIN);
                currency.Add(score);
                GlobalUIManager.Instance.GetView<UserPromptView>().Show("Success !", canLocalize: false);
            }
            else
                GlobalUIManager.Instance.GetView<UserPromptView>().Show("Enter Valid Value !", canLocalize: false);
        }
        
        public void OnExtraScrewButtonAddClick()
        {
            if (int.TryParse(extraScrewBoosterField.text, out int score))
            {
                DataManager.Instance.AddBoosters(BoosterIdConstant.EXTRASCREW, score);
                GlobalUIManager.Instance.GetView<UserPromptView>().Show("Success !", canLocalize: false);
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