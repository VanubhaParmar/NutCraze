using GameAnalyticsSDK;
using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Tag.NutSort
{
    public class MainGameplayHelper : BaseGameplayHelper
    {
        #region PRIVATE_VARIABLES
        private const int Store_Gameplay_Data_Every_X_Seconds = 5;
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
            if (IsSpecialLevelProgressStored())
            {
                int specialLevelNumber = GameplayLevelProgressManager.Instance.CurrentPlayingLevel;
                PlaySpecialLevelView.Show(specialLevelNumber, LoadSpeciallLevel, LoadNormalLevel);
            }
            else
            {
                LoadNormalLevel();
            }
            GameplayView.Show();
            AutoOpenPopupHandler.Instance.OnCheckForAutoOpenPopUps();
        }

        public override void RestartGameplay()
        {
            if (IsPlayingLevel)
            {
                GameplayStateData.gameplayStateType = GameplayStateType.NONE;
                GameplayLevelProgressManager.Instance.ResetLevelProgress();
                LevelDataSO currentLevelDataSO = LevelManager.Instance.CurrentLevelDataSO;
                if (currentLevelDataSO.levelType == LevelType.SPECIAL_LEVEL)
                {
                    LoadSpeciallLevel(currentLevelDataSO.level);
                    AnalyticsManager.Instance.LogSpecialLevelDataEvent(AnalyticsConstants.LevelData_RestartTrigger, currentLevelDataSO.level);
                }
                else
                {
                    LoadNormalLevel();
                    AnalyticsManager.Instance.LogLevelDataEvent(AnalyticsConstants.LevelData_RestartTrigger);
                    AnalyticsManager.Instance.LogProgressionEvent(GAProgressionStatus.Fail);
                }
                Adjust_LogLevelFail();
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
                AdjustManager.Instance.Adjust_LevelStartEvent(LevelManager.Instance.CurrentLevelDataSO.level, LevelManager.Instance.CurrentLevelDataSO.levelType);
                TimeManager.Instance.RegisterTimerTickEvent(IncreaseLevelRunTime);
                TutorialManager.Instance.CheckForTutorialsToStart();
            }
        }

        public override void LoadSpeciallLevel(int specialLevelNumber)
        {
            ScrewSelectionHelper.Instance.ClearSelection();
            LevelManager.Instance.LoadSpecialLevel(specialLevelNumber, OnLevelLoad);
            gameplayStateData.OnGamePlayStart();

            void OnLevelLoad()
            {
                ResetGameStateData();
                AdjustManager.Instance.Adjust_LevelStartEvent(LevelManager.Instance.CurrentLevelDataSO.level, LevelManager.Instance.CurrentLevelDataSO.levelType);
                TimeManager.Instance.RegisterTimerTickEvent(IncreaseLevelRunTime);
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
            gameplayStateData.OnGameplayMove(fromScrew, toScrew, nutsTransferred);
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

        private bool IsSpecialLevelProgressStored()
        {
            GameplayLevelProgressManager gameplayLevelProgressManager = GameplayLevelProgressManager.Instance;
            return gameplayLevelProgressManager.HasLevelProgress() && gameplayLevelProgressManager.GetLevelType() == LevelType.SPECIAL_LEVEL && LevelManager.Instance.HasSpecialLevel(gameplayLevelProgressManager.CurrentPlayingLevel);
        }

        private void ResetGameStateData()
        {
            gameplayStateData.ResetGameplayStateData();
            gameplayStateData.PopulateGameplayStateData();
        }

        private void ShowGameWinView()
        {
            GameplayLevelProgressManager.Instance.ResetLevelProgress();
            LevelManager.Instance.UnLoadLevel();
            GameplayView.Hide();
            GameWinView.ShowWinView(CheckForSpecialLevelFlow);
        }

        private void IncreaseLevelRunTime(DateTime tm)
        {
            if (IsPlayingLevel)
            {
                gameplayStateData.levelRunTime++;
                if (gameplayStateData.levelRunTime % Store_Gameplay_Data_Every_X_Seconds == 0)
                    GameplayLevelProgressManager.Instance.OnLevelTimerSave();
            }
        }
        private void CheckForSpecialLevelFlow()
        {
            GameplayView.Show();
            if (LevelManager.Instance.CanLoadSpecialLevel(out int specialLevelNumber))
            {
                PlaySpecialLevelView.Show(specialLevelNumber, LoadSpeciallLevel, LoadNormalLevel);
            }
            else
            {
                LoadNormalLevel();
                AutoOpenPopupHandler.Instance.OnCheckForAutoOpenPopUps();
            }
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
            TimeManager.Instance.DeRegisterTimerTickEvent(IncreaseLevelRunTime);
            gameplayStateData.gameplayStateType = GameplayStateType.LEVEL_OVER;
            LogLevelCompleteEvent();
            GiveLevelCompleteReward();
            LevelManager.Instance.OnLevelComplete();
            VFXManager.Instance.PlayLevelCompleteAnimation(ShowGameWinView);

            void GiveLevelCompleteReward()
            {
                BaseReward levelCompleteReward = GameManager.Instance.GameMainDataSO.levelCompleteReward;
                levelCompleteReward.GiveReward();
                if (levelCompleteReward.GetRewardId() == CurrencyConstant.COINS)
                    GameStatsCollector.Instance.OnGameCurrencyChanged(CurrencyConstant.COINS, levelCompleteReward.GetAmount(), GameCurrencyValueChangedReason.CURRENCY_EARNED_THROUGH_SYSTEM);
            }
        }

        private void LogLevelCompleteEvent()
        {
            GameplayLevelProgressManager.Instance.LogLevelOverEvents();
            AnalyticsManager.Instance.LogLevelDataEvent(AnalyticsConstants.LevelData_EndTrigger);
            AnalyticsManager.Instance.LogProgressionEvent(GAProgressionStatus.Complete);
            AdjustManager.Instance.Adjust_LevelCompleteEvent(DataManager.PlayerLevel, gameplayStateData.levelRunTime);
        }

        public void Adjust_LogLevelFail()
        {
            AdjustManager.Instance.Adjust_LevelFail(PlayerPersistantData.GetMainPlayerProgressData().playerGameplayLevel, GameplayStateData.levelRunTime);
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