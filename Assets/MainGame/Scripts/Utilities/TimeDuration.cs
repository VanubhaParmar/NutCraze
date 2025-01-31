using System;

namespace com.tag.nut_sort {
    [System.Serializable]
    public class TimeDuration
    {
        public int hours;
        public int minutes;
        public int seconds;

        public TimeDuration() { }
        public TimeDuration(int seconds, int minutes = 0, int hours = 0)
        {
            this.seconds = seconds;
            this.minutes = minutes;
            this.hours = hours;
        }

        public double GetTotalSeconds()
        {
            return (hours * 3600) + (minutes * 60) + seconds;
        }

        public TimeSpan GetTotalTimeSpan()
        {
            return new TimeSpan(hours, minutes, seconds);
        }

        public string GetTimeDurationString(string stringFormat = "")
        {
            if (string.IsNullOrEmpty(stringFormat))
            {
                if (hours == 0 && minutes == 0)
                    return seconds + "s";
                else if (hours == 0)
                    return string.Format("{1}m {0}s", seconds, minutes);
                else
                    return string.Format("{1}h {0}m", minutes, hours);
            }

            return string.Format(stringFormat, seconds, minutes, hours);
        }
    }
}