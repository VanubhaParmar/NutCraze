﻿using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Tag.NutSort
{
    public static class ExtensionClass
    {
        public static Vector3 Clamp(this Vector3 value, Vector3 min, Vector3 max)
        {
            value.x = Mathf.Clamp(value.x, min.x, max.x);
            value.y = Mathf.Clamp(value.y, min.y, max.y);
            value.z = Mathf.Clamp(value.z, min.z, max.z);
            return value;
        }

        public static CompareLevelResult CompareLevel(this Level level, Level compareLevel)
        {
            int n1 = level.world * 1000000 + level.area * 1000 + level.level;
            int n2 = compareLevel.world * 1000000 + compareLevel.area * 1000 + compareLevel.level;
            if (n1 > n2)
                return CompareLevelResult.Grater;
            if (n1 < n2)
                return CompareLevelResult.Lesser;
            return CompareLevelResult.Equal;
        }

        public static float GetAnimationLength(this Animator animator, string clipName)
        {
            RuntimeAnimatorController cont = animator.runtimeAnimatorController;
            for (int i = 0; i < cont.animationClips.Length; i++)
            {
                if (cont.animationClips[i].name == clipName)
                    return cont.animationClips[i].length;
            }
            return 0;
        }

        public static T GetConverted<T>(this Dictionary<string, object> dic, string key, T defaultValue)
        {
            if (dic == null)
                return defaultValue;

            if (dic.ContainsKey(key) && dic[key] != null)
            {
                return (T)Convert.ChangeType(dic[key], typeof(T));
            }

            return defaultValue;
        }

        public static T GetJObjectCast<T>(this Dictionary<string, object> dic, string key, T defaultValue)
        {
            if (dic == null)
                return defaultValue;
            if (dic.ContainsKey(key) && dic[key] != null)
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(dic[key].ToString());
                }
                catch (Exception)
                {
                    return (T)Convert.ChangeType(dic[key], typeof(T));
                }
            }

            return defaultValue;
        }

        public static T GetJObjectCast<T>(this object dic)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(dic.ToString());
            }
            catch (Exception)
            {
                return (T)Convert.ChangeType(dic, typeof(T));
            }
        }

        public static bool ContainsItem(this Dictionary<string, object> dic, string key)
        {
            return dic != null && dic.ContainsKey(key);
        }

        public static int GetGemsCount(this TimeSpan timeSpan)
        {
            if (timeSpan.TotalMinutes <= 60)
                return Mathf.CeilToInt((float)(timeSpan.TotalSeconds * 0.01067f));
            timeSpan = timeSpan.Subtract(new TimeSpan(1, 0, 0));
            return 39 + Mathf.CeilToInt((float)(timeSpan.TotalSeconds * 0.0072f));
        }

        public static string GetEventTitle(this string notificationIntentData)
        {
            return Regex.Replace(notificationIntentData, "[^a-zA-Z0-9_. ]+", "", RegexOptions.Compiled);
        }

        public static int GetUTF8ByteSize(this string s)
        {
            return Encoding.UTF8.GetByteCount(s);
        }


    }

    public enum CompareLevelResult
    {
        Equal,
        Lesser,
        Grater
    }

    public class Level
    {
        public int world;
        public int area;
        public int level;

        public Level()
        {
            world = 1;
            area = 1;
            level = 1;
        }

        public Level(Level level)
        {
            this.world = level.world;
            this.area = level.area;
            this.level = level.level;
        }

        public Level(int world, int area, int level)
        {
            this.world = world;
            this.area = area;
            this.level = level;
        }

        public void Copy(Level level)
        {
            this.world = level.world;
            this.area = level.area;
            this.level = level.level;
        }

        public Level Clone()
        {
            Level l = new Level();
            l.world = world;
            l.area = area;
            l.level = level;
            return l;
        }
    }
}