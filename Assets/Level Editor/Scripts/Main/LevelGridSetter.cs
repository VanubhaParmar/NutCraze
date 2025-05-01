using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort.Editor
{
    public class LevelGridSetter : SerializedMonoBehaviour
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private LevelGridCell cellPrefab;
        [SerializeField] private Transform gridParent;
        [ShowInInspector, ReadOnly] private ScrewArrangementConfig screwArrangementConfig;

        private List<LevelGridCell> _generatedGridCells = new List<LevelGridCell>();
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        [Button]
        public void ShowGrid(ScrewArrangementConfigSO levelArrangementConfigDataSO)
        {
            this.screwArrangementConfig = levelArrangementConfigDataSO.GetArrangementConfig();
            SetGrid();
        }

        [Button]
        public void ShowGrid(ScrewArrangementConfig screwArrangementConfig)
        {
            this.screwArrangementConfig = screwArrangementConfig;
            SetGrid();
        }

        [Button]
        public void ResetGrid()
        {
            _generatedGridCells.ForEach(x => x.DestroyCell());
            _generatedGridCells.Clear();

            if (gridParent.childCount > 0)
            {
                var allCells = gridParent.GetComponentsInChildren<LevelGridCell>();
                foreach (var cell in allCells)
                {
                    cell.DestroyCell();
                }
            }
        }
        #endregion

        #region PRIVATE_METHODS
        private void SetGrid()
        {
            ResetGrid();
            InstantiateGridCells();
        }

        private void InstantiateGridCells()
        {
            for (int i = 0; i < screwArrangementConfig.gridSize.rowNumber; i++)
            {
                for (int j = 0; j < screwArrangementConfig.gridSize.colNumber; j++)
                {
                    LevelGridCell cellInstance = InstantiateGridCell();

                    GridCellId gridCellId;
                    gridCellId.rowNumber = i;
                    gridCellId.colNumber = j;
                    cellInstance.InitGridCell(gridCellId);
                    cellInstance.transform.position = screwArrangementConfig.GetCellPosition(cellInstance.CellId);
                    cellInstance.SetSize(screwArrangementConfig.cellSize.GetVector());
                }
            }
        }

        private LevelGridCell InstantiateGridCell()
        {
            LevelGridCell cellInstance = Instantiate(cellPrefab, gridParent);
            cellInstance.gameObject.SetActive(true);
            _generatedGridCells.Add(cellInstance);

            return cellInstance;
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