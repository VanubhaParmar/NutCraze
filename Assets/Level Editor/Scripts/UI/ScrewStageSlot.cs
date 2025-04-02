using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort.LevelEditor
{
    public class ScrewStageSlot : MonoBehaviour
    {
        #region PRIVATE_VARIABLES

        [SerializeField] private Text screwHeaderText;
        [SerializeField] private RectTransform addNutButton;
        [SerializeField] private Toggle storageToggle;
        [SerializeField] private Toggle refreshToggle;
        [SerializeField] private Toggle generatorTogle;
        [SerializeField] private Dropdown colorDropdown;
        [SerializeField] private Dropdown curtainColorDropdown;
        [SerializeField] private NutDataEditView nutdataEditViewPrefab;

        private List<NutDataEditView> nutdataEditViews = new List<NutDataEditView>();
        private int index = -1;
        private ScrewStage screwStage;
        #endregion

        #region PUBLIC_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void Init(int index, ScrewStage screwStage)
        {
            this.index = index;
            this.screwStage = screwStage;
            SetView();
        }
        #endregion

        #region PRIVATE_METHODS

        private void SetView()
        {
            screwHeaderText.text = (index) + ". Screw Stage";
            SetNutsEditViews();
            SetDropdowns();
            SetToggles();
        }

        private void SetNutsEditViews()
        {
            ResetNutEditViews();
            if (screwStage.nutDatas != null)
            {
                for (int i = 0; i < screwStage.nutDatas.Length; i++)
                {
                    var nutsEditView = Instantiate(nutdataEditViewPrefab, nutdataEditViewPrefab.transform.parent);
                    nutsEditView.InitView(i, index, screwStage.nutDatas[i]);
                    gameObject.SetActive(true);
                    nutdataEditViews.Add(nutsEditView);
                }
            }
            addNutButton.SetAsLastSibling();
        }

        private void SetDropdowns()
        {
            colorDropdown.ClearOptions();
            curtainColorDropdown.ClearOptions();
            BaseIDMappingConfig colorIdMapping = LevelEditorManager.ColorIdMapping;
            List<string> options = colorIdMapping.GetListOfNames();
            colorDropdown.AddOptions(options);
            curtainColorDropdown.AddOptions(options);

            colorDropdown.SetValueWithoutNotify(options.IndexOf(colorIdMapping.GetNameFromId(screwStage.color)));
            curtainColorDropdown.SetValueWithoutNotify(options.IndexOf(colorIdMapping.GetNameFromId(screwStage.curtainColor)));

            colorDropdown.onValueChanged.AddListener((value) =>
            {
                screwStage.color = colorIdMapping.GetIdFromName(colorDropdown.options[value].text);
            });

            curtainColorDropdown.onValueChanged.AddListener((value) =>
            {
                screwStage.curtainColor = colorIdMapping.GetIdFromName(curtainColorDropdown.options[value].text);
            });
        }

        private void SetToggles()
        {
            storageToggle.isOn = screwStage.isStorage;
            refreshToggle.isOn = screwStage.isRefresh;
            generatorTogle.isOn = screwStage.isGenerator;

            storageToggle.onValueChanged.AddListener((value) =>
            {
                screwStage.isStorage = value;
            });

            refreshToggle.onValueChanged.AddListener((value) =>
            {
                screwStage.isRefresh = value;
            });

            generatorTogle.onValueChanged.AddListener((value) =>
            {
                screwStage.isGenerator = value;
            });
        }

        private void ResetNutEditViews()
        {
            for (int i = 0; i < nutdataEditViews.Count; i++)
            {
                Destroy(nutdataEditViews[i].gameObject);
            }
            nutdataEditViews.Clear();
        }

        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        public void OnPasteButtonClick()
        {
            string json = EditorGUIUtility.systemCopyBuffer;
            try
            {
                ScrewStage screwStage = JsonConvert.DeserializeObject<ScrewStage>(json);
                this.screwStage = screwStage;
                SetView();
            }
            catch (Exception e)
            {
                Debug.LogError("Invalid LevelStage JSON in clipboard " + e);
                LevelEditorToastsView.Instance.ShowToastMessage("<color=red>Invalid LevelStage JSON in clipboard</color>");
            }
        }

        public void OnCopyButtonClick()
        {
            string json = JsonConvert.SerializeObject(this.screwStage);
            EditorGUIUtility.systemCopyBuffer = json;
        }
        #endregion
    }
}
