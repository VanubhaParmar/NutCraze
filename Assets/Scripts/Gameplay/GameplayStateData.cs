using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class GameplayStateData
    {
        public GameplayStateType gameplayStateType;
        public Dictionary<int, int> levelNutsUniqueColorsCount = new Dictionary<int, int>();
        public Dictionary<int, bool> levelNutsUniqueColorsSortCompletionState = new Dictionary<int, bool>();
        public List<MoveData> possibleMoves = new List<MoveData>();
        public int levelRunTime;

        public int TotalPossibleMovesCount => possibleMoves.Count;

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
            levelRunTime = 0;
        }

        public void PopulateGameplayStateData()
        {
            LevelDataSO currentLevel = LevelManager.Instance.CurrentLevelData;

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
                        transferrableNuts = CountTransferrableNuts(fromScrew, sourceNutColor, toScrew.Capacity);
                    }
                    else if (!toScrew.IsEmpty && toScrew.CanAddNut && toScrew.PeekNut().GetOriginalNutColorType() == sourceNutColor)
                    {
                        isValidMove = true;
                        int remainingCapacity = toScrew.Capacity - toScrew.CurrentNutCount;
                        transferrableNuts = CountTransferrableNuts(fromScrew, sourceNutColor, remainingCapacity);
                    }
                    if (isValidMove && transferrableNuts > 0)
                        possibleMovesInfo.Add(new GameplayMoveInfo(fromScrew.CellId, toScrew.CellId, transferrableNuts));
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
    }
    public enum GameplayStateType
    {
        NONE = 0,
        PLAYING_LEVEL = 1,
        LEVEL_OVER = 2
    }
}
