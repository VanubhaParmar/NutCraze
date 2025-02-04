using DG.Tweening;
using GameAnalyticsSDK;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class GameplayManager : SerializedManager<GameplayManager>
    {
        #region PUBLIC_VARIABLES
        public GameplayStateData GameplayStateData => gameplayStateData;
        public BaseScrew CurrentSelectedScrew => currentSelectedScrew;
        #endregion

        #region PRIVATE_VARIABLES
        [ShowInInspector, ReadOnly] private BaseScrew currentSelectedScrew;
        [ShowInInspector, ReadOnly] private GameplayStateData gameplayStateData;

        [SerializeField] private List<BaseGameplayAnimator> gameplayAnimators = new List<BaseGameplayAnimator>();

        private const int Store_Gameplay_Data_Every_X_Seconds = 5;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        public override void Awake()
        {
            base.Awake();
            InitGameplayManager();
            OnLoadingDone();
        }
        #endregion

        #region PUBLIC_METHODS
        public void InitGameplayManager()
        {
            gameplayAnimators.ForEach(x => x.InitGameplayAnimator());
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
            currentSelectedScrew = null;
            gameplayStateData.OnGamePlayStart();
            RaiseOnGameplayLevelStart();
            TutorialManager.Instance.CheckForTutorialsToStart();
        }

        public void ResumeGame()
        {
            currentSelectedScrew = null;
            gameplayStateData.OnGamePlayStart();
            RaiseOnGameplayLevelResume();
            TutorialManager.Instance.CheckForTutorialsToStart();
        }

        public void OnScrewClicked(BaseScrew baseScrew)
        {
            if (currentSelectedScrew == null)
                OnScrewSelection(baseScrew);
            else // check for other conditions and game rules
                CheckForNutTransfer(baseScrew);
        }

        public T GetGameplayAnimator<T>() where T : BaseGameplayAnimator
        {
            return gameplayAnimators.Find(x => x is T) as T;
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

            GetGameplayAnimator<MainGameplayAnimator>().PlayLevelCompleteAnimation(() => ShowGameWinView());
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
                GetGameplayAnimator<MainGameplayAnimator>().PlayLevelLoadAnimation(StartGame);
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
                GetGameplayAnimator<MainGameplayAnimator>().PlayLevelLoadAnimation(loadAnimationAction);
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

        public bool CanUseUndoBooster()
        {
            return DataManager.Instance.CanUseUndoBooster() && gameplayStateData.gameplayStateType == GameplayStateType.PLAYING_LEVEL && gameplayStateData.gameplayMoveInfos.Count > 0;
        }

        public void UseUndoBooster()
        {
            var playerData = PlayerPersistantData.GetMainPlayerProgressData();
            playerData.undoBoostersCount = Mathf.Max(playerData.undoBoostersCount - 1, 0);
            PlayerPersistantData.SetMainPlayerProgressData(playerData);

            var lastMoveState = gameplayStateData.GetLastGameplayMove();

            if (currentSelectedScrew != null)
                OnScrewSelectionRemove();

            currentSelectedScrew = LevelManager.Instance.GetScrewOfGridCell(lastMoveState.moveToScrew);

            bool isSortedScrew = IsScrewSortCompleted(currentSelectedScrew);
            if (isSortedScrew) // Reset all data when undoing sorted screw
            {
                DOTween.Kill(lastMoveState.moveToScrew); // kill all tweens on target screw and reset cap
                currentSelectedScrew.ScrewTopRenderer.gameObject.SetActive(false);
                currentSelectedScrew.SetScrewInteractableState(ScrewInteractibilityState.Interactable);
                currentSelectedScrew.StopStackFullIdlePS();

                NutsHolderScrewBehaviour currentSelectedScrewNutsHolder = currentSelectedScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();
                int firstNutColorId = currentSelectedScrewNutsHolder.PeekNut().GetNutColorType();
                gameplayStateData.levelNutsUniqueColorsSortCompletionState[firstNutColorId] = false;
            }

            GameplayLevelProgressManager.Instance.OnUndoBoosterUsed();
            RetransferNutFromCurrentSelectedScrewTo(LevelManager.Instance.GetScrewOfGridCell(lastMoveState.moveFromScrew), lastMoveState.transferredNumberOfNuts);

            gameplayStateData.CalculatePossibleNumberOfMoves();

            RaiseOnUndoBoosterUsed();
        }

        public bool CanUseExtraScrewBooster()
        {
            var boosterActivatedScrew = LevelManager.Instance.LevelScrews.Find(x => x is BoosterActivatedScrew) as BoosterActivatedScrew;

            if (boosterActivatedScrew == null)
                return false;

            return DataManager.Instance.CanUseExtraScrewBooster() && gameplayStateData.gameplayStateType == GameplayStateType.PLAYING_LEVEL && boosterActivatedScrew.CanExtendScrew();
        }

        public void UseExtraScrewBooster()
        {
            var boosterActivatedScrew = LevelManager.Instance.LevelScrews.Find(x => x is BoosterActivatedScrew) as BoosterActivatedScrew;

            if (boosterActivatedScrew != null && boosterActivatedScrew.CanExtendScrew())
            {
                var playerData = PlayerPersistantData.GetMainPlayerProgressData();
                playerData.extraScrewBoostersCount = Mathf.Max(playerData.extraScrewBoostersCount - 1, 0);
                PlayerPersistantData.SetMainPlayerProgressData(playerData);
                GameplayLevelProgressManager.Instance.OnBoosterScrewStateUpgrade();

                boosterActivatedScrew.ExtendScrew();

                gameplayStateData.CalculatePossibleNumberOfMoves();
                RaiseOnExtraScrewBoosterUsed();
            }
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

        public List<string> GetListOfRunningEvents()
        {
            List<string> runningEvents = new List<string>();
            if (LeaderboardManager.Instance != null && LeaderboardManager.Instance.CanOpenLeaderboardUI())
                runningEvents.Add(AdjustConstant.Leader_Board_Event_Name);

            return runningEvents;
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
        private void OnScrewSelection(BaseScrew currentScrew)
        {
            if (currentSelectedScrew == null && currentScrew.TryGetScrewBehaviour(out NutsHolderScrewBehaviour nutsHolderScrewBehaviour) && !nutsHolderScrewBehaviour.IsEmpty)
            {
                currentSelectedScrew = currentScrew;
                OnScrewSelectionSuccess();
            }
        }

        private void CheckForNutTransfer(BaseScrew baseScrew)
        {
            bool nutTransferResult = false;

            if (currentSelectedScrew != baseScrew)
            {
                if (baseScrew.TryGetScrewBehaviour(out NutsHolderScrewBehaviour nutsHolderScrewBehaviour) && nutsHolderScrewBehaviour.CanAddNut) // check if we can add nut to target screw
                {
                    BaseNut currentSelectedScrewNut = currentSelectedScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>().PeekNut();
                    BaseNut lastNutOnScrew = nutsHolderScrewBehaviour.PeekNut();
                    if (lastNutOnScrew == null || lastNutOnScrew.GetNutColorType() == currentSelectedScrewNut.GetNutColorType())
                    {
                        nutTransferResult = true;
                    }
                }
            }

            if (!nutTransferResult)
                OnScrewSelectionRemove();
            else
                TransferNutFromCurrentSelectedScrewTo(baseScrew);
        }

        private void RetransferNutFromCurrentSelectedScrewTo(BaseScrew baseScrew, int nutsCountToTransfer)
        {
            NutsHolderScrewBehaviour currentSelectedScrewNutsHolder = currentSelectedScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();
            NutsHolderScrewBehaviour targetScrewNutsHolder = baseScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();

            BaseNut lastNut = currentSelectedScrewNutsHolder.PopNut();
            targetScrewNutsHolder.AddNut(lastNut, false);

            MainGameplayAnimator nutSelectionGameplayAnimator = GetGameplayAnimator<MainGameplayAnimator>(); // Transfer target nut first
            nutSelectionGameplayAnimator.TransferThisNutFromStartScrewTopToEndScrew(lastNut, currentSelectedScrew, baseScrew);

            int extraNutIndex = 0;
            nutsCountToTransfer--;

            while (nutsCountToTransfer > 0)
            {
                //int extraNutIndex = currentSelectedScrewNutsHolder.CurrentNutCount - 1;
                BaseNut extraNut = currentSelectedScrewNutsHolder.PopNut();
                targetScrewNutsHolder.AddNut(extraNut, false);

                nutSelectionGameplayAnimator.TransferThisNutFromStartScrewToEndScrew(extraNut, extraNutIndex, currentSelectedScrew, baseScrew); // Transfer all other nuts
                extraNutIndex++;
                nutsCountToTransfer--;
            }

            currentSelectedScrew = null;
        }

        private void TransferNutFromCurrentSelectedScrewTo(BaseScrew baseScrew)
        {
            NutsHolderScrewBehaviour currentSelectedScrewNutsHolder = currentSelectedScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();
            NutsHolderScrewBehaviour targetScrewNutsHolder = baseScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();

            BaseNut lastNut = currentSelectedScrewNutsHolder.PopNut();
            targetScrewNutsHolder.AddNut(lastNut, false);

            MainGameplayAnimator nutSelectionGameplayAnimator = GetGameplayAnimator<MainGameplayAnimator>(); // Transfer target nut first
            nutSelectionGameplayAnimator.TransferThisNutFromStartScrewTopToEndScrew(lastNut, currentSelectedScrew, baseScrew);

            int extraNutIndex = 0;
            while (targetScrewNutsHolder.CanAddNut && currentSelectedScrewNutsHolder.CurrentNutCount > 0 && currentSelectedScrewNutsHolder.PeekNut().GetNutColorType() == lastNut.GetNutColorType())
            {
                //int extraNutIndex = currentSelectedScrewNutsHolder.CurrentNutCount - 1;
                BaseNut extraNut = currentSelectedScrewNutsHolder.PopNut();
                targetScrewNutsHolder.AddNut(extraNut, false);

                nutSelectionGameplayAnimator.TransferThisNutFromStartScrewToEndScrew(extraNut, extraNutIndex, currentSelectedScrew, baseScrew); // Transfer all other nuts
                extraNutIndex++;
            }

            gameplayStateData.OnGameplayMove(currentSelectedScrew, baseScrew, extraNutIndex + 1);

            CheckForSurpriseNutColorReveal(currentSelectedScrew);
            CheckForScrewSortCompletion(baseScrew);

            gameplayStateData.CalculatePossibleNumberOfMoves();
            currentSelectedScrew = null;
        }

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
                        //SurpriseNutAnimation nutAnimation = ObjectPool.Instance.Spawn(PrefabsHolder.Instance.NutRevealAnimation, surpriseNextNut.transform.parent);
                        //nutAnimation.transform.position = surpriseNextNut.transform.position;
                        //nutAnimation.transform.localScale = Vector3.one;
                        //nutAnimation.transform.localEulerAngles = new Vector3(0, 30, 0);
                        surpriseNextNut.transform.localScale = Vector3.one;

                        MainGameplayAnimator nutSelectionGameplayAnimator = GetGameplayAnimator<MainGameplayAnimator>(); // Transfer target nut first
                        nutSelectionGameplayAnimator.PlayRevealAnimationOnNut(surpriseNextNut);
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

                GetGameplayAnimator<MainGameplayAnimator>().OnPlayScrewSortCompletion(baseScrew);
                CheckForAllScrewSortCompletion();
            }
        }

        private void CheckForAllScrewSortCompletion()
        {
            if (!gameplayStateData.levelNutsUniqueColorsSortCompletionState.ContainsValue(false)) // All Screw Sort is Completed
                RaiseOnGameplayLevelOver();
        }

        private void OnScrewSelectionRemove()
        {
            OnScrewDeselctionSuccess();
            currentSelectedScrew = null;
        }

        private void OnScrewSelectionSuccess()
        {
            if (currentSelectedScrew.TryGetScrewBehaviour(out NutsHolderScrewBehaviour nutsHolderScrewBehaviour) && !nutsHolderScrewBehaviour.IsEmpty)
            {
                MainGameplayAnimator nutSelectionGameplayAnimator = GetGameplayAnimator<MainGameplayAnimator>();
                if (nutSelectionGameplayAnimator != null)
                    nutSelectionGameplayAnimator.LiftTheFirstSelectionNut(currentSelectedScrew);
            }
        }

        private void OnScrewDeselctionSuccess()
        {
            if (currentSelectedScrew.TryGetScrewBehaviour(out NutsHolderScrewBehaviour nutsHolderScrewBehaviour) && !nutsHolderScrewBehaviour.IsEmpty)
            {
                MainGameplayAnimator nutSelectionGameplayAnimator = GetGameplayAnimator<MainGameplayAnimator>();
                if (nutSelectionGameplayAnimator != null)
                    nutSelectionGameplayAnimator.ResetTheFirstSelectionNut(currentSelectedScrew);
            }
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

        public static event GameplayManagerVoidEvents onUndoBoosterUsed;
        public static void RaiseOnUndoBoosterUsed()
        {
            onUndoBoosterUsed?.Invoke();
        }

        public static event GameplayManagerVoidEvents onExtraScrewBoosterUsed;
        public static void RaiseOnExtraScrewBoosterUsed()
        {
            onExtraScrewBoosterUsed?.Invoke();
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