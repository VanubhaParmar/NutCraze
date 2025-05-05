using GameAnalyticsSDK;

namespace Tag.NutSort
{
    public class MainGameplayHelper : BaseGameplayHelper
    {
        #region PRIVATE_VARIABLES
        #endregion

        #region PUBLIC_VARIABLES
        #endregion

        #region PROPERTIES
        private GameplayView GameplayView => MainSceneUIManager.Instance.GetView<GameplayView>();
        private GameWinView GameWinView => MainSceneUIManager.Instance.GetView<GameWinView>();
        private PlaySpecialLevelView PlaySpecialLevelView => MainSceneUIManager.Instance.GetView<PlaySpecialLevelView>();
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

        public override void StartGameplay()
        {
            if (IsLevelProgressStored())
                LoadSavedLevel();
            else
                StartNewLevel();
            GameplayView.Show();
            AutoOpenPopupHandler.Instance.OnCheckForAutoOpenPopUps();
        }

        public override void RestartGameplay()
        {
            if (IsPlayingLevel)
            {
                GameplayStateData.gameplayStateType = GameplayStateType.NONE;
                int currentLevel = LevelProgressManager.Instance.CurrentLevel;
                LevelType currentLevelType = LevelProgressManager.Instance.CurrentLevelType;
                int levelRunTime = LevelProgressManager.Instance.LevelSaveData.runTime;
                LogLevelRestartEvents(currentLevelType, currentLevel, levelRunTime);
                LevelProgressManager.Instance.ResetLevelProgress();

                if (currentLevelType == LevelType.SPECIAL_LEVEL)
                    LoadSpecialLevel(currentLevel);
                else
                    LoadNormalLevel();
            }
            LevelManager.Instance.OnLevelRestart();

            void LogLevelRestartEvents(LevelType levelType, int level, int levelRunTime)
            {
                if (levelType == LevelType.NORMAL_LEVEL)
                {
                    AdjustManager.Instance.Adjust_LevelFail(level, levelRunTime);
                    AnalyticsManager.Instance.LogLevelDataEvent(AnalyticsConstants.LevelData_RestartTrigger);
                    AnalyticsManager.Instance.LogProgressionEvent(GAProgressionStatus.Fail);
                }
                else
                {
                    AnalyticsManager.Instance.LogSpecialLevelDataEvent(AnalyticsConstants.LevelData_RestartTrigger, level);
                }
            }
        }

        public override void EndGameplay()
        {
            OnLevelComplete();
        }

        public override void LoadLevel(LevelDataSO levelDataSO)
        {
            ScrewSelectionHelper.Instance.ClearSelection();
            LevelManager.Instance.LoadLevel(levelDataSO, ResetGameStateData);
            gameplayStateData.OnGamePlayStart();
        }

        public override void LoadNormalLevel()
        {
            ScrewSelectionHelper.Instance.ClearSelection();
            LevelManager.Instance.LoadCurrentLevel(OnLevelLoad);
            gameplayStateData.OnGamePlayStart();

            void OnLevelLoad()
            {
                ResetGameStateData();
                LevelProgressManager.Instance.StartLevelTimer();
                TutorialManager.Instance.CheckForTutorialsToStart();
            }
        }

        public override void LoadSpecialLevel(int specialLevelNumber)
        {
            ScrewSelectionHelper.Instance.ClearSelection();
            LevelManager.Instance.LoadSpecialLevel(specialLevelNumber, OnLevelLoad);
            gameplayStateData.OnGamePlayStart();

            void OnLevelLoad()
            {
                ResetGameStateData();
                LevelProgressManager.Instance.StartLevelTimer();
                TutorialManager.Instance.CheckForTutorialsToStart();
            }
        }

        public override void LoadSavedLevel()
        {
            ScrewSelectionHelper.Instance.ClearSelection();
            LevelManager.Instance.LoadSavedLevel(OnLevelLoad);
            gameplayStateData.OnGamePlayStart();

            void OnLevelLoad()
            {
                ResetGameStateData();
                LevelProgressManager.Instance.StartLevelTimer();
                TutorialManager.Instance.CheckForTutorialsToStart();
            }
        }

        public override void OnScrewSortComplete(BaseScrew baseScrew)
        {
            gameplayStateData.OnNutColorSortCompletion(baseScrew.PeekNut().GetNutColorType());
            CheckForLevelComplete();
        }

        public override void OnNutTransferComplete(BaseScrew fromScrew, BaseScrew toScrew, int nutsTransferred)
        {
            //var move = new MoveData(fromScrew.GridCellId, toScrew.GridCellId, nutsTransferred);
            //LevelProgressManager.Instance.OnPlayerMoveConfirmed(move);
            gameplayStateData.CalculatePossibleNumberOfMoves();
            LevelFailManager.Instance.CheckForLevelFail();
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

        private void CheckForLevelComplete()
        {
            if (IsLevelComplete())
                OnLevelComplete();
        }
        private bool IsLevelProgressStored()
        {
            LevelProgressManager levelProgressManager = LevelProgressManager.Instance;
            if (levelProgressManager.IsLevelProgressDataExist)
            {
                if (levelProgressManager.CurrentLevelType == LevelType.SPECIAL_LEVEL)
                {
                    return LevelManager.Instance.HasSpecialLevel(levelProgressManager.CurrentLevel);
                }
                else if (levelProgressManager.CurrentLevelType == LevelType.NORMAL_LEVEL)
                {
                    return true;
                }
            }
            return false;
        }

        private void ResetGameStateData()
        {
            gameplayStateData.CalculateGameplayState();
        }

        private void ShowGameWinView()
        {
            LevelProgressManager.Instance.ResetLevelProgress();
            LevelManager.Instance.UnLoadLevel();
            GameplayView.Hide();
            GameWinView.ShowWinView(() =>
            {
                GameplayView.Show();
                StartNewLevel();
                AutoOpenPopupHandler.Instance.OnCheckForAutoOpenPopUps();

            });
        }


        private void StartNewLevel()
        {
            int currentNormalLevel = DataManager.PlayerLevel;
            int nextSpecialLevelIndex = DataManager.PlayerSpecialLevel;
            LevelType lastPlayedLevelType = DataManager.LastPlayedLevelType;

            bool isSpecialLevelPlayed = lastPlayedLevelType == LevelType.SPECIAL_LEVEL;
            bool isSpecialLevelTiming = LevelManager.Instance.CanLoadSpecialLevel(currentNormalLevel);
            bool doesNextSpecialLevelExist = LevelManager.Instance.HasSpecialLevel(nextSpecialLevelIndex);

            if (isSpecialLevelTiming && doesNextSpecialLevelExist && !isSpecialLevelPlayed)
                PlaySpecialLevelView.Show(nextSpecialLevelIndex, LoadSpecialLevel, LoadNormalLevel);
            else
                LoadNormalLevel();

            LogLevelStartEvent();

            void LogLevelStartEvent()
            {
                LevelProgressManager levelProgressManager = LevelProgressManager.Instance;
                AdjustManager.Instance.Adjust_LevelStartEvent(levelProgressManager.CurrentLevel, levelProgressManager.CurrentLevelType);
                if (levelProgressManager.CurrentLevelType == LevelType.NORMAL_LEVEL)
                {
                    AnalyticsManager.Instance.LogLevelDataEvent(AnalyticsConstants.LevelData_StartTrigger);
                    AnalyticsManager.Instance.LogProgressionEvent(GAProgressionStatus.Start);
                }
            }
        }

        private bool IsLevelComplete()
        {
            return !gameplayStateData.levelNutsUniqueColorsSortCompletionState.ContainsValue(false);// All Screw Sort is Completed
        }

        private void OnLevelComplete()
        {
            LevelProgressManager.Instance.StopLevelTimer();
            gameplayStateData.gameplayStateType = GameplayStateType.LEVEL_OVER;
            LevelProgressManager.Instance.LogLevelOverEvents();
            GiveLevelCompleteReward();
            LevelManager.Instance.OnLevelComplete();
            VFXManager.Instance.PlayLevelCompleteAnimation(ShowGameWinView);

            void GiveLevelCompleteReward()
            {
                BaseReward levelCompleteReward = GameManager.Instance.GameMainDataSO.levelCompleteReward;
                levelCompleteReward.GiveReward();
                if (levelCompleteReward.GetRewardId() == CurrencyConstant.COIN)
                    GameStatsCollector.Instance.OnGameCurrencyChanged(CurrencyConstant.COIN, levelCompleteReward.GetAmount(), CurrencyChangeReason.EARNED_THROUGH_SYSTEM);
            }
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