using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort.LevelEditor
{
    public class ScrewDataEditView : MonoBehaviour
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private Text screwNameText;
        [SerializeField] private Dropdown screwTypeDropdown;
        [SerializeField] private InputField sizeInput;
        [SerializeField] private Text screwStageCountText;

        private ScrewData screwData;
        private int screwDataIndex;
        private int levelStageIndex;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void InitView(int levelStageIndex, int screwDataIndex, ScrewData screwData)
        {
            this.levelStageIndex = levelStageIndex;
            this.screwDataIndex = screwDataIndex;
            this.screwData = screwData;
            SetView();
        }
        #endregion

        #region PRIVATE_METHODS
        private void SetView()
        {
            screwNameText.text = (screwData.id) + ". Screw";
            BaseIDMappingConfig screwTypeIds = LevelEditorManager.ScrewTypeIdMapping;
            string name = screwTypeIds.GetNameFromId(screwData.screwType);
            List<string> options = screwTypeIds.GetListOfNames();
            screwTypeDropdown.ClearOptions();
            screwTypeDropdown.AddOptions(options);
            screwTypeDropdown.SetValueWithoutNotify(options.IndexOf(name));
            sizeInput.text = screwData.capacity.ToString();
            screwStageCountText.text = screwData.screwStages.Length.ToString();
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        public void OnEditButtonClick()
        {
        }

        public void OnScrewTypeDropdownValueChange()
        {
            int value = screwTypeDropdown.value;
            screwData.screwType = LevelEditorManager.ScrewTypeIdMapping.GetIdFromName(screwTypeDropdown.options[value].text);
        }

        public void OnPasteButtonClick()
        {
            string json = EditorGUIUtility.systemCopyBuffer;
            try
            {
                ScrewData screwData = JsonConvert.DeserializeObject<ScrewData>(json);
                this.screwData = screwData;
                SetView();
            }
            catch (Exception e)
            {
                Debug.LogError("Invalid LevelStage JSON in clipboard " + e);
                LevelEditorToastsView.Instance.ShowToastMessage("<color=red>Invalid ScrewData JSON in clipboard</color>");
            }
        }

        public void OnCopyButtonClick()
        {
            string json = JsonConvert.SerializeObject(this.screwData);
            EditorGUIUtility.systemCopyBuffer = json;
        }
        #endregion
    }
}