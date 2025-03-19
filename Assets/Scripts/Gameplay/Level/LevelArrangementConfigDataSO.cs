using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "LevelArrangementConfigDataSO", menuName = Constant.GAME_NAME + "/Level Data/LevelArrangementConfigDataSO")]
    public class LevelArrangementConfigDataSO : SerializedScriptableObject
    {
        #region PUBLIC_VARIABLES
        [SerializeField, LevelArrangementId] private int arrangementId;
        public Vector2Int arrangementGridSize;
        public Vector2 arrangementCellSize;
        public Vector3 arrangementSpacing;

        [Space]
        public List<GridCellId> arrangementCellIds;
        public int ArrangementId { get => arrangementId; set => arrangementId = value; }
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public Vector3 GetCellPosition(GridCellId cellId)
        {
            // Calculate the total width and height of the grid
            float totalWidth = (arrangementCellSize.x + arrangementSpacing.x) * (arrangementGridSize.y - 1);
            float totalHeight = (arrangementCellSize.y + arrangementSpacing.y) * (arrangementGridSize.x - 1);

            // Calculate the offset to center the grid
            float xOffset = totalWidth / 2f;
            float yOffset = totalHeight / 2f;

            //Vector2 gridStartPoint = new Vector2(-xOffset, -yOffset);

            // Calculate the position based on cell size, spacing, and offset
            float xPosition = -xOffset + (cellId.colNumber * (arrangementCellSize.x + arrangementSpacing.x));
            float yPosition = -yOffset + (cellId.rowNumber * (arrangementCellSize.y + arrangementSpacing.y));

            return new Vector3(xPosition, yPosition, arrangementSpacing.z * cellId.rowNumber);
        }

        public Vector3 GetCentrePosition()
        {
            float zPos = ((arrangementGridSize.x - 1) / 2f) * arrangementSpacing.z;
            return new Vector3(0f, 0f, zPos);
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