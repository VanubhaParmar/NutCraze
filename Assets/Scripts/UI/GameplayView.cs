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
        [SerializeField] private Text undoBoosterAdWatchText;

        [Space]
        [SerializeField] private Text extraScrewBoosterCountText;
        [SerializeField] private Text extraScrewBoosterAdWatchCountText;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        private void OnEnable()
        {
            GameplayManager.onGameplayLevelLoadComplete += GameplayManager_onGameplayLevelLoadComplete;
        }

        private void OnDisable()
        {
            GameplayManager.onGameplayLevelLoadComplete -= GameplayManager_onGameplayLevelLoadComplete;
        }
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

            bool isSpecialLevel = LevelManager.Instance.CurrentLevelDataSO.levelType == LevelType.SPECIAL_LEVEL;

            levelNumberText.text = isSpecialLevel ? $"Special Level {LevelManager.Instance.CurrentLevelDataSO.level}" : $"Level {LevelManager.Instance.CurrentLevelDataSO.level}";

            undoBoosterCountText.text = playerData.undoBoostersCount + "";
            undoBoosterAdWatchText.text = "+" + GameManager.Instance.GameMainDataSO.undoBoostersCountToAddOnAdWatch;
            undoBoosterCountText.transform.parent.gameObject.SetActive(playerData.undoBoostersCount != 0);
            undoBoosterAdWatchText.transform.parent.gameObject.SetActive(playerData.undoBoostersCount == 0);

            extraScrewBoosterCountText.text = playerData.extraScrewBoostersCount + "";
            extraScrewBoosterAdWatchCountText.text = "+" + GameManager.Instance.GameMainDataSO.extraScrewBoostersCountToAddOnAdWatch;
            extraScrewBoosterCountText.transform.parent.gameObject.SetActive(playerData.extraScrewBoostersCount != 0);
            extraScrewBoosterAdWatchCountText.transform.parent.gameObject.SetActive(playerData.extraScrewBoostersCount == 0);
        }
        #endregion

        #region EVENT_HANDLERS
        private void GameplayManager_onGameplayLevelLoadComplete()
        {
            SetView();
        }
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        public void OnButtonClick_ReloadLevel()
        {
            GameplayManager.Instance.OnReloadCurrentLevel();
        }

        public void OnButtonClick_NoAdsPack()
        {
        }

        public void OnButtonClick_Settings()
        {
        }

        public void OnButtonClick_Shop()
        {
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