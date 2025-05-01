using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Tag.NutSort
{
    [Serializable]
    public class NutConfig
    {
        [JsonProperty("nt")] public int nutType;
        [JsonProperty("nd")] public Dictionary<string, object> nutData;

        public NutConfig()
        {
            nutData = new Dictionary<string, object>();
        }

        public NutConfig(BaseNutLevelDataInfo info)
        {
            nutType = info.nutType;
            nutData = new Dictionary<string, object>();
            nutData.Add(NutPrefKeys.NUT_ID, info.nutColorTypeId);
        }
    }

    public static class NutPrefKeys
    {
        public const string NUT_ID = "ni";
        public const string IS_REVEALED = "ir";
    }
}
