using DG.Tweening;
using Sirenix.OdinInspector;
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
        }

        public void StartGame()
        {
            gameplayStateData.ResetGameplayStateData();
            gameplayStateData.PopulateGameplayStateData();

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

            var pData = PlayerPersistantData.GetMainPlayerProgressData();
            pData.playerGameplayLevel++;

            if (!LevelManager.Instance.DoesLevelExist(pData.playerGameplayLevel)) // Reset level if it does not exist... TODO : Remove from production code Plssssss
                pData.playerGameplayLevel = 1;

            PlayerPersistantData.SetMainPlayerProgressData(pData);

            GetGameplayAnimator<MainGameplayAnimator>().PlayLevelCompleteAnimation(OnLoadCurrentReachedLevel);
        }

        public void OnLoadCurrentReachedLevel()
        {
            LevelManager.Instance.LoadCurrentLevel();
            MainSceneUIManager.Instance.GetView<GameplayView>().Show();

            GetGameplayAnimator<MainGameplayAnimator>().PlayLevelLoadAnimation(StartGame);
            //StartGame();
        }

        public void OnReloadCurrentLevel()
        {
            if (gameplayStateData.gameplayStateType == GameplayStateType.PLAYING_LEVEL)
            {
                gameplayStateData.gameplayStateType = GameplayStateType.NONE;
                OnLoadCurrentReachedLevel();
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

            RetransferNutFromCurrentSelectedScrewTo(LevelManager.Instance.GetScrewOfGridCell(lastMoveState.moveFromScrew), lastMoveState.transferredNumberOfNuts);
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

                boosterActivatedScrew.ExtendScrew();
            }
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

            CheckForSurpriseNutColorReveal(currentSelectedScrew);
            CheckForScrewSortCompletion(baseScrew);
            gameplayStateData.OnGameplayMove(currentSelectedScrew, baseScrew, extraNutIndex + 1);

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
                        surpriseNextNut.OnRevealColorOfNut();
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

        private bool IsScrewSortCompleted(BaseScrew baseScrew)
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
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
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

        public GameplayStateData()
        {
        }

        public void ResetGameplayStateData()
        {
            levelNutsUniqueColorsCount = new Dictionary<int, int>();
            gameplayStateType = GameplayStateType.NONE;
            levelNutsUniqueColorsCount.Clear();
            levelNutsUniqueColorsSortCompletionState.Clear();
            gameplayMoveInfos.Clear();
        }

        public void PopulateGameplayStateData()
        {
            currentLevelNumber = LevelManager.Instance.CurrentLevelDataSO.level;
            gameplayStateType = GameplayStateType.PLAYING_LEVEL;
            levelNutsUniqueColorsCount.Clear();
            levelNutsUniqueColorsSortCompletionState.Clear();

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

        public void OnNutColorSortCompletion(int nutColorId)
        {
            levelNutsUniqueColorsSortCompletionState[nutColorId] = true;
        }

        public void OnGameplayMove(BaseScrew fromScrew, BaseScrew toScrew, int transferredNumberOfNuts)
        {
            gameplayMoveInfos.Add(new GameplayMoveInfo(fromScrew.GridCellId, toScrew.GridCellId, transferredNumberOfNuts));
        }

        public GameplayMoveInfo GetLastGameplayMove()
        {
            return gameplayMoveInfos.PopAt(gameplayMoveInfos.Count - 1);
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
    }

    public enum GameplayStateType
    {
        NONE = 0,
        PLAYING_LEVEL = 1,
        LEVEL_OVER = 2
    }
}