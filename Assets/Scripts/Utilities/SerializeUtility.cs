using Newtonsoft.Json;

namespace Tag.NutSort
{
    public class SerializeUtility
    {
        #region PRIVATE_VARS
        private static JsonSerializerSettings settings;
        public static JsonSerializerSettings Settings
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
            return JsonConvert.SerializeObject(value, Settings);
        }
        public static T DeserializeObject<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value, Settings);
        }
        #endregion
    }
}
