using Newtonsoft.Json;

namespace com.tag.nut_sort {
    [System.Serializable]
    public class GridCellId
    {
        [JsonProperty("row")] public int rowNumber;
        [JsonProperty("col")] public int colNumber;

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