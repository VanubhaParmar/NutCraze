using System;
using System.Collections.Generic;

namespace Tag.NutSort
{
    public static class Extension
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
    }
}