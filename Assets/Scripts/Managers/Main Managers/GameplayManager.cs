using System;
using System.Collections.Generic;

namespace Tag.NutSort
{
    public class GameplayManager : SerializedManager<GameplayManager>
    {
        #region PRIVATE_VARIABLES
        private GameplayStateData gameplayStateData;
        private List<Action<BaseScrew>> onScrewSortComplete = new List<Action<BaseScrew>>();

        #endregion

        #region PUBLIC_VARIABLES
        #endregion

        #region PROPERTIES
        public GameplayStateData GameplayStateData => gameplayStateData;
        private GameplayView GameplayView => MainSceneUIManager.Instance.GetView<GameplayView>();
        private GameWinView GameWinView => MainSceneUIManager.Instance.GetView<GameWinView>();
        private PlaySpecialLevelView PlaySpecialLevelView => MainSceneUIManager.Instance.GetView<PlaySpecialLevelView>();
        public bool IsPlayingLevel => gameplayStateData.gameplayStateType == GameplayStateType.PLAYING_LEVEL;
        #endregion

        #region UNITY_CALLBACKS
        public override void Awake()
        {
            base.Awake();
            gameplayStateData = new GameplayStateData();
            RegisterEvents();
            OnLoadingDone();
        }

        public override void OnDestroy()
        {
            DeRegisterEvents();
            base.OnDestroy();
        }

        private void RegisterEvents()
        {
            NutTransferHelper.Instance.RegisterOnNutTransferComplete(OnNutTransferComplete);
        }

        private void DeRegisterEvents()
        {
            NutTransferHelper.Instance.DeRegisterOnNutTransferComplete(OnNutTransferComplete);
        }

        public void OnMainSceneLoaded()
        {
            if (LevelManager.Instance)
            {
                
            }
        }
        #endregion

        #region PUBLIC_METHODS
        private void ResetGameStateData()
        {
            gameplayStateData.ResetGameplayStateData();
            gameplayStateData.PopulateGameplayStateData();
        }

        public void ShowGameWinView()
        {
            LevelProgressManager.Instance.ResetLevelProgress();
            LevelManager.Instance.UnLoadLevel();
            GameplayView.Hide();
            GameWinView.ShowWinView(CheckForSpecialLevelFlow);
        }

        // this is only for editor
        public void LoadLevel(LevelData levelData)
        {
            ScrewSelectionHelper.Instance.ClearSelection();
            LevelManager.Instance.LoadLevel(levelData, ResetGameStateData);
            gameplayStateData.OnGamePlayStart();
        }

        public void LoadNormalLevel()
        {
            ScrewSelectionHelper.Instance.ClearSelection();
            LevelManager.Instance.LoadCurrentLevel(OnLevelLoad);
            gameplayStateData.OnGamePlayStart();

            void OnLevelLoad()
            {
                ResetGameStateData();
                TutorialManager.Instance.CheckForTutorialsToStart();
            }
        }

        public void LoadSpecailLevel(LevelData levelData)
        {
            ScrewSelectionHelper.Instance.ClearSelection();
            LevelManager.Instance.LoadSpecialLevel(levelData, OnLevelLoad);
            gameplayStateData.OnGamePlayStart();

            void OnLevelLoad()
            {
                ResetGameStateData();
                TutorialManager.Instance.CheckForTutorialsToStart();
            }
        }

        public void OnScrewSortComplete(BaseScrew baseScrew)
        {
            InvokeOnScrewSortComplete(baseScrew);
            gameplayStateData.OnNutColorSortCompletion(baseScrew.PeekNut().GetNutColorType());
            CheckForLevelComplete();
        }

        public void RegisterOnScrewSortComplete(Action<BaseScrew> action)
        {
            if (!onScrewSortComplete.Contains(action))
                onScrewSortComplete.Add(action);
        }

        public void DeRegisterOnScrewSortComplete(Action<BaseScrew> action)
        {
            if (onScrewSortComplete.Contains(action))
                onScrewSortComplete.Remove(action);
        }

        #endregion

        #region PRIVATE_METHODS
        private void InvokeOnScrewSortComplete(BaseScrew screw)
        {
            for (int i = 0; i < onScrewSortComplete.Count; i++)
                onScrewSortComplete[i]?.Invoke(screw);
        }

        private void CheckForSpecialLevelFlow()
        {
            GameplayView.Show();
            if (LevelManager.Instance.CanLoadSpecialLevel(out LevelData levelData))
            {
                PlaySpecialLevelView.Show(levelData, LoadSpecailLevel, LoadNormalLevel);
            }
            else
            {
                LoadNormalLevel();
                AutoOpenPopupHandler.Instance.OnCheckForAutoOpenPopUps();
            }
        }

        private void OnNutTransferComplete(BaseScrew fromScrew, BaseScrew toScrew, int nutsTransferred)
        {
            gameplayStateData.CalculatePossibleNumberOfMoves();
        }

        private void CheckForLevelComplete()
        {
            if (IsLevelComplete())
            {
                gameplayStateData.gameplayStateType = GameplayStateType.LEVEL_OVER;
                LevelManager.Instance.OnLevelComplete();
                VFXManager.Instance.PlayLevelCompleteAnimation(() => ShowGameWinView());
            }
        }

        private bool IsLevelComplete()
        {
            return !gameplayStateData.levelNutsUniqueColorsSortCompletionState.ContainsValue(false);// All Screw Sort is Completed
        }
        #endregion

        #region ANALYTICS_EVENTS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion

        #region UNITY_EDITOR_FUNCTIONS
        #endregion
    }
}