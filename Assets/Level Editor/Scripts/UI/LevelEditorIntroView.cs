#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort.Editor
{
    public class LevelEditorIntroView : BaseView
    {
        #region PUBLIC_VARIABLES
        public Text totalLevelsText;

        [Space]
        public Dropdown levelTypeDropdown;
        public Dropdown abTestTypeDropdown;
        public InputField levelLoadInputField;
        public InputField levelDuplicateInputField;
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
            SetView();

            RefreshView();
        }
        #endregion

        #region PRIVATE_METHODS
        private void RefreshView()
        {
            totalLevelsText.text = "Total Levels : " + LevelEditorManager.Instance.GetTotalNumberOfLevels(GetCurrentSelectedLevelType());
        }

        private void SetView()
        {
            levelTypeDropdown.ClearOptions();
            levelTypeDropdown.AddOptions(Enum.GetNames(typeof(LevelType)).ToList());
            levelTypeDropdown.SetValueWithoutNotify((int)LevelEditorManager.Instance.TargetLevelType);

            abTestTypeDropdown.ClearOptions();

            List<ABTestType> aBTestTypes = LevelEditorManager.Instance.GetAvailableABVariants();

            abTestTypeDropdown.AddOptions(aBTestTypes.Select(x => x.ToString()).ToList());

            ABTestType currentManagerType = LevelEditorManager.Instance.ABTestType;
            int initialIndex = aBTestTypes.IndexOf(currentManagerType);

            abTestTypeDropdown.SetValueWithoutNotify(initialIndex);
            abTestTypeDropdown.onValueChanged.AddListener(delegate (int index)
            {
                LevelEditorManager.Instance.ABTestType = aBTestTypes[index];
            });
        }

     

        private LevelType GetCurrentSelectedLevelType()
        {
            return (LevelType)levelTypeDropdown.value;
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        public void OnValueChanged_LevelTypeDropdown()
        {
            RefreshView();
            LevelEditorManager.Instance.ChangeTargetLevelType(GetCurrentSelectedLevelType());
        }

        public void OnButtonClick_LoadLevel()
        {
            string loadLevel = levelLoadInputField.text;
            if (!string.IsNullOrEmpty(loadLevel) && int.TryParse(loadLevel, out int levelNumber) && LevelEditorManager.Instance.DoesLevelExist(levelNumber, GetCurrentSelectedLevelType()))
            {
                Hide();
                LevelEditorManager.Instance.LoadEditor(levelNumber);
            }
            else
                LevelEditorToastsView.Instance.ShowToastMessage("Dude Level Does Not Exist !!!!");
        }

        public void OnButtonClick_LoadLastLevel()
        {
            int lastLevel = LevelEditorManager.Instance.GetTotalNumberOfLevels(GetCurrentSelectedLevelType());
            if (LevelEditorManager.Instance.DoesLevelExist(lastLevel, GetCurrentSelectedLevelType()))
            {
                Hide();
                LevelEditorManager.Instance.LoadEditor(lastLevel);
            }
            else
                LevelEditorToastsView.Instance.ShowToastMessage("Dude Level Does Not Exist !!!!");
        }

        public void OnButtonClick_CreateNewLevel()
        {
            string loadLevel = levelDuplicateInputField.text;
            int duplicateLevelNum = -1;
            if (int.TryParse(loadLevel, out int parseLevel))
                duplicateLevelNum = parseLevel;

            Hide();
            LevelEditorManager.Instance.LoadEditor_WithCreateNewLevel(duplicateLevelNum);
        }

        public void OnButtonClick_DuplicateLevel()
        {
            string loadLevel = levelDuplicateInputField.text;
            if (!string.IsNullOrEmpty(loadLevel) && int.TryParse(loadLevel, out int levelNumber) && LevelEditorManager.Instance.DoesLevelExist(levelNumber, GetCurrentSelectedLevelType()))
            {
                Hide();
                LevelEditorManager.Instance.LoadEditor_WithDuplicateLevel(levelNumber);
            }
            else
                LevelEditorToastsView.Instance.ShowToastMessage("Dude Level Does Not Exist !!!!");
        }
        #endregion
    }
}
#endif