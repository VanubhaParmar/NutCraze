using ProtoBuf;

namespace Tag.NutSort
{
    [ProtoContract]
    public class NutData
    {
        [ProtoMember(1)] public int nutType;
        [ProtoMember(2)] public int nutColorTypeId;

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
