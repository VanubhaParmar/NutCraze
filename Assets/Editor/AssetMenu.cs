using com.tag.nut_sort;
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace com.tag.editor
{
    public class AssetMenu
    {
        private static bool isPortrait = true;
        private static Vector2 landscapeSize = new Vector2(1920, 1080);
        private static Vector2 portraitSize = new Vector2(1080, 1920);

        [MenuItem(Constant.GAME_NAME + "/PlayerPrefabs/Load Data")]
        public static void LoadSaveDataToPrefabs()
        {
            string filePath = EditorUtility.OpenFilePanel("Player prefabs", Application.persistentDataPath, "txt");
            PlayerPrefbsHelper.SaveDataToPrefabs(filePath);
        }

        [MenuItem(Constant.GAME_NAME + "/PlayerPrefabs/Save Data #%&s")]
        public static void SaveData()
        {
            if (Application.isPlaying)
                PlayerPrefbsHelper.SaveDataInFile();
        }

        [MenuItem(Constant.GAME_NAME + "/Clear Player Prefs")]
        static void ClearData()
        {
            PlayerPrefs.DeleteAll();
        }

        [MenuItem("Scene/Load Loading Scene _F1", false, 1000)]
        static void LoadIntroScene()
        {
            isPortrait = true;
            //ChangeGameViewOrientation();
            EditorSceneManager.OpenScene("Assets/MainGame/Scenes/Main/Loading.unity");
            SetGameViewPortraitOrLandscape();
        }

        [MenuItem("Scene/Load Editor Scene _F4", false, 2000)]
        static void LoadLevelEditorScene()
        {
            isPortrait = false;
            //ChangeGameViewOrientation();
            EditorSceneManager.OpenScene("Assets/Editor/Level Editor/Scenes/LevelEditor.unity");
            SetGameViewPortraitOrLandscape();
        }

        [MenuItem("Scene/Load Main Scene _F2", false, 1001)]
        static void LoadMainScene()
        {
            isPortrait = true;
            EditorSceneManager.OpenScene("Assets/MainGame/Scenes/Main/MainScene.unity");
            SetGameViewPortraitOrLandscape();
        }

        //[MenuItem("Scene/Load GamePlay Scene %g")]
        //static void LoadGamePlayScene()
        //{
        //    EditorSceneManager.OpenScene("Assets/MainGame/Scenes/GamePlayScene.unity");
        //}

        [MenuItem("Utilities/TakeScreenShot")]
        public static void TakeScreenShot()
        {
            string screenshotName = "Screenshot_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";

            string filePath = EditorUtility.SaveFilePanel(Application.persistentDataPath, "", screenshotName, "png");

            Debug.Log("Screenshot saved: " + Application.dataPath);

            ScreenCapture.CaptureScreenshot(filePath);
        }

        public static void SetGameViewPortraitOrLandscape()
        {
            SetSize(isPortrait ? 5 : 6);
        }

        public static void SetSize(int index)
        {
            var gvWndType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
            var selectedSizeIndexProp = gvWndType.GetProperty("selectedSizeIndex",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var gvWnd = EditorWindow.GetWindow(gvWndType);
            selectedSizeIndexProp.SetValue(gvWnd, index, null);
        }

        private static void ChangeGameViewOrientation()
        {
            EditorWindow gameView = GetMainGameView();
            if (gameView == null)
            {
                Debug.LogError("Game View not found!");
                return;
            }

            Type gameViewType = gameView.GetType();
            PropertyInfo currentGameViewSizeProp = gameViewType.GetProperty("currentGameViewSize", BindingFlags.Instance | BindingFlags.NonPublic);

            if (currentGameViewSizeProp != null)
            {
                object sizeObj = currentGameViewSizeProp.GetValue(gameView);
                Type sizeType = sizeObj.GetType();

                PropertyInfo widthProp = sizeType.GetProperty("width");
                PropertyInfo heightProp = sizeType.GetProperty("height");

                if (widthProp != null && heightProp != null)
                {
                    Vector2 newSize = isPortrait ? portraitSize : landscapeSize;
                    widthProp.SetValue(sizeObj, (int)newSize.x);
                    heightProp.SetValue(sizeObj, (int)newSize.y);

                    //currentGameViewSizeProp.SetValue(gameView, sizeObj);
                    gameView.Repaint();
                    Debug.Log("Game View orientation changed successfully.");
                }
                else
                {
                    Debug.LogError("Failed to find width or height property on GameViewSize");
                }
            }
            else
            {
                Debug.LogError("Failed to find currentGameViewSize property on GameView");
            }
        }

        private static EditorWindow GetMainGameView()
        {
            var assembly = Assembly.GetAssembly(typeof(EditorWindow));
            Type gameViewType = assembly.GetType("UnityEditor.GameView");
            if (gameViewType == null)
            {
                Debug.LogError("Failed to find GameView type");
                return null;
            }

            var getMainGameViewMethod = gameViewType.GetMethod("GetMainGameView", BindingFlags.NonPublic | BindingFlags.Static);
            if (getMainGameViewMethod != null)
            {
                return getMainGameViewMethod.Invoke(null, null) as EditorWindow;
            }

            // If GetMainGameView doesn't exist, fall back to finding any GameView
            return EditorWindow.GetWindow(gameViewType);
        }
    }
}
