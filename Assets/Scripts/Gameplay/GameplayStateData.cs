using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class GameplayStateData
    {
        public GameplayStateType gameplayStateType;
        public List<MoveData> possibleMoves = new List<MoveData>();
        public Dictionary<int, int> levelNutsUniqueColorsCount = new Dictionary<int, int>();
        public Dictionary<int, bool> levelNutsUniqueColorsSortCompletionState = new Dictionary<int, bool>();
        public int TotalPossibleMoves => possibleMoves.Count;

        public int GetTotalNutCountOfColor(int colorId)
        {
            if (levelNutsUniqueColorsCount.ContainsKey(colorId))
                return levelNutsUniqueColorsCount[colorId];
            return 0;
        }

        private void ResetGameplayStateData()
        {
            gameplayStateType = GameplayStateType.NONE;
            levelNutsUniqueColorsCount.Clear();
            levelNutsUniqueColorsSortCompletionState.Clear();
        }

        public void CalculateGameplayState()
        {
            ResetGameplayStateData();
            List<BaseScrew> screws = ScrewManager.Instance.Screws;

            foreach (var item in screws)
            {
                List<BaseNut> nuts = item.Nuts;
                for (int i = 0; i < nuts.Count; i++)
                {
                    BaseNut baseNut = nuts[i];
                    int nutId = baseNut.GetRealNutColorType();
                    if (levelNutsUniqueColorsCount.ContainsKey(nutId))
                        levelNutsUniqueColorsCount[nutId]++;
                    else
                    {
                        levelNutsUniqueColorsSortCompletionState.Add(nutId, false);
                        levelNutsUniqueColorsCount.Add(nutId, 1);
                    }
                }
            }
        }

        public void CalculatePossibleNumberOfMoves()
        {
            possibleMoves.Clear();
            List<BaseScrew> screws = ScrewManager.Instance.Screws;
            foreach (var fromScrew in screws)
            {
                if (fromScrew.ScrewState == ScrewState.Locked || fromScrew.IsEmpty)
                    continue;

                int sourceNutColor = fromScrew.PeekNut().GetRealNutColorType();

                foreach (var toScrew in screws)
                {
                    if (fromScrew == toScrew || toScrew.ScrewState == ScrewState.Locked)
                        continue;

                    bool isValidMove = false;
                    int transferrableNuts = 0;

                    if (toScrew.IsEmpty && toScrew.CanAddNut)
                    {
                        isValidMove = true;
                        transferrableNuts = CountTransferrableNuts(fromScrew, sourceNutColor, toScrew.CurrentCapacity);
                    }
                    else if (!toScrew.IsEmpty && toScrew.CanAddNut && toScrew.PeekNut().GetRealNutColorType() == sourceNutColor)
                    {
                        isValidMove = true;
                        int remainingCapacity = toScrew.CurrentCapacity - toScrew.CurrentNutCount;
                        transferrableNuts = CountTransferrableNuts(fromScrew, sourceNutColor, remainingCapacity);
                    }
                    if (isValidMove && transferrableNuts > 0)
                        possibleMoves.Add(new MoveData(fromScrew.GridCellId, toScrew.GridCellId, transferrableNuts));
                }
            }

            AdjustManager.Instance?.Adjust_ChokePointEvent(TotalPossibleMoves);
        }

        private int CountTransferrableNuts(BaseScrew fromScrew, int colorToMatch, int maxTransferCount)
        {
            int count = 0;
            int nutsToCheck = Mathf.Min(fromScrew.CurrentNutCount, maxTransferCount);

            for (int i = 0; i < nutsToCheck; i++)
            {
                if (fromScrew.PeekNut(i).GetRealNutColorType() == colorToMatch)
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
            LevelFailManager.Instance.CheckForLevelFail();
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
