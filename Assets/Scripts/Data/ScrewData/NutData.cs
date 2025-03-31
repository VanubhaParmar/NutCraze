namespace Tag.NutSort
{
    public class NutData
    {
        [NutTypeId] public int nutType;
        [ColorId] public int nutColorTypeId;

        public NutData()
        {
        }

        public NutData(NutData nutData)
        {
            nutType = nutData.nutType;
            nutColorTypeId = nutData.nutColorTypeId;
        }
    }
}
