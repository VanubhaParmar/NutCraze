using Newtonsoft.Json;
using System;

namespace Tag.NutSort
{
    [Serializable]
    public class NutConfig
    {
        [JsonProperty("nt")] public int nutType;
        [JsonProperty("ncti")] public int nutColorTypeId;
        [JsonProperty("ir")] public bool isSurprise;

        public NutConfig()
        {
        }

        public NutConfig(NutData nutData)
        {
            this.nutType = nutData.nutType;
            this.nutColorTypeId = nutData.nutColorTypeId;
        }
    }
}
