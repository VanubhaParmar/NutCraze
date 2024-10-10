using System;
using UnityEngine;

namespace Tag.NutSort
{
    public class PersistantVariable<T>
    {
        public readonly string _key = string.Empty;
        public static Type intType = typeof(int);
        public static Type floatType = typeof(float);
        public static Type stringType = typeof(string);
        private T defaultValue;

        public PersistantVariable(string key) => _key = key;

        public PersistantVariable(string key, T defaultValue)
        {
            this._key = key;
            this.defaultValue = defaultValue;
        }

        public T Value
        {
            get
            {
                Type t = typeof(T);
                if (IsInt(t))
                    return (T)(object)PlayerPrefs.GetInt(_key, (int)(object)defaultValue);
                if (IsFloat(t))
                    return (T)(object)PlayerPrefs.GetFloat(_key, (float)(object)defaultValue);
                if (IsString(t))
                    return (T)(object)PlayerPrefs.GetString(_key, (string)(object)this.defaultValue);
                return !HasKey(_key) && defaultValue != null ? defaultValue : SerializeUtility.DeserializeObject<T>(PlayerPrefs.GetString(_key));
            }
            set
            {
                Type t = typeof(T);
                if (IsInt(t))
                {
                    PlayerPrefs.SetInt(_key, (int)(object)value);
                }
                else if (IsFloat(t))
                {
                    PlayerPrefs.SetFloat(_key, (float)(object)value);
                }
                else if (IsString(t))
                {
                    PlayerPrefs.SetString(_key, (string)(object)value);
                }
                else
                {
                    PlayerPrefs.SetString(_key, SerializeUtility.SerializeObject(value));
                }
            }
        }

        public string RawValue
        {
            get
            {
                Type t = typeof(T);
                if (IsInt(t))
                    return PlayerPrefs.GetInt(_key, (int)(object)defaultValue).ToString();
                if (IsFloat(t))
                    return PlayerPrefs.GetFloat(_key, (float)(object)defaultValue).ToString();
                if (IsString(t))
                    return PlayerPrefs.GetString(_key, (string)(object)this.defaultValue);

                return !HasKey(_key) && defaultValue != null ? SerializeUtility.SerializeObject(defaultValue) : PlayerPrefs.GetString(_key);
            }
        }

        public bool IsInt(Type t) => t == intType;

        public bool IsFloat(Type t) => t == floatType;

        public bool IsString(Type t) => t == stringType;

        public static bool HasKey(string key) => PlayerPrefs.HasKey(key);
    }
}