namespace Tag.NutSort
{
    public class ScrewData
    {
        public int id;
        [ScrewTypeId] public int screwType;
        public int size = 4;
        public ScrewStage[] screwStages;

        public ScrewData()
        {
        }

        public ScrewData(ScrewData screwData)
        {
            id = screwData.id;
            screwType = screwData.screwType;
            size = screwData.size;
            screwStages = new ScrewStage[screwData.screwStages.Length];
            for (int index = 0; index < screwData.screwStages.Length; ++index)
                screwStages[index] = new ScrewStage(screwData.screwStages[index]);
        }
    }
}
