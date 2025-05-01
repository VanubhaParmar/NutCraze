using Newtonsoft.Json;
using System;

namespace Tag.NutSort
{
    [Serializable]
    public class MoveData
    {
        [JsonProperty("fs")] public GridCellId fromScrew;
        [JsonProperty("ts")] public GridCellId toScrew;
        [JsonProperty("tn")] public int transferedNuts;

        public MoveData()
        {
        }

        public MoveData(MoveData moveData)
        {
            this.fromScrew = moveData.fromScrew;
            this.toScrew = moveData.toScrew;
            this.transferedNuts = moveData.transferedNuts;
        }

        public MoveData(GridCellId fromScrew, GridCellId toScrew, int transferedNuts)
        {
            this.fromScrew = fromScrew;
            this.toScrew = toScrew;
            this.transferedNuts = transferedNuts;
        }
    }
}
