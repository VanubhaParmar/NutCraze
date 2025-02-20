using GameAnalyticsSDK;
using System;

namespace Tag.NutSort
{
    public class MainGamePlayHandler : BaseGamePlayHandler
    {
        #region PRIVATE_VARS
        #endregion

        #region PUBLIC_VARS
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_FUNCTIONS

        public override void StartNextOrCurrentLevel()
        {
            StartLevel(ResourceManager.Instance.GetLevelData(DataManager.PlayerLevel.Value));
        }

        public override void StartLevel(LevelDataSO levelDataSO)
        {
            LoadLevel(levelDataSO, OnLevelLoadComplete);

            void OnLevelLoadComplete()
            {
                ResetGameStateData();
                AdjustManager.Instance.Adjust_LevelStartEvent(LevelManager.Instance.LoadedLevel.Value, LevelManager.Instance.CurrentLevelDataSO.LevelType);
                TimeManager.Instance.RegisterTimerTickEvent(IncreaseLevelRunTime);
            }
        }

        public override void OnLevelRestart()
        {
            AnalyticsManager.Instance.LogLevelDataEvent(AnalyticsConstants.LevelData_RestartTrigger);
            AnalyticsManager.Instance.LogProgressionEvent(GAProgressionStatus.Fail);
            ResetGameStateData();
            TimeManager.Instance.RegisterTimerTickEvent(IncreaseLevelRunTime);
        }

        public override void OnLevelComplete()
        {
            TimeManager.Instance.DeRegisterTimerTickEvent(IncreaseLevelRunTime);
            gameplayStateData.gameplayStateType = GameplayStateType.LEVEL_OVER;
            BaseReward levelCompleteReward = GameManager.Instance.GameMainDataSO.levelCompleteReward;
            levelCompleteReward.GiveReward();
            if (levelCompleteReward.GetRewardId() == CurrencyConstant.COIN)
                GameStatsCollector.Instance.OnGameCurrencyChanged(CurrencyConstant.COIN, levelCompleteReward.GetAmount(), GameCurrencyValueChangedReason.CURRENCY_EARNED_THROUGH_SYSTEM);
            LogLevelFinishEvent();
            DataManager.PlayerLevel.Add(1);
            VFXManager.Instance.PlayLevelCompleteAnimation(() => ShowGameWinView(levelCompleteReward));
        }


        public override void OnLevelFail()
        {
            var currency = DataManager.Instance.GetCurrency(CurrencyConstant.LIFE);
            currency.Add(-1);
        }

        public override void OnLevelFailRetry()
        {

        }

        public override void OnExit()
        {
            ScrewSelectionHelper.Instance.ClearSelection();
            LevelManager.Instance.UnLoadLevel();
            MainSceneUIManager.Instance.GetView<GameplayView>().Hide();
            MainSceneUIManager.Instance.GetView<MainView>().Show();
            //MainSceneUIManager.Instance.GetView<BottombarView>().Show();
        }

        public override void OnRetry()
        {
            AnalyticsManager.Instance.LogProgressionEvent(GAProgressionStatus.Fail);
            var currency = DataManager.Instance.GetCurrency(CurrencyConstant.LIFE);
            currency.Add(-1);
            AdManager.Instance.ShowInterstitial(InterstatialAdPlaceType.Reload_Level, "GameRestartInterstitial");
        }

        public override void OnNutTransferComplete(BaseScrew fromScrew, BaseScrew toScrew, int nutsTransferred)
        {
            gameplayStateData.OnGameplayMove(fromScrew, toScrew, nutsTransferred);
            gameplayStateData.CalculatePossibleNumberOfMoves();
        }

        public override void OnScrewSortComplete(BaseScrew baseScrew)
        {
            NutsHolderScrewBehaviour currentSelectedScrewNutsHolder = baseScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();
            gameplayStateData.OnNutColorSortCompletion(currentSelectedScrewNutsHolder.PeekNut().GetNutColorType());
            CheckForLevelComplete();
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        private void IncreaseLevelRunTime(DateTime date)
        {
            if (gameplayStateData.gameplayStateType == GameplayStateType.PLAYING_LEVEL)
                gameplayStateData.levelRunTime++;
        }

        private void LoadLevel(LevelDataSO levelDataSO, Action onLoadComplete)
        {
            GlobalUIManager.Instance.GetView<InGameLoadingView>().ShowView(1f, () =>
            {
                ScrewSelectionHelper.Instance.ClearSelection();
                LevelManager.Instance.LoadLevel(levelDataSO, onLoadComplete);
                TutorialManager.Instance.CheckForTutorialsToStart();
            });
        }

        private void ResetGameStateData()
        {
            gameplayStateData.OnGamePlayStart();
            gameplayStateData.ResetGameplayStateData();
            gameplayStateData.PopulateGameplayStateData();
        }

        private void CheckForLevelComplete()
        {
            if (!gameplayStateData.levelNutsUniqueColorsSortCompletionState.ContainsValue(false)) // All Screw Sort is Completed
            {
                OnLevelComplete();
                LevelManager.Instance.OnLevelComplete();
            }
        }

        private void CheckForLevelFail()
        {
            if (gameplayStateData.possibleMovesInfo.Count == 0)
                OnLevelFail();

        }

        private void LogLevelFinishEvent()
        {
            AnalyticsManager.Instance.LogLevelDataEvent(AnalyticsConstants.LevelData_EndTrigger);
            AnalyticsManager.Instance.LogProgressionEvent(GAProgressionStatus.Complete);
            AdjustManager.Instance.Adjust_LevelCompleteEvent(DataManager.PlayerLevel.Value, gameplayStateData.levelRunTime);
        }

        public void ShowGameWinView(BaseReward levelCompleteReward)
        {
            LevelManager.Instance.UnLoadLevel();
            MainSceneUIManager.Instance.GetView<GameplayView>().Hide();
            MainSceneUIManager.Instance.GetView<GameWinView>().ShowWinView(OnRewardClaim, levelCompleteReward);
        }

        public void OnRewardClaim()
        {
            MainSceneUIManager.Instance.GetView<GameplayView>().Show();
            StartNextOrCurrentLevel();
            AutoOpenPopupHandler.Instance.OnCheckForAutoOpenPopUps();
        }

        #endregion

        #region CO-ROUTINES
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region UI_CALLBACKS
        #endregion

        #region EDITOR
#if UNITY_EDITOR
#endif
        #endregion
    }
}
