#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using Tag.NutSort;
using UnityEngine;
using UnityEngine.UI;

namespace tag.editor
{
    public class LevelEditorMainEditLevelView : BaseView
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private LevelEditorScrewDataEditView levelEditorScrewDataEditViewPrefab;
        [SerializeField] private ScrollRect screwViewsScrollRect;

        private List<LevelEditorScrewDataEditView> generatedLevelEditorScrewDataEditViews = new List<LevelEditorScrewDataEditView>();

        private int currentSelectedScrewIndex = -1;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public override void Show(Action action = null, bool isForceShow = false)
        {
            base.Show(action, isForceShow);
            currentSelectedScrewIndex = -1;
            SetView();
        }

        public void Show(int selectedScrewIndex)
        {
            base.Show(null, false);
            currentSelectedScrewIndex = selectedScrewIndex;
            SetView();

            OnScrewDataSelected(currentSelectedScrewIndex);
        }

        public override void Hide()
        {
            base.Hide();
            LevelEditorManager.Instance.Main_ResetScrewSelection();
        }

        public void OnScrewDataSelected(int index)
        {
            currentSelectedScrewIndex = index;
            generatedLevelEditorScrewDataEditViews.ForEach(x => x.OnScrewEditSelection(x.Index == index));
            LevelEditorManager.Instance.Main_OnScrewSelectedForEdit(index);
        }

        public void OnEditCurrentSelectedScrewData()
        {
            Hide();
            LevelEditorUIManager.Instance.GetView<LevelEditorMainEditView>().GetSubView<LevelEditorScrewNutsDataEditView>().Show(currentSelectedScrewIndex);
        }
        #endregion

        #region PRIVATE_METHODS
        private void SetView()
        {
            ResetEditViews();
            SetScrewEditViews();
        }

        private void SetScrewEditViews()
        {
            generatedLevelEditorScrewDataEditViews.ForEach(x => x.gameObject.SetActive(false));

            var screwData = LevelEditorManager.Instance.TempEditLevelDataSO.levelScrewDataInfos;
            for (int i = 0; i < screwData.Count; i++)
            {
                var screwEditView = GenerateNewScrewDataEditView();
                screwEditView.InitView(i, screwData[i]);
            }
        }

        private LevelEditorScrewDataEditView GenerateNewScrewDataEditView()
        {
            LevelEditorScrewDataEditView newView = Instantiate(levelEditorScrewDataEditViewPrefab, screwViewsScrollRect.content);
            generatedLevelEditorScrewDataEditViews.Add(newView);
            return newView;
        }

        private void ResetEditViews()
        {
            for (int i = 0; i < generatedLevelEditorScrewDataEditViews.Count; i++)
            {
                Destroy(generatedLevelEditorScrewDataEditViews[i].gameObject);
            }
            generatedLevelEditorScrewDataEditViews.Clear();
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}
#endif