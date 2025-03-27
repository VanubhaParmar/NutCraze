#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Tag.NutSort;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort.Editor {
    public class LevelEditorScrewDataEditView : MonoBehaviour
    {
        #region PUBLIC_VARIABLES
        public int Index => index;
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private Text screwNameText;
        [SerializeField] private Dropdown screwTypeDropDown;
        [SerializeField] private InputField nutsCapacityInputField;

        [Space]
        [SerializeField] private RectTransform selectionParent;

        private BaseScrewLevelDataInfo myScrewLevelDataInfo;
        private int index;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void InitView(int index, BaseScrewLevelDataInfo baseScrewLevelDataInfo)
        {
            this.index = index;
            myScrewLevelDataInfo = baseScrewLevelDataInfo;
            SetView();
            OnScrewEditSelection(false);

            gameObject.SetActive(true);
        }

        public void OnScrewEditSelection(bool state)
        {
            selectionParent.gameObject.SetActive(state);
        }
        #endregion

        #region PRIVATE_METHODS
        private void RefreshNutCapacityValue()
        {
            nutsCapacityInputField.SetTextWithoutNotify(myScrewLevelDataInfo.screwNutsCapacity.ToString());
        }

        private void SetView()
        {
            screwNameText.text = (index + 1) + ". Screw";

            screwTypeDropDown.ClearOptions();
            List<string> options = LevelEditorManager.Instance.GetMappingIds<ScrewTypeIdAttribute>();
            screwTypeDropDown.AddOptions(options);
            screwTypeDropDown.SetValueWithoutNotify(GetOptionIndexFromDropDown(myScrewLevelDataInfo.screwType));

            RefreshNutCapacityValue();
        }

        private int GetOptionIndexFromDropDown(int dataValue)
        {
            List<string> options = screwTypeDropDown.options.Select(x => x.text.ToString()).ToList();
            string selectedOption = options.Find(x => x.Split("-").First().ConvertToInt() == dataValue);

            return options.IndexOf(selectedOption);
        }

        private int GetKeyValueFromOption()
        {
            return screwTypeDropDown.options[screwTypeDropDown.value].text.Split("-").First().ConvertToInt();
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        public void OnValueChanged_ScrewTypeDropdown()
        {
            LevelEditorManager.Instance.Main_OnChangeScrewType(index, GetKeyValueFromOption());
        }

        public void OnButtonClick_SelectScrewDataEdit()
        {
            LevelEditorUIManager.Instance.GetView<LevelEditorMainEditView>().GetSubView<LevelEditorMainEditLevelView>().OnScrewDataSelected(index);
        }

        public void OnButtonClick_EditData()
        {
            LevelEditorUIManager.Instance.GetView<LevelEditorMainEditView>().GetSubView<LevelEditorMainEditLevelView>().OnScrewDataSelected(index);
            LevelEditorUIManager.Instance.GetView<LevelEditorMainEditView>().GetSubView<LevelEditorMainEditLevelView>().OnEditCurrentSelectedScrewData();
        }

        public void OnValueChangeSubmit_NutsCapacity(string value)
        {
            int valueParse = value.ConvertToInt();
            LevelEditorManager.Instance.Main_OnChangeScrewCapacity(index, valueParse - myScrewLevelDataInfo.screwNutsCapacity);

            RefreshNutCapacityValue();
        }
        #endregion
    }
}
#endif