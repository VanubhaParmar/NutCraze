using Sirenix.OdinInspector;
using System;
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

        [Space]
        [SerializeField] private LevelEditorNutColorDataCountInfoView levelEditorNutColorDataCountInfoViewPrefab;
        [ShowInInspector, ReadOnly] private List<LevelEditorNutColorDataCountInfoView> generatedNutColorDataCountInfoViews = new List<LevelEditorNutColorDataCountInfoView>();

        private bool isViewInitDone;
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
        public override void Show(Action action = null, bool isForceShow = false)
        {
            base.Show(action, isForceShow);
            CheckForViewInit();
            SetView();

            SetNutColorCountDataViews();
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
        private void SetNutColorCountDataViews()
        {
            generatedNutColorDataCountInfoViews.ForEach(x => x.ResetView());

            Dictionary<Color, int> colorValueDict = new Dictionary<Color, int>();

            var allNutsData = LevelEditorManager.Instance.TempEditLevelDataSO.screwNutsLevelDataInfos;
            foreach (var nutDatas in allNutsData)
            {
                foreach(var nutValue in nutDatas.levelNutDataInfos)
                {
                    Color mainCol = LevelManager.Instance.GetNutTheme(nutValue.nutColorTypeId)._mainColor;

                    if (colorValueDict.ContainsKey(mainCol))
                        colorValueDict[mainCol]++;
                    else
                        colorValueDict.Add(mainCol, 1);
                }
            }

            foreach(var kvp in  colorValueDict)
            {
                var emptyView = GetInactiveNutColorDataCountInfoView() ?? GetNewNutColorDataCountInfoView();
                emptyView.InitView(kvp.Value, kvp.Key);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(levelEditorNutColorDataCountInfoViewPrefab.transform.parent.GetComponent<RectTransform>());
        }

        private LevelEditorNutColorDataCountInfoView GetInactiveNutColorDataCountInfoView()
        {
            return generatedNutColorDataCountInfoViews.Find(x => !x.gameObject.activeInHierarchy);
        }

        private LevelEditorNutColorDataCountInfoView GetNewNutColorDataCountInfoView()
        {
            var view = Instantiate(levelEditorNutColorDataCountInfoViewPrefab, levelEditorNutColorDataCountInfoViewPrefab.transform.parent);
            view.ResetView();
            generatedNutColorDataCountInfoViews.Add(view);

            return view;
        }

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
        private void LevelEditorManager_onLevelEditorNutsDataCountChanged()
        {
            SetNutColorCountDataViews();
        }
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