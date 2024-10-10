using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class LevelGridSetter : SerializedMonoBehaviour
    {
        #region PUBLIC_VARIABLES
        public List<BaseScrew> screws;
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private LevelGridCell cellPrefab;
        [SerializeField] private Transform gridParent;
        [SerializeField] private LevelArrangementConfigDataSO levelArrangementConfigDataSO;

        private List<LevelGridCell> _generatedGridCells = new List<LevelGridCell>();
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        [Button]
        public void SetScrews()
        {
            for (int i = 0; i < levelArrangementConfigDataSO.arrangementCellIds.Count; i++)
            {
                screws[i].transform.position = levelArrangementConfigDataSO.GetCellPosition(levelArrangementConfigDataSO.arrangementCellIds[i]);
            }
        }

        [Button]
        public void SetGrid()
        {
            ResetGrid();
            InstantiateGridCells();
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
        private void InstantiateGridCells()
        {
            for (int i = 0; i < levelArrangementConfigDataSO.arrangementGridSize.x; i++)
            {
                for (int j = 0; j < levelArrangementConfigDataSO.arrangementGridSize.y; j++)
                {
                    LevelGridCell cellInstance = InstantiateGridCell();

                    cellInstance.InitGridCell(new GridCellId(i, j));
                    cellInstance.transform.position = levelArrangementConfigDataSO.GetCellPosition(cellInstance.CellId);
                    cellInstance.SetSize(levelArrangementConfigDataSO.arrangementCellSize);
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