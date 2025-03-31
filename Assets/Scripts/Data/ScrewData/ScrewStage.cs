namespace Tag.NutSort
{
    public class ScrewStage
    {
        public bool isStorage;
        public bool isRefresh;
        public bool isGenerator;
        [ColorId] public int color;
        [ColorId] public int curtainColor;
        public NutData[] nutDatas;

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
