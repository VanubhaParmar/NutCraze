using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort
{
    public class GameplayView : BaseView
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private Text levelNumberText;

        [Space]
        [SerializeField] private Text undoBoosterCountText;
        [SerializeField] private RectTransform undoBoosterAdWatchParent;

        [Space]
        [SerializeField] private Text extraScrewBoosterCountText;
        [SerializeField] private RectTransform extraScrewBoosterAdWatchParent;

        [SerializeField] private List<RectTransform> rebuildTransforms;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public override void Show(Action action = null, bool isForceShow = false)
        {
            base.Show(action, isForceShow);
            SetView();
        }
        #endregion

        #region PRIVATE_METHODS
        private void SetView()
        {
            var playerData = PlayerPersistantData.GetMainPlayerProgressData();

            levelNumberText.text = "Level " + LevelManager.Instance.CurrentLevelDataSO.level;

            undoBoosterCountText.text = playerData.undoBoostersCount + "";
            undoBoosterAdWatchParent.gameObject.SetActive(playerData.undoBoostersCount == 0);

            extraScrewBoosterCountText.text = playerData.extraScrewBoostersCount + "";
            extraScrewBoosterCountText.gameObject.SetActive(playerData.extraScrewBoostersCount != 0);
            extraScrewBoosterAdWatchParent.gameObject.SetActive(playerData.extraScrewBoostersCount == 0);

            rebuildTransforms.ForEach(x => LayoutRebuilder.ForceRebuildLayoutImmediate(x));
            UIUtilityEvents.RaiseOnRefreshUIRects();
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        public void OnButtonClick_ReloadLevel()
        {
            GameplayManager.Instance.OnReloadCurrentLevel();
        }

        public void OnButtonClick_UndoBooster()
        {
            if (GameplayManager.Instance.CanUseUndoBooster())
                GameplayManager.Instance.UseUndoBooster();
            else if (!DataManager.Instance.CanUseUndoBooster())
                GameManager.Instance.AddUndoBoosters();

            SetView();
        }

        public void OnButtonClick_ExtraScrewBooster()
        {
            if (GameplayManager.Instance.CanUseExtraScrewBooster())
                GameplayManager.Instance.UseExtraScrewBooster();
            else if (!DataManager.Instance.CanUseExtraScrewBooster())
                GameManager.Instance.AddExtraScrewBoosters();

            SetView();
        }
        #endregion
    }
}