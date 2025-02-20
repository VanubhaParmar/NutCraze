using UnityEngine;
using Application = UnityEngine.Device.Application;
using Screen = UnityEngine.Device.Screen;

namespace Tag.NutSort
{
    public static class SafeAreaUtility
    {
        public static Rect GetSafeArea()
        {
#if UNITY_EDITOR
            if (Application.isMobilePlatform)
            {
                return Screen.safeArea;
            }
            else
            {
                System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
                var getSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                var resolution = getSizeOfMainGameView.Invoke(null, null);
                return new Rect(0, 0, ((Vector2)resolution).x, ((Vector2)resolution).y);
            }
#else
                return Screen.safeArea;
#endif
        }

        public static Vector2 GetResolution()
        {
#if UNITY_EDITOR
            if (Application.isMobilePlatform)
            {
                return new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
            }
            else
            {
                System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
                var getSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                var resolution = getSizeOfMainGameView.Invoke(null, null);
                return (Vector2)resolution;
            }
#else
                return new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
#endif
        }
    }
}