using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Tag.NutSort
{
    [Serializable]
    public class ScrewConfig
    {
        [JsonProperty("id")] public int id;
        [JsonProperty("cid")] public GridCellId cellId;
        [JsonProperty("st")] public int screwType;
        [JsonProperty("sz")] public int capacity;
        [JsonProperty("scm")] public Dictionary<int, bool> stageCompletedMapping;
        [JsonProperty("cs")] public ScrewStageConfig currentStage;
        [JsonProperty("ss")] public ScrewStageConfig[] pendingStages;

        public ScrewConfig()
        {
            this.stageCompletedMapping = new Dictionary<int, bool>();
            for (int i = 0; i < pendingStages.Length; i++)
                stageCompletedMapping.Add(i, false);
        }

        public ScrewConfig(ScrewData screwData) : base()
        {
            this.id = screwData.id;
            this.screwType = screwData.screwType;
            this.capacity = screwData.capacity;
            this.pendingStages = new ScrewStageConfig[screwData.screwStages.Length];
            for (int index = 0; index < screwData.screwStages.Length; ++index)
                this.pendingStages[index] = new ScrewStageConfig(screwData.screwStages[index]);
        }

        public void CompleteStage(int stageIndex)
        {
            if (stageCompletedMapping.ContainsKey(stageIndex))
                stageCompletedMapping[stageIndex] = true;
        }


        public bool IsAllStagesCompleted()
        {
            foreach (var stage in stageCompletedMapping)
            {
                if (!stage.Value)
                    return false;
            }
            return true;
        }

        public bool IsStageCompleted(int stageIndex)
        {
            return stageCompletedMapping.ContainsKey(stageIndex) && stageCompletedMapping[stageIndex];
        }

        public bool TryGetScrewStage(int stageIndex, out ScrewStageConfig screwStageSaveConfig)
        {
            screwStageSaveConfig = null;
            if (stageIndex < pendingStages.Length)
            {
                screwStageSaveConfig = pendingStages[stageIndex];
                return true;
            }
            return false;
        }
    }
}
