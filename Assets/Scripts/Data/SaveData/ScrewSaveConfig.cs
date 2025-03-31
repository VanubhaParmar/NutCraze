using Newtonsoft.Json;
using System;

namespace Tag.NutSort
{
    [Serializable]
    public class ScrewSaveConfig
    {
        [JsonProperty("id")] public int id;
        [JsonProperty("cid")] public GridCellId cellId;
        [JsonProperty("st")] public int screwType;
        [JsonProperty("sz")] public int size;
        [JsonProperty("cs")] public int currentStage;
        [JsonProperty("ss")] public ScrewStageSaveConfig[] screwStages;

        public ScrewSaveConfig()
        {
        }

        public ScrewSaveConfig(ScrewData screwData)
        {
            this.id = screwData.id;
            this.screwType = screwData.screwType;
            this.size = screwData.size;
            this.currentStage = 0;
            this.screwStages = new ScrewStageSaveConfig[screwData.screwStages.Length];
            for (int index = 0; index < screwData.screwStages.Length; ++index)
                this.screwStages[index] = new ScrewStageSaveConfig(screwData.screwStages[index]);
        }

    }
}
