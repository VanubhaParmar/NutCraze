using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Tag.NutSort {
    [Serializable]
    public class TutorialsPlayerData
    {
        [JsonProperty("tpdL")] public List<TutorialPlayerData> tutorialPlayerDatas = new();
    }

    [Serializable]
    public class TutorialPlayerData
    {
        [JsonProperty("tt")] public TutorialType tutorialType; 
        [JsonProperty("ts")] public TutorialState tutorialState;
    }
}
