using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Tag.NutSort
{
    public static class ExtensionClass
    {
        public static string FormateTimeSpan(this TimeSpan timeSpan)
        {
            if (timeSpan.TotalMinutes >= 60)
            {
                return String.Format("{0:D2}:{1:D2}:{2:D2}", (int)timeSpan.TotalHours, timeSpan.Minutes, timeSpan.Seconds);
            }
            return String.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        }

        public static string FormateTimeSpanForDays(this TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours >= 24)
            {
                return String.Format("{0:D2}d:{1:D2}h:{2:D2}m", (int)timeSpan.Days, timeSpan.Hours, timeSpan.Minutes);
            }

            if (timeSpan.TotalMinutes >= 60)
            {
                return String.Format("{0:D2}:{1:D2}:{2:D2}", (int)timeSpan.TotalHours, timeSpan.Minutes, timeSpan.Seconds);
            }
            return String.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        }

        public static string FormateTimeSpanWithoutColon(this TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours > 24)
            {
                return String.Format("{0:D2}d {1:D2}h {2:D2}m", (int)timeSpan.Days, timeSpan.Hours, timeSpan.Minutes);
            }

            if (timeSpan.TotalMinutes >= 60)
            {
                return String.Format("{0:D2}h {1:D2}m {2:D2}s", (int)timeSpan.TotalHours, timeSpan.Minutes, timeSpan.Seconds);
            }
            return String.Format("{0:D2}m {1:D2}s", timeSpan.Minutes, timeSpan.Seconds);
        }

        public static string FormateTimeSpanForDaysInTwoDigit(this TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours > 24)
            {
                return String.Format("{0:D2}d:{1:D2}h", timeSpan.Days, timeSpan.Hours);
            }
            if (timeSpan.TotalHours < 24 && timeSpan.TotalMinutes >= 60)
            {
                return String.Format("{0:D2}h:{1:D2}m", timeSpan.Hours, timeSpan.Minutes);
            }
            if (timeSpan.TotalHours == 24)
            {
                return String.Format("{0:D2}h:{1:D2}m", 24, timeSpan.Minutes);
            }
            if (timeSpan.TotalSeconds < 0)
            {
                return String.Format("{0:D2}:{1:D2}", 0, 0);
            }
            return String.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        }

        public static string FormateTimeSpanForDaysInThreeDigit(this TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours > 24)
            {
                return String.Format("{0:D2}d:{1:D2}h:{2:D2}m", timeSpan.Days, timeSpan.Hours, timeSpan.Minutes);
            }
            if (timeSpan.TotalHours < 24 && timeSpan.TotalMinutes >= 60)
            {
                return String.Format("{0:D2}h:{1:D2}m", timeSpan.Hours, timeSpan.Minutes);
            }
            if (timeSpan.TotalHours == 24)
            {
                return String.Format("{0:D2}h:{1:D2}m", 24, timeSpan.Minutes);
            }
            if (timeSpan.TotalSeconds < 0)
            {
                return String.Format("{0:D2}:{1:D2}", 0, 0);
            }
            return String.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        }

        public static string GetIapPriceText(this KeyValuePair<string, string> price)
        {
            try
            {
                double doublevalue = double.Parse(price.Value);
                float prize = (float)Math.Round(doublevalue, 2);
                return price.Key + " " + prize;
            }
            catch
            {
                return price.Key + " " + price.Value;
            }
        }

        public static float ConvertByteToMegaByte(this long bytes)
        {
            return ((float)(bytes / 1024)) / 1024;
        }
        public static float ConvertByteToMegaByte(this float bytes)
        {
            return ((float)(bytes / 1024)) / 1024;
        }

        public static Vector3 Clamp(this Vector3 value, Vector3 min, Vector3 max)
        {
            value.x = Mathf.Clamp(value.x, min.x, max.x);
            value.y = Mathf.Clamp(value.y, min.y, max.y);
            value.z = Mathf.Clamp(value.z, min.z, max.z);
            return value;
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
}