using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Tag.NutSort
{
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
        //public static T GetPlayerPrefsValue<T>(string key)
        //{
        //    T defaultValue = default;
        //    PersistantVariable<T> persistantVariable = new PersistantVariable<T>(key, defaultValue);
        //    return persistantVariable.Value;
        //}

        //public static T SetPlayerPrefsValue<T>(string key, T value)
        //{
        //    PersistantVariable<T> persistantVariable = new PersistantVariable<T>(key);
        //    return persistantVariable.Value = value;
        //}

        //For Data Parsing From local Variables to Class Configs

        public static X RetrieveAndErasePrefs<X>(string key, X defaultValue)
        {
            // Check the type of defaultValue and get the value accordingly
            if (typeof(X) == typeof(int))
            {
                if (PlayerPrefs.HasKey(key))
                {
                    int value = PlayerPrefs.GetInt(key);
                    PlayerPrefs.DeleteKey(key); // Optional: Delete after retrieving
                    return (X)(object)value;
                }
            }
            else if (typeof(X) == typeof(float))
            {
                if (PlayerPrefs.HasKey(key))
                {
                    float value = PlayerPrefs.GetFloat(key);
                    PlayerPrefs.DeleteKey(key); // Optional: Delete after retrieving
                    return (X)(object)value;
                }
            }
            else if (typeof(X) == typeof(string))
            {
                if (PlayerPrefs.HasKey(key))
                {
                    string value = PlayerPrefs.GetString(key);
                    PlayerPrefs.DeleteKey(key); // Optional: Delete after retrieving
                    return (X)(object)value;
                }
            }
            if (PlayerPrefs.HasKey(key))
            {
                string value = PlayerPrefs.GetString(key);
                PlayerPrefs.DeleteKey(key); // Optional: Delete after retrieving
                return DeserializeObject<X>(value);
            }
            return defaultValue;
        }

        public static X RetrieveAndErasePrefs<X>(string key)
        {
            X defaultValue = default;
            return RetrieveAndErasePrefs(key, defaultValue);
        }
        #endregion


    }
}
