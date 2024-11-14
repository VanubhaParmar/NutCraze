using System;
using System.Globalization;
using UnityEngine;

namespace Tag.NutSort
{
    public class CustomTime
    {
        #region PUBLIC_VARS

        //public static DateTime startTime = new DateTime();
        //public static long startTick;

        #endregion

        #region PRIVATE_VARS

        //private static string userTimeZoneOffset = "UserTimeZoneOffset";
        //private static PersistantVariable<TimeSpan> UserTimeZoneOffSet = new(userTimeZoneOffset, new TimeSpan(-1));

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        //public static void SetTickTime()
        //{
        //    startTick = GetCurrentSystemTickTime();
        //}

        public static DateTime GetCurrentUtcTime()
        {
            //if (!Constants.IS_CUSTOME_TIME_ACTIVE)
                return DateTime.UtcNow;
            //try
            //{
            //    DateTime utcTime = startTime.AddSeconds(GetCurrentSystemTickTime() - startTick);
            //    return utcTime;
            //}
            //catch (Exception e)
            //{
            //    Debug.LogError("CustomTime GetCurrentUtcTime : " + e);
            //    return DateTime.UtcNow;
            //}
        }

        public static DateTime GetCurrentTime()
        {
            //if (!Constants.IS_CUSTOME_TIME_ACTIVE)
            TimeDuration extraTime = TimeManager.Instance == null ? new TimeDuration(0) : TimeManager.Instance.timeMachine;
            return DateTime.Now.AddTimeDuration(extraTime);
            //try
            //{
            //    DateTime utcTime = GetCurrentUtcTime();
            //    if (UserTimeZoneOffSet.Value.Ticks == -1)
            //        UserTimeZoneOffSet.Value = TimeZoneInfo.Local.BaseUtcOffset;
            //    DateTime localTime = utcTime.Add(UserTimeZoneOffSet.Value);
            //    return localTime;
            //}
            //catch (Exception e)
            //{
            //    Debug.LogError("CustomTime GetCurrentTime : " + e);
            //    return DateTime.Now;
            //}
        }


        public static bool TryParseDateTime(string dateTime, out DateTime returnedDateTime)
        {
            return DateTime.TryParse(dateTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out returnedDateTime);
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        private static long GetCurrentSystemTickTime()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return EnergySuite.iOSBridge.GetCurrentMediaTime();
#elif UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaObject jo = new AndroidJavaObject("android.os.SystemClock");
            long time = jo.CallStatic<long>("elapsedRealtime");
            long timeSec = time / 1000;
            return timeSec;
#elif UNITY_STANDALONE || UNITY_EDITOR
            int time = Environment.TickCount;
            if (time < 0)
                time = int.MaxValue + Environment.TickCount;
            int timeSec = time / 1000;
            return timeSec;
#else
            return -1;
#endif
        }

        #endregion
    }
}
