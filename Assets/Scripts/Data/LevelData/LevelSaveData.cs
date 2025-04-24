using Newtonsoft.Json;
using System;

namespace Tag.NutSort
{
    [Serializable]
    public class LevelSaveData
    {
        [JsonProperty("abtt")] public ABTestType aBTestType;
        [JsonProperty("lvl")] public int level;
        [JsonProperty("lt")] public LevelType levelType;
        [JsonProperty("rt")] public int runTime;
        [JsonProperty("mvs")] public MoveData[] moves;
        [JsonProperty("cs")] public LevelStageConfig currentStage;
        [JsonProperty("lssd")] public LevelStageConfig[] pendingLevelStage;

        public LevelSaveData()
        {
        }

        public LevelSaveData(ABTestType aBTestType, LevelData levelData)
        {
            this.aBTestType = aBTestType;
            this.level = levelData.level;
            this.levelType = levelData.levelType;
            moves = new MoveData[0];
            this.pendingLevelStage = new LevelStageConfig[levelData.stages.Length];
            for (int index = 0; index < levelData.stages.Length; ++index)
                this.pendingLevelStage[index] = new LevelStageConfig(levelData.stages[index]);
        }
    }

    [Serializable]
    public class MoveData
    {
        [JsonProperty("fs")] public int fromScrew;
        [JsonProperty("ts")] public int toScrew;
        [JsonProperty("tn")] public int transferedNuts;
    }

    [Serializable]
    public class LevelStageConfig
    {
        [JsonProperty("arr")] public int arrangementId;
        [JsonProperty("gs")] public GridCellId gridSize;
        [JsonProperty("ssc")] public ScrewConfig[] screwConfigs;

        public LevelStageConfig() { }

        public LevelStageConfig(LevelStage levelStage)
        {
            //this.arrangementId = levelStage.arrangementId;
            this.screwConfigs = new ScrewConfig[levelStage.screwDatas.Length];
            for (int index = 0; index < levelStage.screwDatas.Length; ++index)
                this.screwConfigs[index] = new ScrewConfig(levelStage.screwDatas[index]);
        }
    }
}
