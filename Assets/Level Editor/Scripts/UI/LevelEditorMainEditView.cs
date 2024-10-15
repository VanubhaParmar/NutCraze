using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort.LevelEditor
{
    public class LevelEditorMainEditView : BaseView
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private LevelEditorTabButtonsView levelEditorTabButtonsView;
        [SerializeField] private List<BaseView> subViews = new List<BaseView>();

        [SerializeField] private Text baseViewHeaderText;
        [SerializeField] private Text currentLevelNumberText;

        [Space]
        [SerializeField] private RectTransform editLockParent;
        [SerializeField] private Button testButton;
        [SerializeField] private Button stopTestButton;
        [SerializeField] private RectTransform topBarContentParent;


        private bool isViewInitDone;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public override void Show(Action action = null, bool isForceShow = false)
        {
            base.Show(action, isForceShow);
            CheckForViewInit();
            SetView();
        }

        public T GetSubView<T>() where T : BaseView
        {
            return subViews.Find(x => x is T) as T;
        }

        public void SetTestingMode(bool state)
        {
            editLockParent.gameObject.SetActive(state);
            testButton.gameObject.SetActive(!state);
            stopTestButton.gameObject.SetActive(state);

            LayoutRebuilder.ForceRebuildLayoutImmediate(topBarContentParent);
        }
        #endregion

        #region PRIVATE_METHODS
        private void CheckForViewInit()
        {
            if (!isViewInitDone)
            {
                subViews.ForEach(x => x.Init());
                isViewInitDone = true;
            }
        }

        private void SetView()
        {
            levelEditorTabButtonsView.InitView(OnTabChange);
            currentLevelNumberText.text = "Level : " + LevelEditorManager.Instance.TargetLevel;

            SetTestingMode(false);
        }

        private void OnTabChange(int index)
        {
            subViews.ForEach(x => {
                if (x.gameObject.activeInHierarchy)
                    x.Hide();
            });
            subViews[index].Show();
            baseViewHeaderText.text = subViews[index].gameObject.name;
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        public void OnButtonClick_TestLevel()
        {
            LevelEditorManager.Instance.Main_EnableTestingMode();
        }

        public void OnButtonClick_StopTestingLevel()
        {
            LevelEditorManager.Instance.Main_StopTestingMode();
        }

        public void OnButtonClick_SaveLevel()
        {
            LevelEditorManager.Instance.Main_SaveToMainData();
        }
        #endregion
    }
}