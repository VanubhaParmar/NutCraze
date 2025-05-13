using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Tag.NutSort.Editor
{
    public class AssetMenu
    {
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
            EditorSceneManager.OpenScene("Assets/Scenes/Main/Loading.unity");
        }

        [MenuItem("Scene/Load Editor Scene _F4", false, 2000)]
        static void LoadLevelEditorScene()
        {
            EditorSceneManager.OpenScene("Assets/Level Editor/Scenes/LevelEditor.unity");
        }

        [MenuItem("Scene/Load Main Scene _F2", false, 1001)]
        static void LoadMainScene()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Main/MainScene.unity");
        }

        [MenuItem("Utilities/TakeScreenShot")]
        public static void TakeScreenShot()
        {
            string screenshotName = "Screenshot_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";

            string filePath = EditorUtility.SaveFilePanel(Application.persistentDataPath, "", screenshotName, "png");

            Debug.Log("Screenshot saved: " + Application.dataPath);

            ScreenCapture.CaptureScreenshot(filePath);
        }
    }
}
