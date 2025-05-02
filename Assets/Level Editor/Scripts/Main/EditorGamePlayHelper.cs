#if UNITY_EDITOR
namespace Tag.NutSort.Editor
{
    public class EditorGamePlayHelper : BaseGameplayHelper
    {
        #region PRIVATE_VARIABLES
        #endregion

        #region PUBLIC_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region OVERRIDED_METHODS
        public override void Init(GameplayManager gameplayManager)
        {
            base.Init(gameplayManager);
            gameplayStateData = new GameplayStateData();
            RegisterEvents();
        }

        public override void Cleanup()
        {
            DeRegisterEvents();
            base.Cleanup();
        }

        public override void RestartGameplay()
        {
            if (IsPlayingLevel)
            {
                GameplayStateData.gameplayStateType = GameplayStateType.NONE;
                LevelProgressManager levelProgressManager = LevelProgressManager.Instance;
                int currentLevel = levelProgressManager.CurrentLevel;
                LevelType currentLevelType = levelProgressManager.CurrentLevelType;
                levelProgressManager.ResetLevelProgress();
                if (currentLevelType == LevelType.SPECIAL_LEVEL)
                    LoadSpecialLevel(currentLevel);
                else
                    LoadNormalLevel();
            }
        }

        public override void LoadLevel(LevelDataSO levelDataSO)
        {
            ScrewSelectionHelper.Instance.ClearSelection();
            LevelManager.Instance.LoadLevel(levelDataSO, CalculateGameplayState);
            gameplayStateData.OnGamePlayStart();
        }

        public override void LoadNormalLevel()
        {
            ScrewSelectionHelper.Instance.ClearSelection();
            LevelManager.Instance.LoadCurrentLevel(OnLevelLoad);
            gameplayStateData.OnGamePlayStart();

            void OnLevelLoad()
            {
                CalculateGameplayState();
            }
        }

        public override void LoadSpecialLevel(int specialLevelNumber)
        {
            ScrewSelectionHelper.Instance.ClearSelection();
            LevelManager.Instance.LoadSpecialLevel(specialLevelNumber, OnLevelLoad);
            gameplayStateData.OnGamePlayStart();

            void OnLevelLoad()
            {
                CalculateGameplayState();
            }
        }

        public override void OnNutTransferComplete(BaseScrew fromScrew, BaseScrew toScrew, int nutsTransferred)
        {
            var move = new MoveData(fromScrew.GridCellId, toScrew.GridCellId, nutsTransferred);
            LevelProgressManager.Instance.OnPlayerMoveConfirmed(move);
            if (toScrew.IsSorted())
            {
                gameplayStateData.OnNutColorSortCompletion(toScrew.PeekNut().GetNutColorType());
                CheckForLevelComplete();
            }
            gameplayStateData.CalculatePossibleNumberOfMoves();
        }
        #endregion

        #region PUBLIC_METHODS

        #endregion

        #region PRIVATE_METHODS
        private void RegisterEvents()
        {
            NutTransferHelper.Instance.RegisterOnNutTransferComplete(OnNutTransferComplete);
        }

        private void DeRegisterEvents()
        {
            NutTransferHelper.Instance.DeRegisterOnNutTransferComplete(OnNutTransferComplete);

        }

        private void CalculateGameplayState()
        {
            gameplayStateData.CalculateGameplayState();
        }

        private void CheckForLevelComplete()
        {
            if (IsLevelComplete())
                OnLevelComplete();
        }

        private bool IsLevelComplete()
        {
            return !gameplayStateData.levelNutsUniqueColorsSortCompletionState.ContainsValue(false);// All Screw Sort is Completed
        }

        private void OnLevelComplete()
        {
            gameplayStateData.gameplayStateType = GameplayStateType.LEVEL_OVER;
            VFXManager.Instance.PlayLevelCompleteAnimation(() => { });
        }
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion

        #region UNITY_EDITOR_FUNCTIONS
        #endregion
    }
}
#endif