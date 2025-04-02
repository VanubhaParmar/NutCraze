using UnityEngine;

namespace Tag.NutSort
{
    public class LevelStageArrangementConfig
    {
        public GridCellId gridSize;
        public CustomVector2 cellSize;
        public CustomVector3 cellSpacing;
        public LevelStageArrangementConfig()
        {
            
        }

        public LevelStageArrangementConfig(LevelStageArrangementConfig levelStageArrangementConfig)
        {
            gridSize = levelStageArrangementConfig.gridSize;
            cellSize = levelStageArrangementConfig.cellSize;
            cellSpacing = levelStageArrangementConfig.cellSpacing;
        }

        public Vector3 GetCellPosition(GridCellId cellId)
        {
            float totalWidth = (cellSize.x + cellSpacing.x) * (gridSize.colNumber - 1);
            float totalHeight = (cellSize.y + cellSpacing.y) * (gridSize.rowNumber - 1);

            float xOffset = totalWidth / 2f;
            float yOffset = totalHeight / 2f;


            float xPosition = -xOffset + (cellId.colNumber * (cellSize.x + cellSpacing.x));
            float yPosition = -yOffset + (cellId.rowNumber * (cellSize.y + cellSpacing.y));

            return new Vector3(xPosition, yPosition, cellSpacing.z * cellId.rowNumber);
        }

        public Vector3 GetCentrePosition()
        {
            float zPos = ((gridSize.rowNumber - 1) / 2f) * cellSpacing.z;
            return new Vector3(0f, 0f, zPos);
        }



        //public Vector3 GetCellPosition(GridCellId cellId)
        //{
        //    float totalWidth = (cellSize.x * gridSize.colNumber) + (cellSpacing.x * (gridSize.colNumber - 1));
        //    if (gridSize.colNumber <= 1) totalWidth = cellSize.x; 

        //    float totalHeight = (cellSize.y * gridSize.rowNumber) + (cellSpacing.y * (gridSize.rowNumber - 1));
        //    if (gridSize.rowNumber <= 1) totalHeight = cellSize.y;

        //    float xOffset = totalWidth / 2f;
        //    float yOffset = totalHeight / 2f;

        //    float startX = -xOffset + (cellSize.x / 2f);
        //    float startY = -yOffset + (cellSize.y / 2f);


        //    float xPosition = startX + (cellId.colNumber * (cellSize.x + cellSpacing.x));
        //    float yPosition = startY + (cellId.rowNumber * (cellSize.y + cellSpacing.y));

        //    float zPosition = cellId.rowNumber * cellSpacing.z;

        //    return new Vector3(xPosition, yPosition, zPosition);
        //}

        //public Vector3 GetCentrePosition()
        //{
        //    if (gridSize.rowNumber <= 0)
        //    {
        //        return Vector3.zero;
        //    }

        //    float centerRowIndex = (gridSize.rowNumber - 1) / 2.0f;

        //    float zPos = centerRowIndex * cellSpacing.z;

        //    return new Vector3(0f, 0f, zPos);
        //}
    }
}
