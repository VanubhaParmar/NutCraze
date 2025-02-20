using System;
using System.Collections.Generic;
using System.Linq;
using Tag.NutSort;
using UnityEngine;
using UnityEngine.UI;

namespace com.tag.editor
{
    public class LevelEditorPatternSelectView : BaseView
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private Dropdown arrangementTypeDropdown;
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

            LevelEditorManager.Instance.levelGridSetter.ShowGrid(LevelEditorManager.Instance.TempEditLevelDataSO.levelArrangementConfigDataSO);
        }

        public override void Hide()
        {
            base.Hide();

            LevelEditorManager.Instance.levelGridSetter.ResetGrid();
        }
        #endregion

        #region PRIVATE_METHODS
        private void SetView()
        {
            var currentArrangement = LevelEditorManager.Instance.TempEditLevelDataSO.levelArrangementConfigDataSO;

            arrangementTypeDropdown.ClearOptions();
            List<string> options = LevelEditorManager.Instance.LevelArrangementsListDataSO.levelArrangementConfigDataSOs.Select(x => x.name).ToList();
            arrangementTypeDropdown.AddOptions(options);
            arrangementTypeDropdown.SetValueWithoutNotify(options.IndexOf(currentArrangement.name));
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        public void OnValueChanged_ArrangementTypeDropdown()
        {
            int currentValue = arrangementTypeDropdown.value;
            LevelEditorManager.Instance.Main_OnChangeLevelArrangementConfig(currentValue);
            LevelEditorManager.Instance.levelGridSetter.ShowGrid(LevelEditorManager.Instance.TempEditLevelDataSO.levelArrangementConfigDataSO);
        }
        #endregion
    }
}