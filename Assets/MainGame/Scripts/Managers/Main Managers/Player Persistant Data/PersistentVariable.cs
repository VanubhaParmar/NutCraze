using System;
using UnityEngine;

namespace Tag.NutSort {
    public class PersistentVariable<T>
    {
        public readonly string _key = string.Empty;

        private static readonly Type IntType = typeof(int);
        private static readonly Type FloatType = typeof(float);
        private static readonly Type StringType = typeof(string);

        private readonly bool isInt;
        private readonly bool isFloat;
        private readonly bool isString;

        private T defaultValue;

        public PersistentVariable(string key)
        {
            _key = key;
            Type t = typeof(T);
            isInt = t == IntType;
            isFloat = t == FloatType;
            isString = t == StringType;
        }

        public PersistentVariable(string key, T defaultValue) : this(key)
        {
            this.defaultValue = defaultValue;
        }

        public T Value
        {
            get
            {
                if (isInt)
                    return (T)(object)PlayerPrefbsHelper.GetInt(_key, (int)(object)defaultValue);
                if (isFloat)
                    return (T)(object)PlayerPrefbsHelper.GetFloat(_key, (float)(object)defaultValue);
                if (isString)
                    return (T)(object)PlayerPrefbsHelper.GetString(_key, (string)(object)this.defaultValue);
                return !HasKey(_key) && defaultValue != null ? defaultValue : SerializeUtility.DeserializeObject<T>(PlayerPrefbsHelper.GetString(_key));
            }
            set
            {
                if (isInt)
                    PlayerPrefbsHelper.SetInt(_key, (int)(object)value);
                else if (isFloat)
                    PlayerPrefbsHelper.SetFloat(_key, (float)(object)value);
                else if (isString)
                    PlayerPrefbsHelper.SetString(_key, (string)(object)value);
                else
                    PlayerPrefbsHelper.SetString(_key, SerializeUtility.SerializeObject(value));
            }
        }

        public string RawValue
        {
            get
            {
                if (isInt)
                    return PlayerPrefbsHelper.GetInt(_key, (int)(object)defaultValue).ToString();
                if (isFloat)
                    return PlayerPrefbsHelper.GetFloat(_key, (float)(object)defaultValue).ToString();
                if (isString)
                    return PlayerPrefbsHelper.GetString(_key, (string)(object)this.defaultValue);

                return !HasKey(_key) && defaultValue != null ? SerializeUtility.SerializeObject(defaultValue) : PlayerPrefbsHelper.GetString(_key);
            }
        }
        public static bool HasKey(string key) => PlayerPrefbsHelper.HasKey(key);
    }
}