using GameAnalyticsSDK;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class GameplayManager : SerializedManager<GameplayManager>
    {
        #region PUBLIC_VARIABLES
        public GameplayStateData GameplayStateData => gameplayStateData;
        #endregion

        #region PRIVATE_VARIABLES
        [ShowInInspector, ReadOnly] private GameplayStateData gameplayStateData;
        private const int Store_Gameplay_Data_Every_X_Seconds = 5;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        public override void Awake()
        {
            base.Awake();
            InitGameplayManager();
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
        #endregion

        #region PUBLIC_METHODS
        public void InitGameplayManager()
        {
            gameplayStateData = new GameplayStateData();
            onGameplayLevelOver += OnLevelOver;

            if (DataManager.Instance.isFirstSession)
            {
                LogLevelStartEvent();
                LogCoinRewardFaucetEvent(AnalyticsConstants.ItemId_Default, DataManager.Instance.GetDefaultCurrencyAmount(CurrencyConstant.COINS));
            }
        }

        public void StartGame()
        {
            ScrewSelectionHelper.Instance.ClearSelection();
            gameplayStateData.OnGamePlayStart();
            RaiseOnGameplayLevelStart();
            TutorialManager.Instance.CheckForTutorialsToStart();
        }

        public void ResumeGame()
        {
            ScrewSelectionHelper.Instance.ClearSelection();
            gameplayStateData.OnGamePlayStart();
            RaiseOnGameplayLevelResume();
            TutorialManager.Instance.CheckForTutorialsToStart();
        }

        public void OnLevelOver()
        {
            gameplayStateData.gameplayStateType = GameplayStateType.LEVEL_OVER;

            GameManager.Instance.GameMainDataSO.levelCompleteReward.GiveReward();
            if (GameManager.Instance.GameMainDataSO.levelCompleteReward.GetRewardType() == RewardType.Currency)
                GameStatsCollector.Instance.OnGameCurrencyChanged((int)CurrencyType.Coin, GameManager.Instance.GameMainDataSO.levelCompleteReward.GetAmount(), GameCurrencyValueChangedReason.CURRENCY_EARNED_THROUGH_SYSTEM);

            if (LevelManager.Instance.CurrentLevelDataSO.levelType == LevelType.NORMAL_LEVEL)
            {
                LogLevelFinishEvent();

                var pData = PlayerPersistantData.GetMainPlayerProgressData();
                pData.playerGameplayLevel++;
                PlayerPersistantData.SetMainPlayerProgressData(pData);
            }
            else
            {
                LogSpecialLevelFinishEvent();
            }
            Adjust_LogLevelFinishEvent();

            GameplayLevelProgressManager.Instance.OnResetLevelProgress();

            VFXManager.Instance.PlayLevelCompleteAnimation(() => ShowGameWinView());
        }

        public void ShowGameWinView()
        {
            RaiseOnLevelRecycle();
            MainSceneUIManager.Instance.GetView<GameplayView>().Hide();
            MainSceneUIManager.Instance.GetView<GameWinView>().ShowWinView(CheckForSpecialLevelFlow);
        }

        public void CheckForSpecialLevelFlow()
        {
            MainSceneUIManager.Instance.GetView<GameplayView>().Show();

            int currentPlayerLevel = PlayerPersistantData.GetMainPlayerProgressData().playerGameplayLevel;
            int specialLevelNumber = GameManager.Instance.GameMainDataSO.GetSpecialLevelNumberCountToLoad(currentPlayerLevel);

            bool isPlayingSpecialLevel = LevelManager.Instance.CurrentLevelDataSO.levelType == LevelType.SPECIAL_LEVEL && LevelManager.Instance.CurrentLevelDataSO.level == specialLevelNumber;

            if (!isPlayingSpecialLevel && GameManager.Instance.GameMainDataSO.CanLoadSpecialLevel(currentPlayerLevel) && LevelManager.Instance.DoesSpecialLevelExist(specialLevelNumber))
            {
                MainSceneUIManager.Instance.GetView<PlaySpecialLevelView>().Show(specialLevelNumber,
                    () => OnLoadSpecialLevelAndStartGame(specialLevelNumber),
                    () => OnLoadCurrentReachedLevelAndStartGame());
            }
            else
            {
                OnLoadCurrentReachedLevelAndStartGame();
                AutoOpenPopupHandler.Instance.OnCheckForAutoOpenPopUps();
            }
        }

        public void OnLoadCurrentReachedLevel()
        {
            LevelManager.Instance.LoadCurrentLevel();
            OnLevelLoadComplete();
        }

        public void OnLevelLoadComplete()
        {
            Adjust_LogLevelEvent();

            gameplayStateData.ResetGameplayStateData();
            gameplayStateData.PopulateGameplayStateData();

            RaiseOnGameplayLevelLoadComplete();
        }

        public void OnLoadCurrentReachedLevelAndStartGame(bool playLevelLoadAnimation = true)
        {
            OnLoadCurrentReachedLevel();

            if (playLevelLoadAnimation)
                VFXManager.Instance.PlayLevelLoadAnimation(StartGame);
            else
                StartGame();
        }

        public void OnLoadSpecialLevelAndStartGame(int specialLevelNumber, bool playLevelLoadAnimation = true, bool isResumeGame = false)
        {
            LevelManager.Instance.LoadSpecialLevel(specialLevelNumber);
            OnLevelLoadComplete();

            Action loadAnimationAction = () =>
            {
                if (isResumeGame)
                    ResumeGame();
                else
                    StartGame();
            };

            if (playLevelLoadAnimation)
                VFXManager.Instance.PlayLevelLoadAnimation(loadAnimationAction);
            else
                loadAnimationAction?.Invoke();
        }

        public void OnReloadCurrentLevel()
        {
            if (gameplayStateData.gameplayStateType == GameplayStateType.PLAYING_LEVEL)
            {
                gameplayStateData.gameplayStateType = GameplayStateType.NONE;
                GameplayLevelProgressManager.Instance.OnResetLevelProgress();

                if (LevelManager.Instance.CurrentLevelDataSO.levelType == LevelType.SPECIAL_LEVEL)
                {
                    OnLoadSpecialLevelAndStartGame(LevelManager.Instance.CurrentLevelDataSO.level);
                    LogSpecialLevelRestartEvent();
                }
                else
                {
                    OnLoadCurrentReachedLevelAndStartGame();
                    LogLevelRestartEvent();
                }

                RaiseOnGameplayLevelReload();
            }
        }

        public void IncreaseLevelRunTime()
        {
            if (gameplayStateData.gameplayStateType == GameplayStateType.PLAYING_LEVEL)
            {
                gameplayStateData.levelRunTime++;
                if (gameplayStateData.levelRunTime % Store_Gameplay_Data_Every_X_Seconds == 0)
                    GameplayLevelProgressManager.Instance.OnLevelTimerSave();
            }
        }

        public void CalculatePossibleNumberOfMoves()
        {
            gameplayStateData.CalculatePossibleNumberOfMoves();
        }

        public bool IsScrewSortCompleted(BaseScrew baseScrew)
        {
            NutsHolderScrewBehaviour currentSelectedScrewNutsHolder = baseScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();
            if (!currentSelectedScrewNutsHolder.CanAddNut)
            {
                int firstNutColorId = currentSelectedScrewNutsHolder.PeekNut().GetNutColorType();
                int colorCountOfNuts = gameplayStateData.levelNutsUniqueColorsCount[firstNutColorId];

                int currentColorCount = 0;
                for (int i = 0; i < currentSelectedScrewNutsHolder.CurrentNutCount; i++)
                {
                    int colorOfNut = currentSelectedScrewNutsHolder.PeekNut(i).GetNutColorType();
                    if (colorOfNut == firstNutColorId)
                        currentColorCount++;
                    else
                        break;
                }

                if (currentColorCount == colorCountOfNuts) // Screw Sort is Completed
                {
                    return true;
                }
            }

            return false;
        }

        private void OnNutTransferComplete(BaseScrew fromScrew, BaseScrew toScrew, int nutsTransferred)
        {
            gameplayStateData.OnGameplayMove(fromScrew, toScrew, nutsTransferred);
            CheckForSurpriseNutColorReveal(fromScrew);
            CheckForScrewSortCompletion(toScrew);
            gameplayStateData.CalculatePossibleNumberOfMoves();
        }
        #endregion

        #region ANALYTICS_EVENTS
        public void LogLevelStartEvent()
        {
            AnalyticsManager.Instance.LogLevelDataEvent(AnalyticsConstants.LevelData_StartTrigger);
            AnalyticsManager.Instance.LogProgressionEvent(GAProgressionStatus.Start);
        }

        public void Adjust_LogLevelEvent()
        {
            AdjustManager.Instance.Adjust_LevelStartEvent(LevelManager.Instance.CurrentLevelDataSO.level, LevelManager.Instance.CurrentLevelDataSO.levelType);
        }

        public void LogSpecialLevelRestartEvent()
        {
            AnalyticsManager.Instance.LogSpecialLevelDataEvent(AnalyticsConstants.LevelData_RestartTrigger);
        }

        public void LogSpecialLevelFinishEvent()
        {
            AnalyticsManager.Instance.LogSpecialLevelDataEvent(AnalyticsConstants.LevelData_EndTrigger);
        }

        public void LogCoinRewardFaucetEvent(string itemId, float amount)
        {
            AnalyticsManager.Instance.LogResourceEvent(GAResourceFlowType.Source, AnalyticsConstants.CoinCurrency, amount, AnalyticsConstants.ItemType_Reward, itemId);
        }

        public void LogCoinPurchaseFaucetEvent(string itemType, string itemId, float amount)
        {
            AnalyticsManager.Instance.LogResourceEvent(GAResourceFlowType.Source, AnalyticsConstants.CoinCurrency, amount, itemType, itemId);
        }

        public void LogCoinTradeSinkEvent(string itemId, float amount)
        {
            AnalyticsManager.Instance.LogResourceEvent(GAResourceFlowType.Sink, AnalyticsConstants.CoinCurrency, amount, AnalyticsConstants.ItemType_Trade, itemId);
        }

        public void LogLevelRestartEvent()
        {
            AnalyticsManager.Instance.LogLevelDataEvent(AnalyticsConstants.LevelData_RestartTrigger);
            AnalyticsManager.Instance.LogProgressionEvent(GAProgressionStatus.Fail);
        }

        public void LogLevelFinishEvent()
        {
            AnalyticsManager.Instance.LogLevelDataEvent(AnalyticsConstants.LevelData_EndTrigger);
            AnalyticsManager.Instance.LogProgressionEvent(GAProgressionStatus.Complete);
        }

        public void Adjust_LogLevelFinishEvent()
        {
            AdjustManager.Instance.Adjust_LevelCompleteEvent(PlayerPersistantData.GetMainPlayerProgressData().playerGameplayLevel, gameplayStateData.levelRunTime);
        }
        #endregion

        #region PRIVATE_METHODS
        private void CheckForSurpriseNutColorReveal(BaseScrew baseScrew)
        {
            NutsHolderScrewBehaviour currentSelectedScrewNutsHolder = baseScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();

            int myNutCheckColorId = -1;
            for (int i = 0; i < currentSelectedScrewNutsHolder.CurrentNutCount; i++)
            {
                BaseNut nextNut = currentSelectedScrewNutsHolder.PeekNut(i);

                if (nextNut is SurpriseColorNut surpriseNextNut && surpriseNextNut.SurpriseColorNutState == SurpriseColorNutState.COLOR_NOT_REVEALED)
                {
                    if (myNutCheckColorId == -1 || myNutCheckColorId == surpriseNextNut.GetRealNutColorType())
                    {
                        myNutCheckColorId = surpriseNextNut.GetRealNutColorType();
                        surpriseNextNut.transform.localScale = Vector3.one;
                        VFXManager.Instance.PlayRevealAnimationOnNut(surpriseNextNut);
                    }
                    else
                        break;
                }
                else
                    break;
            }
        }

        private void CheckForScrewSortCompletion(BaseScrew baseScrew)
        {
            bool isScrewSortCompleted = IsScrewSortCompleted(baseScrew);

            if (isScrewSortCompleted) // Screw Sort is Completed
            {
                NutsHolderScrewBehaviour currentSelectedScrewNutsHolder = baseScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();

                gameplayStateData.OnNutColorSortCompletion(currentSelectedScrewNutsHolder.PeekNut().GetNutColorType());
                baseScrew.SetScrewInteractableState(ScrewInteractibilityState.Locked);
                VFXManager.Instance.PlayScrewSortCompletion(baseScrew);
                CheckForAllScrewSortCompletion();
            }
        }

        private void CheckForAllScrewSortCompletion()
        {
            if (!gameplayStateData.levelNutsUniqueColorsSortCompletionState.ContainsValue(false)) // All Screw Sort is Completed
                RaiseOnGameplayLevelOver();
        }
        #endregion

        #region EVENT_HANDLERS
        public delegate void GameplayManagerVoidEvents();
        public static event GameplayManagerVoidEvents onGameplayLevelOver;
        public static void RaiseOnGameplayLevelOver()
        {
            onGameplayLevelOver?.Invoke();
        }

        public static event GameplayManagerVoidEvents onGameplayLevelStart;
        public static void RaiseOnGameplayLevelStart()
        {
            onGameplayLevelStart?.Invoke();
        }

        public static event GameplayManagerVoidEvents onGameplayLevelResume;
        public static void RaiseOnGameplayLevelResume()
        {
            onGameplayLevelResume?.Invoke();
        }

        public static event GameplayManagerVoidEvents onGameplayLevelReload;
        public static void RaiseOnGameplayLevelReload()
        {
            onGameplayLevelReload?.Invoke();
        }

        public static event GameplayManagerVoidEvents onGameplayLevelLoadComplete;
        public static void RaiseOnGameplayLevelLoadComplete()
        {
            onGameplayLevelLoadComplete?.Invoke();
        }

        public static event GameplayManagerVoidEvents onLevelRecycle;
        public static void RaiseOnLevelRecycle()
        {
            onLevelRecycle?.Invoke();
        }
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion

        #region UNITY_EDITOR_FUNCTIONS
        [Button]
        public void OnEditor_FinishLevel()
        {
            RaiseOnGameplayLevelOver();
        }
        #endregion
    }

    public class GameplayStateData
    {
        public GameplayStateType gameplayStateType;
        public int currentLevelNumber;
        public int totalNumberOfScrews => LevelManager.Instance.LevelScrews.Count;
        public int totalNumberOfNuts => LevelManager.Instance.LevelNuts.Count;

        public Dictionary<int, int> levelNutsUniqueColorsCount = new Dictionary<int, int>();
        public Dictionary<int, bool> levelNutsUniqueColorsSortCompletionState = new Dictionary<int, bool>();

        public List<GameplayMoveInfo> gameplayMoveInfos = new List<GameplayMoveInfo>();

        public int TotalPossibleMovesCount => possibleMovesInfo.Count;

        public List<GameplayMoveInfo> possibleMovesInfo = new List<GameplayMoveInfo>();

        public int levelRunTime;

        public GameplayStateData()
        {
        }

        public int GetTotalNutCountOfColor(int colorId)
        {
            if (levelNutsUniqueColorsCount.ContainsKey(colorId))
                return levelNutsUniqueColorsCount[colorId];
            return 0;
        }

        public void ResetGameplayStateData()
        {
            levelNutsUniqueColorsCount = new Dictionary<int, int>();
            gameplayStateType = GameplayStateType.NONE;
            levelNutsUniqueColorsCount.Clear();
            levelNutsUniqueColorsSortCompletionState.Clear();
            gameplayMoveInfos.Clear();
            levelRunTime = 0;
        }

        public void PopulateGameplayStateData()
        {
            currentLevelNumber = LevelManager.Instance.CurrentLevelDataSO.level;
            gameplayStateType = GameplayStateType.NONE;
            levelNutsUniqueColorsCount.Clear();
            levelNutsUniqueColorsSortCompletionState.Clear();
            levelRunTime = 0;

            LevelDataSO currentLevel = LevelManager.Instance.CurrentLevelDataSO;

            foreach (var screwData in currentLevel.screwNutsLevelDataInfos)
            {
                foreach (var nutsData in screwData.levelNutDataInfos)
                {
                    if (levelNutsUniqueColorsCount.ContainsKey(nutsData.nutColorTypeId))
                        levelNutsUniqueColorsCount[nutsData.nutColorTypeId]++;
                    else
                    {
                        levelNutsUniqueColorsSortCompletionState.Add(nutsData.nutColorTypeId, false);
                        levelNutsUniqueColorsCount.Add(nutsData.nutColorTypeId, 1);
                    }
                }
            }
        }

        public void CalculatePossibleNumberOfMoves()
        {
            possibleMovesInfo.Clear();

            foreach (var fromScrew in LevelManager.Instance.LevelScrews)
            {
                // Skip if screw is locked or empty
                if (fromScrew.ScrewInteractibilityState == ScrewInteractibilityState.Locked || !fromScrew.TryGetScrewBehaviour(out NutsHolderScrewBehaviour fromNutsHolder) || fromNutsHolder.IsEmpty)
                    continue;

                // Get the color of top nut in the source screw
                int sourceNutColor = fromNutsHolder.PeekNut().GetOriginalNutColorType();

                // Check all other screws as potential destinations
                foreach (var toScrew in LevelManager.Instance.LevelScrews)
                {
                    // Skip if same screw or destination is locked
                    if (fromScrew == toScrew || toScrew.ScrewInteractibilityState == ScrewInteractibilityState.Locked)
                        continue;

                    if (toScrew.TryGetScrewBehaviour(out NutsHolderScrewBehaviour toNutsHolder))
                    {
                        bool isValidMove = false;
                        int transferrableNuts = 0;

                        // If destination screw is empty, it's a valid move
                        if (toNutsHolder.IsEmpty && toNutsHolder.CanAddNut)
                        {
                            isValidMove = true;
                            // Count how many nuts of same color we can transfer
                            transferrableNuts = CountTransferrableNuts(fromNutsHolder, sourceNutColor, toNutsHolder.MaxNutCapacity);
                        }
                        // If destination has same color on top and space available
                        else if (!toNutsHolder.IsEmpty && toNutsHolder.CanAddNut && toNutsHolder.PeekNut().GetOriginalNutColorType() == sourceNutColor)
                        {
                            isValidMove = true;
                            // Count how many nuts we can transfer considering destination's remaining capacity
                            int remainingCapacity = toNutsHolder.MaxNutCapacity - toNutsHolder.CurrentNutCount;
                            transferrableNuts = CountTransferrableNuts(fromNutsHolder, sourceNutColor, remainingCapacity);
                        }

                        if (isValidMove && transferrableNuts > 0)
                            possibleMovesInfo.Add(new GameplayMoveInfo(fromScrew.GridCellId, toScrew.GridCellId, transferrableNuts));
                    }
                }
            }

            AdjustManager.Instance.Adjust_ChokePointEvent(TotalPossibleMovesCount);
        }

        private int CountTransferrableNuts(NutsHolderScrewBehaviour fromHolder, int colorToMatch, int maxTransferCount)
        {
            int count = 0;
            int nutsToCheck = Mathf.Min(fromHolder.CurrentNutCount, maxTransferCount);

            for (int i = 0; i < nutsToCheck; i++)
            {
                if (fromHolder.PeekNut(i).GetOriginalNutColorType() == colorToMatch)
                    count++;
                else
                    break;
            }

            return count;
        }

        public void OnGamePlayStart()
        {
            gameplayStateType = GameplayStateType.PLAYING_LEVEL;
            CalculatePossibleNumberOfMoves();
        }

        public void OnNutColorSortCompletion(int nutColorId)
        {
            levelNutsUniqueColorsSortCompletionState[nutColorId] = true;
        }

        public void OnGameplayMove(BaseScrew fromScrew, BaseScrew toScrew, int transferredNumberOfNuts)
        {
            var gameplayMoveInfo = new GameplayMoveInfo(fromScrew.GridCellId, toScrew.GridCellId, transferredNumberOfNuts);
            gameplayMoveInfos.Add(gameplayMoveInfo);
            GameplayLevelProgressManager.Instance.OnPlayerMoveConfirmed(gameplayMoveInfo);
        }

        public GameplayMoveInfo GetLastGameplayMove()
        {
            return gameplayMoveInfos.PopAt(gameplayMoveInfos.Count - 1);
        }

        public GameplayMoveInfo PeekLastGameplayMove()
        {
            if (gameplayMoveInfos.Count <= 0)
                return null;
            return gameplayMoveInfos[gameplayMoveInfos.Count - 1];
        }
    }

    public class GameplayMoveInfo
    {
        public GridCellId moveFromScrew;
        public GridCellId moveToScrew;
        public int transferredNumberOfNuts;

        public GameplayMoveInfo() { }
        public GameplayMoveInfo(GridCellId moveFromScrew, GridCellId moveToScrew, int transferredNumberOfNuts)
        {
            this.moveFromScrew = moveFromScrew;
            this.moveToScrew = moveToScrew;
            this.transferredNumberOfNuts = transferredNumberOfNuts;
        }

        public PlayerLevelProgressMoveDataInfo GetPlayerLevelProgressMoveInfo()
        {
            return new PlayerLevelProgressMoveDataInfo(moveFromScrew, moveToScrew, transferredNumberOfNuts);
        }
    }

    public enum GameplayStateType
    {
        NONE = 0,
        PLAYING_LEVEL = 1,
        LEVEL_OVER = 2
    }

    public enum BoosterType
    {
        UNDO,
        EXTRA_BOLT
    }
}