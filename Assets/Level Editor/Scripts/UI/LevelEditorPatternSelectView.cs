#if  UNITY_EDITOR
using System;
using System.Collections.Generic;
using Tag.NutSort;
using UnityEngine;
using UnityEngine.UI;

namespace tag.editor
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
            LevelEditorManager.Instance.ShowCurrentLevelGrid();
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
            var currentArrangement = LevelEditorManager.Instance.TempEditLevelDataSO.ArrangementId;
            string name = LevelEditorManager.Instance.LevelArrangementIdMaaping.GetNameFromId(currentArrangement);
            arrangementTypeDropdown.ClearOptions();
            List<string> options = LevelEditorManager.Instance.GetAllArrangementOptions();
            arrangementTypeDropdown.AddOptions(options);
            arrangementTypeDropdown.SetValueWithoutNotify(options.IndexOf(name));
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
            List<string> options = LevelEditorManager.Instance.GetAllArrangementOptions();
            int arrangementID = LevelEditorManager.Instance.LevelArrangementIdMaaping.GetIdFromName(options[currentValue]);
            LevelEditorManager.Instance.Main_OnChangeLevelArrangementConfig(arrangementID);
            LevelEditorManager.Instance.ShowCurrentLevelGrid();
        }
        #endregion
    }
}
#endif