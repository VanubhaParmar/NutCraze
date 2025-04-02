using Newtonsoft.Json;
using System;

namespace Tag.NutSort
{
    [Serializable]
    public class ScrewStageConfig
    {
        [JsonProperty("is")] public bool isStorage;
        [JsonProperty("ir")] public bool isRefresh;
        [JsonProperty("ig")] public bool isGenerator;
        [JsonProperty("clr")] public int color;
        [JsonProperty("cclr")] public int curtainColor;
        [JsonProperty("nd")] public NutConfig[] nutDatas;

        public ScrewStageConfig()
        {
        }

        public ScrewStageConfig(ScrewStage screwStage)
        {
            this.isStorage = screwStage.isStorage;
            this.isRefresh = screwStage.isRefresh;
            this.isGenerator = screwStage.isGenerator;
            this.color = screwStage.color;
            this.curtainColor = screwStage.curtainColor;
            this.nutDatas = new NutConfig[screwStage.nutDatas.Length];
            for (int index = 0; index < screwStage.nutDatas.Length; ++index)
                this.nutDatas[index] = new NutConfig(screwStage.nutDatas[index]);
        }
    }
}
