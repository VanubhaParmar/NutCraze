using UnityEngine;
using CandyCoded.HapticFeedback.Android;

namespace Tag.NutSort
{
    public static class Vibrator
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    public static AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    public static AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    public static AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
#else
        public static AndroidJavaClass unityPlayer;
        public static AndroidJavaObject currentActivity;
        public static AndroidJavaObject vibrator;
#endif 
        public static int hugeIntensity = 6;
        public static int smallIntensity = 1;
        public static int averageIntensity = 3;

        public static void Vibrate(int intensity = 1)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
                HapticFeedback.PerformHapticFeedback((HapticFeedbackConstants)intensity);
#endif
        }
    }
}