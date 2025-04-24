using ProtoBuf;

namespace Tag.NutSort
{
    [ProtoContract]
    public class LevelStage
    {
        [ProtoMember(1)] public LevelStageArrangementConfig levelArrangementConfig;
        [ProtoMember(2)] public ScrewData[] screwDatas;

        public LevelStage()
        {

        }

        public LevelStage(LevelStage levelStage)
        {
            levelArrangementConfig = new LevelStageArrangementConfig(levelStage.levelArrangementConfig);
            screwDatas = new ScrewData[levelStage.screwDatas.Length];
            for (int index = 0; index < levelStage.screwDatas.Length; ++index)
                screwDatas[index] = new ScrewData(levelStage.screwDatas[index]);
        }
    }
}
