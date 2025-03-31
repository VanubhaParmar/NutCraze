namespace Tag.NutSort
{
    public class LevelStage
    {
        [LevelArrangementId] public int arrangementId;
        public ScrewData[] screwDatas;

        public LevelStage()
        {

        }

        public LevelStage(LevelStage levelStage)
        {
            arrangementId = levelStage.arrangementId;
            screwDatas = new ScrewData[levelStage.screwDatas.Length];
            for (int index = 0; index < levelStage.screwDatas.Length; ++index)
                screwDatas[index] = new ScrewData(levelStage.screwDatas[index]);
        }
    }
}
