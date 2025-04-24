using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort.LevelEditor
{
    public class LevelStageDataEditView : MonoBehaviour
    {
        #region PRIVATE_VARIABLES
        [SerializeField] private ScrewDataEditView screwDataEditView;
        [SerializeField] private Dropdown arrangementDropdown;
        [SerializeField] private Text stageIndexText;

        private LevelStage levelStage;
        private int levelStageIndex;
        private List<ScrewDataEditView> screwDataEditViews = new List<ScrewDataEditView>();

        #endregion

        #region PUBLIC_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void Init(int levelStageIndex, LevelStage levelStage)
        {
            this.levelStage = levelStage;
            this.levelStageIndex = levelStageIndex;
            SetView();
        }
        #endregion

        #region PRIVATE_METHODS
        private void SetView()
        {
            stageIndexText.text = "Level Stage- " + (levelStageIndex + 1);
            SetArrangementDropdown();
            ResetEditViews();
            SetScrewEditViews();
        }

        private void SetScrewEditViews()
        {
            for (int i = 0; i < levelStage.screwDatas.Length; i++)
            {
                var screwEditView = Instantiate(screwDataEditView, screwDataEditView.transform.parent);
                screwEditView.gameObject.SetActive(true);
                screwEditView.InitView(levelStageIndex, i, levelStage.screwDatas[i]);
                screwDataEditViews.Add(screwEditView);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(screwDataEditView.transform.parent as RectTransform);
        }

        private void ResetEditViews()
        {
            for (int i = 0; i < screwDataEditViews.Count; i++)
                Destroy(screwDataEditViews[i].gameObject);
            screwDataEditViews.Clear();
        }

        private void SetArrangementDropdown()
        {
            //BaseIDMappingConfig idMapping = LevelEditorManager.LevelArrangementIdMaaping;
            //var currentArrangement = levelStage.arrangementId;
            //List<string> options = idMapping.GetListOfNames();
            //arrangementDropdown.ClearOptions();
            //arrangementDropdown.AddOptions(options);
            //string currentOption = idMapping.GetNameFromId(currentArrangement);
            //arrangementDropdown.SetValueWithoutNotify(options.IndexOf(currentOption));
        }

        private void SetScrewsOnArrangementChange()
        {
            //LevelArrangementConfigDataSO levelArrangementConfigDataSO = LevelEditorManager.Instance.GetArrangementConfigDataSO(levelStage.arrangementId);
            //ScrewData[] currentScrewData = levelStage.screwDatas;

            //int newScrewCount = levelArrangementConfigDataSO.arrangementCellIds.Count;
            //ScrewData[] newScrewData = new ScrewData[newScrewCount];

            //for (int i = 0; i < newScrewCount; i++)
            //{
            //    if (i < currentScrewData.Length)
            //        newScrewData[i] = currentScrewData[i];
            //    else
            //        newScrewData[i] = new ScrewData();
            //}

            ResetEditViews();
            SetScrewEditViews();
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        public void OnArrangementDropdownValueChanged()
        {
            //int index = arrangementDropdown.value;
            //BaseIDMappingConfig idMapping = LevelEditorManager.LevelArrangementIdMaaping;
            //levelStage.arrangementId = idMapping.GetIdFromName(arrangementDropdown.options[index].text);
            //SetScrewsOnArrangementChange();
        }

        public void OnEditButtonClick()
        {
        }

        public void OnRemoveButtonClick()
        {
            LevelEditorManager.Instance.RemoveLevelStage(levelStageIndex);
        }

        public void OnSaveButtonClick()
        {
        }

        public void OnPasteButtonClick()
        {
            string json = EditorGUIUtility.systemCopyBuffer;
            try
            {
                LevelStage stage = JsonConvert.DeserializeObject<LevelStage>(json);
                this.levelStage = stage;
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
            string json = JsonConvert.SerializeObject(this.levelStage);
            EditorGUIUtility.systemCopyBuffer = json;
        }
        #endregion
    }
}