using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Tag.NutSort
{
    [Serializable]
    public class LevelSaveData
    {
        [JsonProperty("pl")] public int level;
        [JsonProperty("ly")] public LevelType levelType;
        [JsonProperty("mvs")] public List<MoveData> moves;
        [JsonProperty("arrc")] public ScrewArrangementConfig arrangementConfig;
        [JsonProperty("sc")] public Dictionary<string, ScrewConfig> screws;
        [JsonProperty("crt")] public int runTime;
        [JsonProperty("bud")] public Dictionary<int, int> boostersUsed = new Dictionary<int, int>();
        [JsonProperty("aw")] public int adWatched;
        [JsonProperty("lfc")] public LevelFailSaveData levelFailSaveData;

        public LevelSaveData()
        {
        }

        public LevelSaveData(LevelDataSO levelDataSO)
        {
            level = levelDataSO.level;
            levelType = levelDataSO.levelType;
            moves = new List<MoveData>();
            ScrewArrangementConfigSO screwArrangementConfigSO = LevelManager.Instance.GetCurrentArrangementConfigSO(levelDataSO.ArrangementId);
            arrangementConfig = screwArrangementConfigSO.GetArrangementConfig();
            screws = new Dictionary<string, ScrewConfig>();
            levelFailSaveData = new LevelFailSaveData();
            for (int i = 0; i < levelDataSO.levelScrewDataInfos.Count; i++)
            {
                if (i >= screwArrangementConfigSO.arrangementCellIds.Count)
                {
                    break;
                }

                GridCellId gridCellId = screwArrangementConfigSO.arrangementCellIds[i];
                BaseScrewLevelDataInfo screwData = levelDataSO.levelScrewDataInfos[i];
                if (TryGetNutData(gridCellId, out ScrewNutsLevelDataInfo nutData))
                {
                    screws.Add(gridCellId.ToString(), new ScrewConfig(i, screwData, nutData));
                }
                else
                {
                    screws.Add(gridCellId.ToString(), new ScrewConfig(i, screwData));
                }

            }

            bool TryGetNutData(GridCellId gridCellId, out ScrewNutsLevelDataInfo nutData)
            {
                nutData = levelDataSO.screwNutsLevelDataInfos.Find(x => x.targetScrewGridCellId == gridCellId);
                return nutData != null;
            }

        }
    }
}

