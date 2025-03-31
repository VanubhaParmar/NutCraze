using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
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
            //currentLevelNumber = LevelManager.Instance.CurrentLevelData.level;
            //gameplayStateType = GameplayStateType.NONE;
            //levelNutsUniqueColorsCount.Clear();
            //levelNutsUniqueColorsSortCompletionState.Clear();
            //levelRunTime = 0;

            //LevelDataSO currentLevel = LevelManager.Instance.CurrentLevelData;

            //foreach (var screwData in currentLevel.screwNutsLevelDataInfos)
            //{
            //    foreach (var nutsData in screwData.levelNutDataInfos)
            //    {
            //        if (levelNutsUniqueColorsCount.ContainsKey(nutsData.nutColorTypeId))
            //            levelNutsUniqueColorsCount[nutsData.nutColorTypeId]++;
            //        else
            //        {
            //            levelNutsUniqueColorsSortCompletionState.Add(nutsData.nutColorTypeId, false);
            //            levelNutsUniqueColorsCount.Add(nutsData.nutColorTypeId, 1);
            //        }
            //    }
            //}
        }

        public void CalculatePossibleNumberOfMoves()
        {
            possibleMovesInfo.Clear();

            foreach (var fromScrew in LevelManager.Instance.LevelScrews)
            {
                if (fromScrew.ScrewState == ScrewState.Locked || fromScrew.IsEmpty)
                    continue;

                int sourceNutColor = fromScrew.PeekNut().GetOriginalNutColorType();

                foreach (var toScrew in LevelManager.Instance.LevelScrews)
                {
                    if (fromScrew == toScrew || toScrew.ScrewState == ScrewState.Locked)
                        continue;

                    bool isValidMove = false;
                    int transferrableNuts = 0;

                    if (toScrew.IsEmpty && toScrew.CanAddNut)
                    {
                        isValidMove = true;
                        transferrableNuts = CountTransferrableNuts(fromScrew, sourceNutColor, toScrew.MaxNutCapacity);
                    }
                    else if (!toScrew.IsEmpty && toScrew.CanAddNut && toScrew.PeekNut().GetOriginalNutColorType() == sourceNutColor)
                    {
                        isValidMove = true;
                        int remainingCapacity = toScrew.MaxNutCapacity - toScrew.CurrentNutCount;
                        transferrableNuts = CountTransferrableNuts(fromScrew, sourceNutColor, remainingCapacity);
                    }
                    if (isValidMove && transferrableNuts > 0)
                        possibleMovesInfo.Add(new GameplayMoveInfo(fromScrew.GridCellId, toScrew.GridCellId, transferrableNuts));
                }
            }

            AdjustManager.Instance.Adjust_ChokePointEvent(TotalPossibleMovesCount);
        }

        private int CountTransferrableNuts(BaseScrew fromScrew, int colorToMatch, int maxTransferCount)
        {
            int count = 0;
            int nutsToCheck = Mathf.Min(fromScrew.CurrentNutCount, maxTransferCount);

            for (int i = 0; i < nutsToCheck; i++)
            {
                if (fromScrew.PeekNut(i).GetOriginalNutColorType() == colorToMatch)
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
}
