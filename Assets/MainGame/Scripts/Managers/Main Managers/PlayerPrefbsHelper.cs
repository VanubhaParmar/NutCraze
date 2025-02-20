using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;


namespace Tag.NutSort {
    public class PlayerPrefbsHelper
    {
        #region private veriables

        private static Dictionary<string, int> intPrefs = new Dictionary<string, int>();
        private static Dictionary<string, string> stringPrefs = new Dictionary<string, string>();
        private static Dictionary<string, float> floatPrefs = new Dictionary<string, float>();
        private static Dictionary<string, List<Action>> onValueChange = new Dictionary<string, List<Action>>();

        private const string INT_PREFS = "intPrefab";
        private const string FLOAT_PREFS = "floatPrefab";
        private const string STRING_PREFS = "stringPrefab";
        #endregion

        #region propertices

        public static bool SaveData { get; set; }

        #endregion


        #region public methods

        public static void SaveAllData()
        {
            foreach (var i in intPrefs)
                PlayerPrefs.SetInt(i.Key, i.Value);

            foreach (var s in stringPrefs)
                PlayerPrefs.SetString(s.Key, s.Value);

            foreach (var f in floatPrefs)
                PlayerPrefs.SetFloat(f.Key, f.Value);
        }

        public static void SaveDataInFile()
        {
            string fileName = Constant.GAME_NAME + "-Data" + TimeManager.Now + ".txt";
            fileName = fileName.Replace(":", "-");
            FileStream file = File.Create(Application.persistentDataPath + "/" + fileName);
            Debug.LogError(Application.persistentDataPath + "/" + fileName);
            Dictionary<string, object> d = new Dictionary<string, object>();
            d.Add(INT_PREFS, GetIntPrefab());
            d.Add(FLOAT_PREFS, GetFloatPrefab());
            d.Add(STRING_PREFS, GetStringPrefab());
            string data = JsonConvert.SerializeObject(d);
            byte[] dataArray = Encoding.ASCII.GetBytes(data);
            file.Write(dataArray, 0, dataArray.Length);
            file.Close();
        }

        public static void SaveDataToPrefabs(string filePath)
        {
#if !UNITY_EDITOR
            filePath = filePath.Replace("content", "file");
            Debug.LogError(filePath);
#endif
            if (!File.Exists(filePath))
            {
                Debug.Log("File Not exist");
                return;
            }

            string data = File.ReadAllText(filePath);
            Debug.Log(data);
            DeleteAllLocalSaveKey();
            Dictionary<string, object> d = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);

            if (d.ContainsKey(INT_PREFS))
                intPrefs = (d[INT_PREFS].GetJObjectCast<Dictionary<string, int>>());

            if (d.ContainsKey(FLOAT_PREFS))
                floatPrefs = (d[FLOAT_PREFS].GetJObjectCast<Dictionary<string, float>>());

            if (d.ContainsKey(STRING_PREFS))
                stringPrefs = (d[STRING_PREFS].GetJObjectCast<Dictionary<string, string>>());

            SaveAllData();
        }

        public static int GetInt(string key, int defaultValue = 0)
        {
            if (!intPrefs.ContainsKey(key))
            {
                intPrefs.Add(key, PlayerPrefs.GetInt(key, defaultValue));
            }
            return intPrefs[key];
        }

        public static int GetSavedInt(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        public static void SetInt(string key, int value)
        {
            if (!intPrefs.ContainsKey(key))
                intPrefs.Add(key, value);
            else
                intPrefs[key] = value;
            if (SaveData)
                PlayerPrefs.SetInt(key, value);
            OnValueChange(key);
        }

        public static string GetString(string key, string defaultValue = "")
        {
            if (!stringPrefs.ContainsKey(key))
            {
                stringPrefs.Add(key, PlayerPrefs.GetString(key, defaultValue));
            }
            return stringPrefs[key];
        }

        public static string GetSavedString(string key, string defaultValue = "")
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }

        public static void SetString(string key, string value)
        {
            if (!stringPrefs.ContainsKey(key))
                stringPrefs.Add(key, value);
            else
                stringPrefs[key] = value;
            if (SaveData)
                PlayerPrefs.SetString(key, value);
            OnValueChange(key);
        }

        public static float GetFloat(string key, float defaultValue = 0)
        {
            if (!floatPrefs.ContainsKey(key))
            {
                floatPrefs.Add(key, PlayerPrefs.GetFloat(key, defaultValue));
            }
            return floatPrefs[key];
        }

        public static float GetSavedFloat(string key, float defaultValue = 0)
        {
            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        public static void SetFloat(string key, float value)
        {
            if (!floatPrefs.ContainsKey(key))
                floatPrefs.Add(key, value);
            else
                floatPrefs[key] = value;
            if (SaveData)
                PlayerPrefs.SetFloat(key, value);
            OnValueChange(key);
        }

        public static void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
            if (intPrefs.ContainsKey(key))
                intPrefs.Remove(key);
            if (stringPrefs.ContainsKey(key))
                stringPrefs.Remove(key);
            if (floatPrefs.ContainsKey(key))
                floatPrefs.Remove(key);
        }

        public static void DeleteAllLocalSaveKey()
        {
            PlayerPrefs.DeleteAll();
            intPrefs.Clear();
            stringPrefs.Clear();
            floatPrefs.Clear();
            onValueChange.Clear();
        }

        public static void DeleteAllKey()
        {
            intPrefs.Clear();
            stringPrefs.Clear();
            floatPrefs.Clear();
        }

        public static bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public static void RegisterEvent(string key, Action action)
        {
            if (!onValueChange.ContainsKey(key))
                onValueChange.Add(key, new List<Action>());
            if (!onValueChange[key].Contains(action))
                onValueChange[key].Add(action);
        }

        public static void DeregisterEvent(string key, Action action)
        {
            if (onValueChange.ContainsKey(key) && onValueChange[key].Contains(action))
                onValueChange[key].Remove(action);
        }

        #endregion

        #region private methods

        private static void OnValueChange(string key)
        {
            if (onValueChange.ContainsKey(key))
            {
                for (int i = 0; i < onValueChange[key].Count; i++)
                {
                    if (onValueChange[key][i] != null)
                        onValueChange[key][i].Invoke();
                }
            }
        }

        private static Dictionary<string, int> GetIntPrefab()
        {
            Dictionary<string, int> values = new Dictionary<string, int>();

            foreach (var i in intPrefs)
            {
                if (PlayerPrefs.HasKey(i.Key))
                    values.Add(i.Key, PlayerPrefs.GetInt(i.Key));
            }
            return values;
        }


        private static Dictionary<string, float> GetFloatPrefab()
        {
            Dictionary<string, float> values = new Dictionary<string, float>();

            foreach (var i in floatPrefs)
            {
                if (PlayerPrefs.HasKey(i.Key))
                    values.Add(i.Key, PlayerPrefs.GetFloat(i.Key));
            }
            return values;
        }


        private static Dictionary<string, string> GetStringPrefab()
        {
            Dictionary<string, string> values = new Dictionary<string, string>();

            foreach (var i in stringPrefs)
            {
                if (PlayerPrefs.HasKey(i.Key))
                    values.Add(i.Key, PlayerPrefs.GetString(i.Key));
            }
            return values;
        }

        #endregion
    }
}