using ProtoBuf;

namespace Tag.NutSort
{
    [ProtoContract]
    public class ScrewStage
    {
        [ProtoMember(1)] public bool isStorage;
        [ProtoMember(2)] public bool isRefresh;
        [ProtoMember(3)] public bool isGenerator;
        [ProtoMember(4)] public int color;
        [ProtoMember(5)] public int curtainColor;
        [ProtoMember(6)] public NutData[] nutDatas;

        public ScrewStage()
        {
        }

        public ScrewStage(ScrewStage screwStage)
        {
            isStorage = screwStage.isStorage;
            isRefresh = screwStage.isRefresh;
            isGenerator = screwStage.isGenerator;
            color = screwStage.color;
            curtainColor = screwStage.curtainColor;
            nutDatas = new NutData[screwStage.nutDatas.Length];
            for (int index = 0; index < screwStage.nutDatas.Length; ++index)
                nutDatas[index] = new NutData(screwStage.nutDatas[index]);
        }
    }
}
