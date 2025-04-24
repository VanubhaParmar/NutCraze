using ProtoBuf;

namespace Tag.NutSort
{
    [ProtoContract]
    public class LevelData
    {
        [ProtoMember(1)] public int level;
        [ProtoMember(2)] public LevelType levelType;
        [ProtoMember(3)] public LevelStage[] stages;

        public LevelData()
        {
        }

        public LevelData(LevelData levelData)
        {
            level = levelData.level;
            levelType = levelData.levelType;
            stages = new LevelStage[levelData.stages.Length];
            for (int index = 0; index < levelData.stages.Length; ++index)
                stages[index] = new LevelStage(levelData.stages[index]);
        }
    }
}
