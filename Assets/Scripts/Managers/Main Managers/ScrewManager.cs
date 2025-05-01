using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tag.NutSort
{
    public class ScrewManager : SerializedManager<ScrewManager>
    {
        #region PRIVATE_VARIABLES
        [SerializeField] private Transform animationParent;
        [SerializeField] private Transform levelScrewsParent;
        [SerializeField] private Transform levelNutsParent;

        [ShowInInspector, ReadOnly] private List<BaseScrew> screws = new List<BaseScrew>();
        private const int MAX_SCREWS_IN_ROW = 8;
        private const int MAX_SCREWS_IN_COLUMN = 8;
        #endregion

        #region PUBLIC_VARIABLES
        #endregion

        #region PROPERTIES
        public List<BaseScrew> Screws => screws;
        public Transform AnimationParent => animationParent;
        #endregion

        #region UNITY_CALLBACKS
        public override void Awake()
        {
            base.Awake();
            OnLoadingDone();
        }
        #endregion

        #region PUBLIC_METHODS
        public void InitScrews(LevelSaveData levelData)
        {
            Dictionary<string, ScrewConfig> screwDatas = levelData.screws;
            ScrewArrangementConfig arrangementConfig = levelData.arrangementConfig;
            foreach (var screwData in screwDatas)
            {
                GridCellId gridCellId = GridCellId.Parse(screwData.Key);
                ScrewConfig screwConfig = screwData.Value;
                BaseScrew myScrew = ObjectPool.Instance.Spawn(ResourceManager.Instance.GetScrew(screwConfig.screwType), levelScrewsParent);
                myScrew.transform.position = arrangementConfig.GetCellPosition(gridCellId);
                myScrew.gameObject.SetActive(true);
                myScrew.InitScrew(gridCellId, screwConfig);
                this.screws.Add(myScrew);
            }
        }

        public void CheckForImmediateScrewSortCompletion()
        {
            foreach (var screw in screws)
            {
                if (screw.IsSorted())
                {
                    screw.OnScrewSortCompleteImmediate();
                    GameplayManager.Instance.GameplayStateData.OnNutColorSortCompletion(screw.PeekNut().GetNutColorType());
                }
            }
        }
        public void RecycleAllElements()
        {
            screws.ForEach(x => x?.Recycle());
            screws.Clear();
        }

        public bool TryGetBoosterScrew(out BoosterActivatedScrew boosterActivatedScrew)
        {
            boosterActivatedScrew = screws.Find(x => x is BoosterActivatedScrew) as BoosterActivatedScrew;
            return boosterActivatedScrew != null;
        }

        public BaseScrew GetScrew(GridCellId gridCellId)
        {
            return screws.Find(x => x.GridCellId == gridCellId);
        }

        public List<List<BaseScrew>> GetAllRowCells()
        {
            ScrewArrangementConfig arrangementConfig = LevelProgressManager.Instance.ArrangementConfig;
            List<List<BaseScrew>> allRowCells = new List<List<BaseScrew>>();
            for (int i = 0; i < arrangementConfig.gridSize.rowNumber; i++)
            {
                List<BaseScrew> rowCells = new List<BaseScrew>();
                for (int j = 0; j < arrangementConfig.gridSize.colNumber; j++)
                {
                    GridCellId gridCellId = new GridCellId(i, j);
                    BaseScrew screw = GetScrew(gridCellId);
                    if (screw != null)
                    {
                        rowCells.Add(screw);
                    }
                }
                allRowCells.Add(rowCells);
            }
            allRowCells.RemoveAll(x => x.Count == 0);
            return allRowCells;
        }

        public void AddSimpleScrew(int capacity)
        {
            AddScrew(ScrewTypeIdConstant.Simple, capacity);
        }
     
        public int GetMaxCapacityFromPeerScrew()
        {
            int maxCapacity = 0;
            for (int i = 0; i < screws.Count; i++)
            {
                if (screws[i].CurrentCapacity > maxCapacity)
                    maxCapacity = screws[i].CurrentCapacity;
            }
            return Mathf.Clamp(maxCapacity, 1, Constant.MAX_NEW_SCREW_CAPACITY);
        }
        #endregion

        #region PRIVATE_METHODS
        private void AddScrew(int screwType, int capacity = 4)
        {
            if (TryToFindMinScrewRaw(out List<BaseScrew> minScrewRaw))
            {
                LevelProgressManager.Instance.ClearAllScrewData();
                int rowIndex = minScrewRaw.FirstOrDefault().GridCellId.rowNumber;
                ScrewArrangementConfig arrangementConfig = LevelProgressManager.Instance.ArrangementConfig;
                List<GridCellId> gridCellIds = arrangementConfig.GetRowIds(rowIndex);
                BaseScrew baseScrew = SpawnNewScrew(screwType, capacity);
                minScrewRaw.Add(baseScrew);
                CenterScrewsWithinCells(gridCellIds, minScrewRaw);
                ResetAllScrewsPosition();
                CameraSizeHandler.Instance.InitializeSize();
                SaveAllScrewData();
            }
            else if (TryToExpandColumns())
            {
                AddScrew(screwType, capacity);
            }
            else if (TryToExpandRow(out int newRow))
            {
                LevelProgressManager.Instance.ClearAllScrewData();
                List<BaseScrew> newRowToAdd = new List<BaseScrew>();
                BaseScrew baseScrew = SpawnNewScrew(screwType, capacity);
                newRowToAdd.Add(baseScrew);
                ScrewArrangementConfig arrangementConfig = LevelProgressManager.Instance.ArrangementConfig;
                List<GridCellId> gridCellIds = arrangementConfig.GetRowIds(newRow);
                CenterScrewsWithinCells(gridCellIds, newRowToAdd);
                ResetAllScrewsPosition();
                CameraSizeHandler.Instance.InitializeSize();
                SaveAllScrewData();
            }
        }

        private BaseScrew SpawnNewScrew(int screwType, int capacity)
        {
            BaseScrew newScrew = ObjectPool.Instance.Spawn(ResourceManager.Instance.GetScrew(screwType), levelScrewsParent);
            newScrew.gameObject.SetActive(true);
            ScrewConfig screwConfig = new ScrewConfig();
            screwConfig.screwType = screwType;
            screwConfig.screwData = new Dictionary<string, object>();
            screwConfig.screwData.Add(ScrewPrefKeys.MAX_CAPACITY, capacity);
            screwConfig.screwData.Add(ScrewPrefKeys.NUT_DATA, new List<NutConfig>());
            newScrew.InitScrew(screwConfig);
            screws.Add(newScrew);
            return newScrew;
        }

        private bool TryToFindMinScrewRaw(out List<BaseScrew> minScrewRaw)
        {
            List<List<BaseScrew>> baseScrews = GetAllRowCells();
            int minScrewCount = int.MaxValue;
            for (int i = 0; i < baseScrews.Count; i++)
            {
                if (baseScrews[i].Count < minScrewCount)
                    minScrewCount = baseScrews[i].Count;
            }
            minScrewRaw = baseScrews.Find(x => x.Count == minScrewCount);
            return minScrewCount != int.MaxValue && !IsRowFull(minScrewRaw);

            bool IsRowFull(List<BaseScrew> row)
            {
                return row.Count == (LevelProgressManager.Instance.ArrangementConfig.gridSize.colNumber / 2) + 1;
            }
        }

        private bool TryToExpandColumns()
        {
            List<List<BaseScrew>> list = GetAllRowCells();
            List<BaseScrew> firstRow = list[0];
            int columnCount = firstRow.Count;
            if (columnCount < MAX_SCREWS_IN_COLUMN)
            {
                ScrewArrangementConfig arrangementConfig = LevelProgressManager.Instance.ArrangementConfig;
                arrangementConfig.gridSize.colNumber += 2;
                arrangementConfig.ReduceCellProperties(2);
                return true;
            }
            return false;
        }

        private bool TryToExpandRow(out int newRow)
        {
            List<List<BaseScrew>> rawScrew = GetAllRowCells();
            if (rawScrew.Count < MAX_SCREWS_IN_ROW)
            {
                ScrewArrangementConfig arrangementConfig = LevelProgressManager.Instance.ArrangementConfig;
                int maxCapacity = GetLastScrewRawMaxCapacity(rawScrew);
                arrangementConfig.gridSize.rowNumber += maxCapacity;
                int lastRowNumber = rawScrew.Last().First().GridCellId.rowNumber;
                newRow = lastRowNumber + maxCapacity;
                return true;
            }
            newRow = -1;
            return false;
        }

        private void CenterScrewsWithinCells(List<GridCellId> availableCellIds, List<BaseScrew> screwsToCenter)
        {
            availableCellIds.Sort((a, b) =>
            {
                int rowCompare = a.rowNumber.CompareTo(b.rowNumber);
                if (rowCompare != 0) return rowCompare;
                return a.colNumber.CompareTo(b.colNumber);
            });


            int n1 = availableCellIds.Count;
            int n2 = screwsToCenter.Count;

            int requiredWidth = (n2 > 0) ? (2 * n2 - 1) : 0;

            if (requiredWidth > n1)
            {
                Debug.LogError($"Cannot center {n2} screws within {n1} available cells using alternate spacing. Required width is {requiredWidth}.");
                return;
            }

            int startIndexInAvailableList = (n1 - requiredWidth) / 2;

            ScrewArrangementConfig currentArrangement = LevelProgressManager.Instance.ArrangementConfig;

            for (int k = 0; k < n2; k++)
            {
                BaseScrew currentScrew = screwsToCenter[k];
                if (currentScrew == null) continue;

                int targetIndexInAvailableList = startIndexInAvailableList + 2 * k;

                GridCellId targetCellId = availableCellIds[targetIndexInAvailableList];

                currentScrew.SetNewGridCellId(targetCellId);
            }
        }

        private void ResetAllScrewsPosition()
        {
            screws.ForEach(x => x?.ResetPosition());
        }

        private void SaveAllScrewData()
        {
            screws.ForEach(x => x?.SaveData());
        }

        private int GetLastScrewRawMaxCapacity(List<List<BaseScrew>> baseScrews)
        {
            int maxCapacity = 0;
            List<BaseScrew> lastRaw = baseScrews.Last();
            for (int i = 0; i < lastRaw.Count; i++)
            {
                if (lastRaw[i].CurrentCapacity > maxCapacity)
                    maxCapacity = lastRaw[i].CurrentCapacity;
            }
            return maxCapacity;
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}