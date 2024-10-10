using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using Scene = UnityEngine.SceneManagement.SceneManager;

namespace Tag.NutSort.LevelEditor
{
    public class LevelEditorManager : SerializedManager<LevelEditorManager>
    {
        #region PUBLIC_VARIABLES
        public Transform mainEditorParent;
        public RectTransform mainEditorUIParent;
        public float gameWidth = 0.4f;
        public int targetGameWindowResolution;
        #endregion

        #region PRIVATE_VARIABLES
        private int targetLevel;
        private LevelDataSO targetLevelDataSO;

        private Coroutine levelEditorLoadCoroutine;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        public override void Awake()
        {
            base.Awake();
            InitializeLevelEditorManager();
        }
        #endregion

        #region PUBLIC_METHODS
        public void InitializeLevelEditorManager()
        {
            StartCoroutine(LevelEditorManagerLoadCoroutine());
        }

        public bool DoesLevelExist(int level)
        {
            return GetLevelDataSOOfLevel(level) != null;
        }

        public LevelDataSO GetLevelDataSOOfLevel(int level)
        {
            LevelDataSO loadedLevel = Utility.LoadResourceAsset<LevelDataSO>(string.Format(ResourcesConstants.LEVELS_PATH + ResourcesConstants.LEVEL_SO_NAME_FORMAT, level));
            return loadedLevel;
        }

        public int GetTotalNumberOfLevels()
        {
            string directoryPath = Application.dataPath + ResourcesConstants.MAIN_RESOURCE_PATH_FROM_PERSISTANT_PATH + ResourcesConstants.LEVELS_PATH;
            string[] files = System.IO.Directory.GetFiles(directoryPath);
            string lastFileName = files.Length > 0 ? files[files.Length - 1] : "";

            int levelNumber = 0;

            if (string.IsNullOrEmpty(lastFileName))
                return 0;

            lastFileName = lastFileName.Split("/").ToList().GetLastItemFromList().Remove(".asset").Remove(".meta");

            string finalSub = lastFileName.Substring(ResourcesConstants.LEVEL_SO_NAME_FORMAT.IndexOf("{0}"));
            if (!int.TryParse(finalSub, out levelNumber))
                Debug.LogError("Level Number Could Not Be Parsed... Please Check Format ! " + finalSub);

            return levelNumber;
        }

        public void SetSize(int index)
        {
#if UNITY_EDITOR
            var gvWndType = typeof(UnityEditor.EditorWindow).Assembly.GetType("UnityEditor.GameView");
            var selectedSizeIndexProp = gvWndType.GetProperty("selectedSizeIndex",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var gvWnd = UnityEditor.EditorWindow.GetWindow(gvWndType);
            selectedSizeIndexProp.SetValue(gvWnd, index, null);
#endif
        }

        public void LoadEditor(int targetLevel)
        {
            this.targetLevel = targetLevel;
            targetLevelDataSO = GetLevelDataSOOfLevel(targetLevel);

            if (levelEditorLoadCoroutine == null)
                levelEditorLoadCoroutine = StartCoroutine(LevelEditorLoadCoroutine());
        }
        #endregion

        #region PRIVATE_METHODS
        private void OnLevelEditorInitialized()
        {
            LevelEditorUIManager.Instance.GetView<LevelEditorIntroView>().Show();
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        IEnumerator LevelEditorManagerLoadCoroutine()
        {
            SetSize(targetGameWindowResolution);
            yield return null;
            DontDestroyOnLoad(mainEditorParent.gameObject);
            OnLevelEditorInitialized();
        }

        IEnumerator LevelEditorLoadCoroutine()
        {
            LevelEditorUIManager.Instance.GetView<LevelEditorLoadingView>().Show();
            yield return LoadSceneCoroutine(SceneType.Loading.ToString());

            while (GameplayManager.Instance == null || GameplayManager.Instance.GameplayStateData.gameplayStateType != GameplayStateType.PLAYING_LEVEL)
            {
                yield return null;
            }

            LevelEditorUIManager.Instance.GetView<LevelEditorLoadingView>().Hide();
            LevelEditorUIManager.Instance.GetView<LevelEditorMainEditView>().Show();

            mainEditorUIParent.anchorMin = new Vector2(gameWidth, 0f);
            List<Camera> allCameras = FindObjectsOfType<Camera>(true).ToList();
            for (int i = 0; i < allCameras.Count; i++)
            {
                allCameras[i].rect = new Rect(0f, 0f, gameWidth, 1f);
            }

            levelEditorLoadCoroutine = null;
        }

        IEnumerator LoadSceneCoroutine(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            AsyncOperation asyncOperation = Scene.LoadSceneAsync(sceneName, loadSceneMode);

            while (!asyncOperation.isDone)
            {
                yield return null;
            }

            Resources.UnloadUnusedAssets();
        }
        #endregion

        #region UI_CALLBACKS
        #endregion
    }

    public class LevelEditorConstants
    {
        public static string Level_Editor_Temp_Folder_Path = Application.dataPath + Level_Editor_Temp_Folder_Raw_Path;
        public static string Level_Editor_Temp_Folder_Relative_Path = "Assets" + Level_Editor_Temp_Folder_Raw_Path;

        public const string Level_Editor_Temp_Folder_Raw_Path = "/Level Editor/Data/Temp/";
    }
}