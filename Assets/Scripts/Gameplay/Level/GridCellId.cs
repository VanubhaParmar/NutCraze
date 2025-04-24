using Newtonsoft.Json;
using ProtoBuf;

namespace Tag.NutSort
{
    [System.Serializable, ProtoContract]
    public struct GridCellId
    {
        [JsonProperty("row"), ProtoMember(1)] public int rowNumber;
        [JsonProperty("col"), ProtoMember(2)] public int colNumber;

        [JsonIgnore] public static readonly GridCellId Zero = new GridCellId(0, 0);

        public GridCellId(int rowNumber, int colNumber)
        {
            this.rowNumber = rowNumber;
            this.colNumber = colNumber;
        }

        public static bool operator ==(GridCellId a, GridCellId b)
        {
            return a.rowNumber == b.rowNumber && a.colNumber == b.colNumber;
        }

        public static bool operator !=(GridCellId a, GridCellId b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is GridCellId))
                return false;

            return this == (GridCellId)obj;
        }

        public override int GetHashCode()
        {
            return rowNumber.GetHashCode() ^ colNumber.GetHashCode();
        }
    }
}