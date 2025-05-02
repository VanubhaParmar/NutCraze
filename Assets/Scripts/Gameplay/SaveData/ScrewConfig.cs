using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Tag.NutSort
{
    [Serializable]
    public class ScrewConfig
    {
        [JsonProperty("st")] public int screwType;
        [JsonProperty("sd")] public Dictionary<string, object> screwData;

        public ScrewConfig()
        {
        }

        public ScrewConfig(int id, BaseScrewLevelDataInfo info)
        {
            screwType = info.screwType;
            screwData = new Dictionary<string, object>();
            //screwData.Add(ScrewPrefKeys.SCREW_ID, id);
            screwData.Add(ScrewPrefKeys.MAX_CAPACITY, info.screwNutsCapacity);
            screwData.Add(ScrewPrefKeys.NUT_DATA, new List<NutConfig>());
        }

        public ScrewConfig(int id, BaseScrewLevelDataInfo info, ScrewNutsLevelDataInfo screwNutsLevelDataInfo)
        {
            screwType = info.screwType;
            screwData = new Dictionary<string, object>();
            //screwData.Add(ScrewPrefKeys.SCREW_ID, id);
            screwData.Add(ScrewPrefKeys.MAX_CAPACITY, info.screwNutsCapacity);
            screwData.Add(ScrewPrefKeys.NUT_DATA, GetNutData());

            List<NutConfig> GetNutData()
            {
                List<NutConfig> nutConfigs = new List<NutConfig>();
                for (int j = 0; j < screwNutsLevelDataInfo.levelNutDataInfos.Count; j++)
                    nutConfigs.Add(new NutConfig(screwNutsLevelDataInfo.levelNutDataInfos[j]));
                return nutConfigs;
            }
        }
    }

    public static class ScrewPrefKeys
    {
        //public const string SCREW_ID = "id";
        public const string MAX_CAPACITY = "mcp";
        public const string NUT_DATA = "nd";
        public const string CURRENT_CAPACITY = "ccp";
    }
}
