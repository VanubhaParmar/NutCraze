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
                    return (T)(object)PlayerPrefbsHelper.GetInt(_key, (int)(object)defaultValue);
                if (IsFloat(t))
                    return (T)(object)PlayerPrefbsHelper.GetFloat(_key, (float)(object)defaultValue);
                if (IsString(t))
                    return (T)(object)PlayerPrefbsHelper.GetString(_key, (string)(object)this.defaultValue);
                return !HasKey(_key) && defaultValue != null ? defaultValue : SerializeUtility.DeserializeObject<T>(PlayerPrefbsHelper.GetString(_key));
            }
            set
            {
                Type t = typeof(T);
                if (IsInt(t))
                {
                    PlayerPrefbsHelper.SetInt(_key, (int)(object)value);
                }
                else if (IsFloat(t))
                {
                    PlayerPrefbsHelper.SetFloat(_key, (float)(object)value);
                }
                else if (IsString(t))
                {
                    PlayerPrefbsHelper.SetString(_key, (string)(object)value);
                }
                else
                {
                    PlayerPrefbsHelper.SetString(_key, SerializeUtility.SerializeObject(value));
                }
            }
        }

        public string RawValue
        {
            get
            {
                Type t = typeof(T);
                if (IsInt(t))
                    return PlayerPrefbsHelper.GetInt(_key, (int)(object)defaultValue).ToString();
                if (IsFloat(t))
                    return PlayerPrefbsHelper.GetFloat(_key, (float)(object)defaultValue).ToString();
                if (IsString(t))
                    return PlayerPrefbsHelper.GetString(_key, (string)(object)this.defaultValue);

                return !HasKey(_key) && defaultValue != null ? SerializeUtility.SerializeObject(defaultValue) : PlayerPrefbsHelper.GetString(_key);
            }
        }

        public bool IsInt(Type t) => t == intType;

        public bool IsFloat(Type t) => t == floatType;

        public bool IsString(Type t) => t == stringType;

        public static bool HasKey(string key) => PlayerPrefbsHelper.HasKey(key);
    }
}