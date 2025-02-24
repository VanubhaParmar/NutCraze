using UnityEngine;

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
        private const string Vibrate_Prefs_key = "VibratePlayerPref";

        public static bool IsVibrateOn
        {
            get => VibrateState;
            set => VibrateState = value;
        }

        private static bool VibrateState
        {
            get { return PlayerPrefbsHelper.GetInt(Vibrate_Prefs_key, 1) == 1; }
            set { PlayerPrefbsHelper.SetInt(Vibrate_Prefs_key, value ? 1 : 0); }
        }


        public static void Vibrate(VibrateIntensity feedbackType = VibrateIntensity.Light)
        {
            if (!IsVibrateOn)
                return;

            if (IsAndroid())
                vibrator.Call("vibrate", (long)(int)feedbackType);

        }

        public static void LightFeedback()
        {
            Vibrate(VibrateIntensity.Light);
        }

        public static void MediumFeedback()
        {
            Vibrate(VibrateIntensity.Medium);
        }

        public static void HeavyFeedback()
        {
            Vibrate(VibrateIntensity.Heavy);
        }

        public static void Cancel()
        {
            if (!IsVibrateOn)
                return;

            if (IsAndroid())
                vibrator.Call("cancel");
        }

        public static bool IsAndroid()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
                return true;
#else
            return false;
#endif
        }
    }

    public enum VibrateIntensity
    {
        Light = 40,
        Medium = 50,
        Heavy = 60,
    }
}