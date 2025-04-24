using ProtoBuf;

namespace Tag.NutSort
{
    [ProtoContract]
    public class ScrewData
    {
        [ProtoMember(1)] public int id;
        [ProtoMember(2)] public int screwType;
        [ProtoMember(3)] public int capacity = 4;
        [ProtoMember(4)] public ScrewStage[] screwStages;

        public ScrewData()
        {
        }

        public ScrewData(ScrewData screwData)
        {
            id = screwData.id;
            screwType = screwData.screwType;
            capacity = screwData.capacity;
            screwStages = new ScrewStage[screwData.screwStages.Length];
            for (int index = 0; index < screwData.screwStages.Length; ++index)
                screwStages[index] = new ScrewStage(screwData.screwStages[index]);
        }
    }
}
