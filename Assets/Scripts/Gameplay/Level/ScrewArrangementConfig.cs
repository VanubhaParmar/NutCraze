using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    [Serializable]
    public class ScrewArrangementConfig
    {
        public GridCellId gridSize;
        public CustomVector2 cellSize;
        public CustomVector3 cellSpacing;

        public ScrewArrangementConfig()
        {

        }

        public ScrewArrangementConfig(ScrewArrangementConfig levelArrangementConfig)
        {
            gridSize = levelArrangementConfig.gridSize;
            cellSize = levelArrangementConfig.cellSize;
            cellSpacing = levelArrangementConfig.cellSpacing;
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

        public List<GridCellId> GetRowIds(int rowNumber)
        {
            List<GridCellId> rowIds = new List<GridCellId>();
            for (int i = 0; i < gridSize.colNumber; i++)
            {
                rowIds.Add(new GridCellId(rowNumber, i));
            }
            return rowIds;
        }

        public void ReduceCellProperties(int percent)
        {
            ReduceCellSpacing(percent);
            ReduceCellSize(percent);

            void ReduceCellSpacing(int percent)
            {
                cellSpacing.x = cellSpacing.x * (1 - (percent / 100f));
                cellSpacing.y = cellSpacing.y * (1 - (percent / 100f));
            }
            void ReduceCellSize(int percent)
            {
                cellSize.x = cellSize.x * (1 - (percent / 100f));
                cellSize.y = cellSize.y * (1 - (percent / 100f));
            }
        }



    }
}
