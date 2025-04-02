using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort.LevelEditor
{
    public class ScrewStageDataEditView : BaseView
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private Text screwHeaderText;
        [SerializeField] private ScrewStageSlot stageSlot;
        [SerializeField] private RectTransform addScrewStageButton;
        [SerializeField] private Dropdown screwTypeDropDown;
        [SerializeField] private InputField sizeInputField;

        [SerializeField] private Button nextScrewButton;
        [SerializeField] private Button prevScrewButton;

        private List<ScrewStageSlot> screwStageSlots = new List<ScrewStageSlot>();
        private int currentSelectedScrewDataIndex = -1;
        private ScrewData screwData;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void Show(ScrewData screwData)
        {
            this.screwData = screwData;
            currentSelectedScrewDataIndex = screwData.id;
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
            screwHeaderText.text = (screwData.id) + ". Screw";
            SetScrewStageSlot();
            SetScrewTypeDropdwon();
            SetNavigateButtons();
        }

        private void SetScrewTypeDropdwon()
        {
            screwTypeDropDown.ClearOptions();
            BaseIDMappingConfig screwTypeIdMapping = LevelEditorManager.ScrewTypeIdMapping;
            List<string> options = screwTypeIdMapping.GetListOfNames();
            screwTypeDropDown.AddOptions(options);
            screwTypeDropDown.SetValueWithoutNotify(options.IndexOf(screwTypeIdMapping.GetNameFromId(screwData.screwType)));
        }

        private void SetNavigateButtons()
        {
            var allScrews = LevelEditorManager.CurrentLevelStage.screwDatas.Length;
            nextScrewButton.interactable = allScrews > currentSelectedScrewDataIndex + 1;
            prevScrewButton.interactable = currentSelectedScrewDataIndex > 0;
        }

        private void SetScrewStageSlot()
        {
            ResetScrewStageSlots();
            if (screwData.screwStages != null)
            {
                for (int i = 0; i < screwData.screwStages.Length; i++)
                {
                    var slot = Instantiate(stageSlot, stageSlot.transform.parent);
                    slot.Init(i, screwData.screwStages[i]);
                    slot.gameObject.SetActive(true);
                    screwStageSlots.Add(slot);
                }
            }
            addScrewStageButton.SetAsLastSibling();
        }

        private void ResetScrewStageSlots()
        {
            for (int i = 0; i < screwStageSlots.Count; i++)
                Destroy(screwStageSlots[i].gameObject);
            screwStageSlots.Clear();
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        public void OnAddStageDataButtonClick()
        {
            ScrewStage[] currentStages = screwData.screwStages;
            ScrewStage[] newStages = new ScrewStage[currentStages.Length + 1];
            for (int i = 0; i < currentStages.Length; i++)
                newStages[i] = currentStages[i];
            newStages[currentStages.Length] = new ScrewStage(currentStages.Last());

            screwData.screwStages = newStages;
            SetView();
        }
        #endregion
    }
}