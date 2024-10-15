using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort.LevelEditor
{
    public class LevelEditorNutDataEditView : MonoBehaviour
    {
        #region PUBLIC_VARIABLES
        public int Index => nutIndex;
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private Text indexText;
        [SerializeField] private Dropdown nutTypeDropDown;
        [SerializeField] private Dropdown nutColorTypeDropDown;
        [SerializeField] private Image nutColorDemoImage;

        private BaseNutLevelDataInfo myNutLevelDataInfo;
        private int nutIndex;
        private int nutDataIndex;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void InitView(int nutIndex, int nutDataIndex, BaseNutLevelDataInfo baseNutLevelDataInfo)
        {
            myNutLevelDataInfo = baseNutLevelDataInfo;
            this.nutIndex = nutIndex;
            this.nutDataIndex = nutDataIndex;

            SetView();

            gameObject.SetActive(true);
        }
        #endregion

        #region PRIVATE_METHODS
        private void SetView()
        {
            indexText.text = (nutIndex + 1) + "";

            nutTypeDropDown.ClearOptions();
            List<string> optionsNutTypes = LevelEditorManager.Instance.GetMappingIds<NutTypeIdAttribute>();
            nutTypeDropDown.AddOptions(optionsNutTypes);
            nutTypeDropDown.SetValueWithoutNotify(GetOptionIndexFromDropDown(nutTypeDropDown, myNutLevelDataInfo.nutType));

            nutColorTypeDropDown.ClearOptions();
            List<string> optionsColorTypes = LevelEditorManager.Instance.GetMappingIds<NutColorIdAttribute>();

            nutColorTypeDropDown.AddOptions(optionsColorTypes);
            nutColorTypeDropDown.SetValueWithoutNotify(GetOptionIndexFromDropDown(nutColorTypeDropDown, myNutLevelDataInfo.nutColorTypeId));

            RefreshDemoImageColor();
        }

        private void RefreshDemoImageColor()
        {
            nutColorDemoImage.color = LevelManager.Instance.NutColorThemeTemplateDataSO.GetNutColorThemeInfoOfColor(myNutLevelDataInfo.nutColorTypeId)._mainColor;
        }

        private int GetOptionIndexFromDropDown(Dropdown selectionDropDown, int dataValue)
        {
            List<string> options = selectionDropDown.options.Select(x => x.text.ToString()).ToList();
            string selectedOption = options.Find(x => x.Split("-").First().ConvertToInt() == dataValue);

            return options.IndexOf(selectedOption);
        }

        private int GetNutTypeKeyValueFromOption()
        {
            return nutTypeDropDown.options[nutTypeDropDown.value].text.Split("-").First().ConvertToInt();
        }

        private int GetNutColorTypeKeyValueFromOption()
        {
            return nutColorTypeDropDown.options[nutColorTypeDropDown.value].text.Split("-").First().ConvertToInt();
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        public void OnValueChanged_NutTypeDropdown()
        {
            LevelEditorManager.Instance.Main_OnChangeNutType(nutDataIndex, nutIndex, GetNutTypeKeyValueFromOption());
        }

        public void OnValueChanged_NutColorTypeDropdown()
        {
            LevelEditorManager.Instance.Main_OnChangeNutColorType(nutDataIndex, nutIndex, GetNutColorTypeKeyValueFromOption());
            RefreshDemoImageColor();
        }

        public void OnButtonClick_DeleteData()
        {
            LevelEditorManager.Instance.Main_OnRemoveNutData(nutDataIndex, nutIndex);
        }

        public void OnButtonClick_SelectNutsDataEdit()
        {
        }
        #endregion
    }
}