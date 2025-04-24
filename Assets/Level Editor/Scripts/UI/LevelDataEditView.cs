using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort.LevelEditor
{
    public class LevelDataEditView : BaseView
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private Transform addStageButton;
        [SerializeField] private LevelStageDataEditView stageDataEditView;
        private LevelData levelData;
        private List<LevelStageDataEditView> stageDataEditViews = new List<LevelStageDataEditView>();
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        [Button]
        public void Show(LevelData levelData)
        {
            this.levelData = levelData;
            ////this.levelData = LevelEditorManager.CurrentLevelData;
            this.levelData = ProtoLevelDataFactory.GetLevelData(ABTestType.Default, LevelType.NORMAL_LEVEL, 20);
            base.Show();
            SetView();
        }
        #endregion

        #region PRIVATE_METHODS
        public void SetView()
        {
            if (levelData != null)
            {
                if (levelData.stages == null)
                    levelData.stages = new LevelStage[0];

                ResetStageDataViews();
                for (int i = 0; i < levelData.stages.Length; i++)
                {
                    LevelStageDataEditView tmp = Instantiate(stageDataEditView, stageDataEditView.transform.parent);
                    tmp.gameObject.SetActive(true);
                    tmp.Init(i, levelData.stages[i]);
                    stageDataEditViews.Add(tmp);
                }
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(stageDataEditView.transform.parent as RectTransform);
            addStageButton.SetAsLastSibling();
        }

        private void ResetStageDataViews()
        {
            for (int i = 0; i < stageDataEditViews.Count; i++)
                Destroy(stageDataEditViews[i].gameObject);
            stageDataEditViews.Clear();
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS

        public void OnAddNewStageButtonClick()
        {
            LevelStage[] currentStage = levelData.stages;
            levelData.stages = new LevelStage[currentStage.Length + 1];
            for (int i = 0; i < currentStage.Length; i++)
                levelData.stages[i] = currentStage[i];

            LevelStage levelStage = currentStage.Last();
            levelData.stages[currentStage.Length] = new LevelStage(levelStage);
            SetView();
        }

        public void OnPasteButtonClick()
        {
            string json = EditorGUIUtility.systemCopyBuffer;
            try
            {
                LevelData levelData = JsonConvert.DeserializeObject<LevelData>(json);
                this.levelData = levelData;
                SetView();
            }
            catch (Exception e)
            {
                Debug.LogError("Invalid LevelStage JSON in clipboard " + e);
                LevelEditorToastsView.Instance.ShowToastMessage("<color=red>Invalid LevelData JSON in clipboard</color>");
            }
        }

        public void OnCopyButtonClick()
        {
            string json = JsonConvert.SerializeObject(this.levelData);
            EditorGUIUtility.systemCopyBuffer = json;
        }
        #endregion
    }
}