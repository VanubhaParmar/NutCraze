namespace Tag.NutSort
{
    public class LevelStage
    {
        public LevelStageArrangementConfig levelArrangementConfig;
        public ScrewData[] screwDatas;

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
