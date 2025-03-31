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
        [JsonProperty("cs")] public int currentStage;
        [JsonProperty("mvs")] public MoveData[] moves;
        [JsonProperty("lssd")] public LevelStageSaveData[] levelStageSaveData;

        public LevelSaveData()
        {
        }

        public LevelSaveData(ABTestType aBTestType, LevelData levelData)
        {
            this.aBTestType = aBTestType;
            this.level = levelData.level;
            this.levelType = levelData.levelType;
            this.currentStage = 0;
            moves = new MoveData[0];
            this.levelStageSaveData = new LevelStageSaveData[levelData.stages.Length];
            for (int index = 0; index < levelData.stages.Length; ++index)
                this.levelStageSaveData[index] = new LevelStageSaveData(levelData.stages[index]);
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
    public class LevelStageSaveData
    {
        [JsonProperty("arr")] public int arrangementId;
        [JsonProperty("gs")] public GridCellId gridSize;
        [JsonProperty("ssc")] public ScrewSaveConfig[] screwSaveConfigs;

        public LevelStageSaveData() { }

        public LevelStageSaveData(LevelStage levelStage)
        {
            this.arrangementId = levelStage.arrangementId;
            this.screwSaveConfigs = new ScrewSaveConfig[levelStage.screwDatas.Length];
            for (int index = 0; index < levelStage.screwDatas.Length; ++index)
                this.screwSaveConfigs[index] = new ScrewSaveConfig(levelStage.screwDatas[index]);
        }
    }
}
