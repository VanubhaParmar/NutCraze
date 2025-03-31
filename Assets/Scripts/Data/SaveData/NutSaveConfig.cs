using Newtonsoft.Json;
using System;

namespace Tag.NutSort
{
    [Serializable]
    public class NutSaveConfig
    {
        [JsonProperty("nt")] public int nutType;
        [JsonProperty("ncti")] public int nutColorTypeId;

        public NutSaveConfig()
        {
        }

        public NutSaveConfig(NutData nutData)
        {
            this.nutType = nutData.nutType;
            this.nutColorTypeId = nutData.nutColorTypeId;
        }
    }
}
