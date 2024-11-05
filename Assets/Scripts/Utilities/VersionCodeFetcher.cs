using UnityEngine;

namespace Tag.NutSort
{
    public static class VersionCodeFetcher
    {
        public static int GetBundleVersionCode()
        {
            int versionCode = -1;

#if UNITY_EDITOR
            versionCode = UnityEditor.PlayerSettings.Android.bundleVersionCode;
#elif UNITY_ANDROID
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject packageManager = context.Call<AndroidJavaObject>("getPackageManager");
                string packageName = context.Call<string>("getPackageName");
                AndroidJavaObject packageInfo = packageManager.Call<AndroidJavaObject>("getPackageInfo", packageName, 0);
                versionCode = packageInfo.Get<int>("versionCode");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to get Android bundle version code: " + e.Message);
        }
        
#elif UNITY_IOS
        try
        {
            // iOS bundle version as a string (e.g., "1.0.0")
            string bundleVersion = GetIOSBundleVersion();
            // You can parse it to int if needed or just use it as a string
            versionCode = int.Parse(bundleVersion.Replace(".", ""));
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to get iOS bundle version: " + e.Message);
        }
#endif

            return versionCode;
        }

        private static string GetIOSBundleVersion()
        {
#if UNITY_IOS
            return UnityEngine.iOS.Device.systemVersion; // Retrieves version in iOS format
#else
            return Application.version;
#endif
        }
    }
}