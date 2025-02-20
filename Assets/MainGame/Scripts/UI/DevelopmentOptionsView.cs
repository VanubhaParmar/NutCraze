using System;
using UnityEngine.UI;

namespace Tag.NutSort {
    public class DevelopmentOptionsView : BaseView
    {
        #region PUBLIC_VARIABLES
        public InputField levelNumberInput;
        public Toggle levelTapWin;
        public InputField lbScoreInput;
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
            levelNumberInput.text = DataManager.PlayerLevel.Value + "";
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
                    DataManager.PlayerLevel.SetValue(levelNumber);
                    GameplayManager.Instance.StartMainGameLevel();
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
        #endregion
    }
}