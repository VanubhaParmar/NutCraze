using Sirenix.OdinInspector;
using UnityEngine;

namespace Tag.NutSort.LevelEditor
{
    public class GridCell : SerializedMonoBehaviour
    {
        #region PUBLIC_VARIABLES
        public GridCellId CellId => _cellId;
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private GridCellId _cellId;
        [SerializeField] private SpriteRenderer _mySpriteRenderer;

        private BaseScrew baseScrew;
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
}