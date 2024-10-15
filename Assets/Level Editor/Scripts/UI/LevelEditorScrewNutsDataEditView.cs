using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort.LevelEditor
{
    public class LevelEditorScrewNutsDataEditView : BaseView
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private Text screwHeaderText;
        [SerializeField] private LevelEditorNutDataEditView levelEditorNutDataEditViewPrefab;
        [SerializeField] private ScrollRect nutsDataEditScrollRect;
        [SerializeField] private RectTransform nutsDataAddScrollRectButton;

        [Space]
        [SerializeField] private Dropdown screwTypeDropDown;
        [SerializeField] private InputField nutsCapacityInputField;
        [SerializeField] private Toggle fillRemainingDataToggle;

        [Space]
        [SerializeField] private Button nextScrewButton;
        [SerializeField] private Button prevScrewButton;

        private List<LevelEditorNutDataEditView> generatedLevelEditorNutsDataEditViews = new List<LevelEditorNutDataEditView>();

        private int currentSelectedScrewDataIndex = -1;
        private BaseScrewLevelDataInfo myScrewLevelDataInfo;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        private void OnEnable()
        {
            LevelEditorManager.onLevelEditorNutsDataCountChanged += LevelEditorManager_onLevelEditorNutsDataCountChanged;
        }

        private void OnDisable()
        {
            LevelEditorManager.onLevelEditorNutsDataCountChanged -= LevelEditorManager_onLevelEditorNutsDataCountChanged;
        }
        #endregion

        #region PUBLIC_METHODS
        public void Show(int selectedScrewIndex)
        {
            var screwData = LevelEditorManager.Instance.TempEditLevelDataSO.levelScrewDataInfos;

            currentSelectedScrewDataIndex = selectedScrewIndex;
            myScrewLevelDataInfo = screwData[currentSelectedScrewDataIndex];

            Show();

            SetView();
            LevelEditorManager.Instance.Main_OnScrewSelectedForEdit(currentSelectedScrewDataIndex);
        }

        public override void Hide()
        {
            base.Hide();
            LevelEditorManager.Instance.Main_ResetScrewSelection();
        }
        #endregion

        #region PRIVATE_METHODS
        private void SetView()
        {
            screwHeaderText.text = (currentSelectedScrewDataIndex + 1) + ". Screw";

            SetNutsEditViews();

            screwTypeDropDown.ClearOptions();
            List<string> options = LevelEditorManager.Instance.GetMappingIds<ScrewTypeIdAttribute>();
            screwTypeDropDown.AddOptions(options);
            screwTypeDropDown.SetValueWithoutNotify(GetOptionIndexFromDropDown(myScrewLevelDataInfo.screwType));

            RefreshNutCapacityValue();

            var allScrews = LevelEditorManager.Instance.TempEditLevelDataSO.levelScrewDataInfos.Count;

            nextScrewButton.interactable = allScrews > currentSelectedScrewDataIndex + 1;
            prevScrewButton.interactable = currentSelectedScrewDataIndex > 0;

            fillRemainingDataToggle.SetIsOnWithoutNotify(false);
        }

        private void RefreshNutCapacityValue()
        {
            nutsCapacityInputField.SetTextWithoutNotify(myScrewLevelDataInfo.screwNutsCapacity.ToString());
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

        private void SetNutsEditViews()
        {
            generatedLevelEditorNutsDataEditViews.ForEach(x => x.gameObject.SetActive(false));

            var targetScrewGridCellId = LevelEditorManager.Instance.TempEditLevelDataSO.levelArrangementConfigDataSO.arrangementCellIds[currentSelectedScrewDataIndex];

            var nutsData = LevelEditorManager.Instance.TempEditLevelDataSO.screwNutsLevelDataInfos;
            var targetNutsData = nutsData.Find(x => x.targetScrewGridCellId.IsEqual(targetScrewGridCellId));

            if (targetNutsData != null)
            {
                for (int i = 0; i < targetNutsData.levelNutDataInfos.Count; i++)
                {
                    var nutsEditView = GetInactiveNutDataEditView() ?? GenerateNewNutDataEditView();
                    nutsEditView.InitView(i, currentSelectedScrewDataIndex, targetNutsData.levelNutDataInfos[i]);
                }
            }

            nutsDataAddScrollRectButton.SetAsLastSibling();
        }

        private LevelEditorNutDataEditView GetInactiveNutDataEditView()
        {
            return generatedLevelEditorNutsDataEditViews.Find(x => !x.gameObject.activeInHierarchy);
        }

        private LevelEditorNutDataEditView GenerateNewNutDataEditView()
        {
            LevelEditorNutDataEditView newView = Instantiate(levelEditorNutDataEditViewPrefab, nutsDataEditScrollRect.content);
            newView.gameObject.SetActive(false);
            generatedLevelEditorNutsDataEditViews.Add(newView);
            return newView;
        }
        #endregion

        #region EVENT_HANDLERS
        private void LevelEditorManager_onLevelEditorNutsDataCountChanged()
        {
            RefreshNutCapacityValue();
            SetNutsEditViews();
        }
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        public void OnValueChanged_ScrewTypeDropdown()
        {
            LevelEditorManager.Instance.Main_OnChangeScrewType(currentSelectedScrewDataIndex, GetKeyValueFromOption());
        }
        public void OnValueChangeSubmit_NutsCapacity(string value)
        {
            int valueParse = value.ConvertToInt();
            LevelEditorManager.Instance.Main_OnChangeScrewCapacity(currentSelectedScrewDataIndex, valueParse - myScrewLevelDataInfo.screwNutsCapacity, fillRemainingDataToggle.isOn ? LevelEditorConstants.Max_Number_Of_Nuts_In_Screw : 0);
        }
        public void OnButtonClick_AddNewNutsData()
        {
            LevelEditorManager.Instance.Main_OnChangeAddScrewCapacity(currentSelectedScrewDataIndex);
        }

        public void OnButtonClick_Back()
        {
            Hide();
            LevelEditorUIManager.Instance.GetView<LevelEditorMainEditView>().GetSubView<LevelEditorMainEditLevelView>().Show(currentSelectedScrewDataIndex);
        }
        public void OnButtonClick_NextScrewData()
        {
            Show(currentSelectedScrewDataIndex + 1);
        }
        public void OnButtonClick_PrevScrewData()
        {
            Show(currentSelectedScrewDataIndex - 1);
        }
        #endregion
    }
}