using UnityEngine;
using CandyCoded.HapticFeedback.Android;

namespace Tag.NutSort
{
    public static class Vibrator
    {
        public static bool IsVibrateOn
        {
            get => VibrateState;
            set => VibrateState = value;
        }

        private static bool VibrateState { get { return PlayerPrefs.GetInt(Vibrate_Prefs_key, 1) == 1; } set { PlayerPrefs.SetInt(Vibrate_Prefs_key, value ? 1 : 0); } }
        private const string Vibrate_Prefs_key = "VibratePlayerPref";


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
            if (!IsVibrateOn)
                return;

#if UNITY_ANDROID && !UNITY_EDITOR
                HapticFeedback.PerformHapticFeedback((HapticFeedbackConstants)intensity);
#endif
        }
    }
}