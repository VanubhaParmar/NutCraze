using Newtonsoft.Json;

namespace com.tag.nut_sort {
    public class SerializeUtility
    {
        #region PRIVATE_VARS
        private static JsonSerializerSettings settings;
        public static JsonSerializerSettings JsonSerializerSettings
        {
            get
            {
                if (settings == null)
                    settings = new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    };
                return settings;
            }
        }
        #endregion

        #region PUBLIC_FUNCTIONS
        public static string SerializeObject<T>(T value)
        {
            return JsonConvert.SerializeObject(value, JsonSerializerSettings);
        }
        public static T DeserializeObject<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value, JsonSerializerSettings);
        }
        #endregion
    }
}