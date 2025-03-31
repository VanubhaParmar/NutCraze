namespace Tag.NutSort
{
    public class LevelData
    {
        public int level;
        public LevelType levelType;
        public LevelStage[] stages;

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
