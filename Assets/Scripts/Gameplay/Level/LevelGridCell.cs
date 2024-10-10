using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class LevelGridCell : SerializedMonoBehaviour
    {
        #region PUBLIC_VARIABLES
        public GridCellId CellId => _cellId;
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private GridCellId _cellId;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void InitGridCell(GridCellId cellId)
        {
            this._cellId = cellId;
            gameObject.name = $"{cellId.rowNumber} - {cellId.colNumber}";
        }

        public void SetSize(Vector2 cellSize)
        {
            transform.localScale = cellSize;
        }

        public void DestroyCell()
        {
            DestroyImmediate(gameObject);
        }
        #endregion

        #region PRIVATE_METHODS
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }

    [System.Serializable]
    public class GridCellId
    {
        public int rowNumber;
        public int colNumber;

        public bool IsEqual(GridCellId cellId)
        {
            return rowNumber == cellId.rowNumber && colNumber == cellId.colNumber;
        }

        public GridCellId() { }

        public GridCellId(GridCellId cellId)
        {
            this.rowNumber = cellId.rowNumber;
            this.colNumber = cellId.colNumber;
        }

        public GridCellId(int rowNumber, int colNumber)
        {
            this.rowNumber = rowNumber;
            this.colNumber = colNumber;
        }

        public GridCellId Clone()
        {
            return new GridCellId(this);
        }
    }
}