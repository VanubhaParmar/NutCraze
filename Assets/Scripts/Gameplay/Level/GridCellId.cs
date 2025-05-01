using System;

namespace Tag.NutSort
{
    [Serializable]
    public struct GridCellId : IEquatable<GridCellId>
    {
        public int rowNumber;
        public int colNumber;

        public static readonly GridCellId Zero = new GridCellId(0, 0);

        public GridCellId(int rowNumber, int colNumber)
        {
            this.rowNumber = rowNumber;
            this.colNumber = colNumber;
        }

        public override bool Equals(object obj) => obj is GridCellId other && Equals(other);
        public bool Equals(GridCellId other) => rowNumber == other.rowNumber && colNumber == other.colNumber;

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + rowNumber.GetHashCode();
                hash = hash * 23 + colNumber.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(GridCellId a, GridCellId b) => a.Equals(b);
        public static bool operator !=(GridCellId a, GridCellId b) => !(a == b);

        public override string ToString()
        {
            return $"{rowNumber}-{colNumber}"; 
        }

        public static GridCellId Parse(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new ArgumentException("Input string cannot be null or empty for GridCellId parsing.");
            }

            string[] parts = s.Split('-');
            if (parts.Length != 2)
            {
                throw new FormatException($"Input string '{s}' is not in the expected format 'row-column' for GridCellId.");
            }

            if (!int.TryParse(parts[0], out int parsedRow))
            {
                throw new FormatException($"Could not parse row from string part '{parts[0]}'.");
            }
            if (!int.TryParse(parts[1], out int parsedColumn))
            {
                throw new FormatException($"Could not parse column from string part '{parts[1]}'.");
            }

            return new GridCellId(parsedRow, parsedColumn);
        }
    }
}
