using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Scene = UnityEngine.SceneManagement.SceneManager;

namespace Tag.NutSort.LevelEditor
{
    public class LevelEditorManager : SerializedManager<LevelEditorManager>
    {

        #region PRIVATE_VARIABLES
        [SerializeField] private ABTestType aBTestType = ABTestType.Default;
        [SerializeField] private Transform mainEditorParent;
        [SerializeField] private RectTransform mainEditorUIParent;
        [SerializeField] private float gameWidth = 0.4f;
        [SerializeField] private int targetGameWindowResolution;
        [SerializeField] private Vector2Int targetScreenResolution;
        [SerializeField] private float targetOrthoSize = 8f;
        [SerializeField] private SpriteRenderer screwObjectSelectorSR;
        [SerializeField] private LevelGridSetter levelGridSetter;
        [SerializeField] private LevelArrangementsListDataSO _levelArrangementsListDataSO;

        private static BaseIDMappingConfig levelArrangementIdMaaping;
        private static BaseIDMappingConfig screwTypeIdMapping;
        private static BaseIDMappingConfig nutTypeIdMapping;
        private static BaseIDMappingConfig colorIdMapping;
        private int targetLevel;
        private LevelType targetLevelType;
        [ShowInInspector] private static LevelData currentLevelData;
        [ShowInInspector] private static LevelStage currentLevelStage;
        private Coroutine levelEditorLoadCoroutine;
        private Coroutine levelEditorTestingModeCoroutine;
        private bool isTestingMode;
        private int currentSelectionScrewDataIndex = -1;

        #endregion

        #region PUBLIC_VARIABLES
        #endregion

        #region PROPERTIES
        public int TargetLevel => targetLevel;
        public static LevelData CurrentLevelData => currentLevelData;
        public static LevelStage CurrentLevelStage { get => currentLevelStage; set => currentLevelStage = value; }
        public LevelArrangementsListDataSO LevelArrangementsListDataSO => _levelArrangementsListDataSO;
        public LevelType TargetLevelType => targetLevelType;

        public static BaseIDMappingConfig LevelArrangementIdMaaping
        {
            get
            {
                if (levelArrangementIdMaaping == null)
                    levelArrangementIdMaaping = LevelEditorUtility.LoadAssetAtPath<BaseIDMappingConfig>(LevelEditorConstants.Level_Arrangement_Key_Mapping_Ids_Path);
                return levelArrangementIdMaaping;
            }
        }

        public static BaseIDMappingConfig ScrewTypeIdMapping
        {
            get
            {
                if (screwTypeIdMapping == null)
                    screwTypeIdMapping = LevelEditorUtility.LoadAssetAtPath<BaseIDMappingConfig>(LevelEditorConstants.Screw_Types_Key_Mapping_Ids_Path);
                return screwTypeIdMapping;
            }
        }

        public static BaseIDMappingConfig NutTypeIdMapping
        {
            get
            {
                if (nutTypeIdMapping == null)
                    nutTypeIdMapping = LevelEditorUtility.LoadAssetAtPath<BaseIDMappingConfig>(LevelEditorConstants.Nut_Types_Key_Mapping_Ids_Path);
                return nutTypeIdMapping;
            }
        }

        public static BaseIDMappingConfig ColorIdMapping
        {
            get
            {
                if (colorIdMapping == null)
                    colorIdMapping = LevelEditorUtility.LoadAssetAtPath<BaseIDMappingConfig>(LevelEditorConstants.NutColor_Types_Key_Mapping_Ids_Path);
                return colorIdMapping;
            }
        }

        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        public override void Awake()
        {
            base.Awake();
            InitializeLevelEditorManager();
        }
        public override void OnDestroy()
        {
            if (LevelManager.Instance)
                LevelManager.Instance.DeRegisterOnLevelUnload(Main_StopTestingMode);
            base.OnDestroy();
        }
        #endregion

        #region PUBLIC_METHODS
        public LevelArrangementConfigDataSO GetArrangementConfigDataSO(int arrangementID)
        {
            return _levelArrangementsListDataSO.GetLevelArrangementConfig(arrangementID);
        }

        public LevelArrangementConfigDataSO GetCurrentArrangementConfig()
        {
            return null;
            //return GetArrangementConfigDataSO(currentLevelStage.arrangementId);
        }

        public void ShowGrid(LevelArrangementConfigDataSO so)
        {
            levelGridSetter.ShowGrid(so);
        }

        public void ShowCurrentStageGrid()
        {
            ShowGrid(GetCurrentArrangementConfig());
        }

        public void ResetGrid()
        {
            levelGridSetter.ResetGrid();
        }

        public void InitializeLevelEditorManager()
        {
            StartCoroutine(LevelEditorManagerLoadCoroutine());
        }

        public bool DoesLevelExist(int level, LevelType levelType = LevelType.NORMAL_LEVEL)
        {
            return GetLevelData(level, levelType) != null;
        }

        public LevelData GetLevelData(int level, LevelType levelType = LevelType.NORMAL_LEVEL)
        {
            return ProtoLevelDataFactory.GetLevelData(aBTestType, levelType, level);
        }

        public int GetTotalLevelCount(LevelType levelType = LevelType.NORMAL_LEVEL)
        {
            return ProtoLevelDataFactory.GetTotalLevelCount(aBTestType, levelType);
        }

        public void SetGameViewSize()
        {
#if UNITY_EDITOR
            int targetGameViewSize = 3; // For Android its 3, check enum GameViewSizeGroupType

            // Use reflection to call the SetSize method from GameViewUtils
            var assembly = System.Reflection.Assembly.Load("Assembly-CSharp-Editor");
            Type myType = assembly.GetType("Tag.NutSort.Editor.GameViewUtils");

            if (myType == null)
                return;

            var methodInfo = myType.GetMethod("SetOrAddSize", new Type[] { typeof(int), typeof(int), typeof(string), typeof(int) });
            if (methodInfo != null)
                methodInfo.Invoke(null, new object[] { targetScreenResolution.x, targetScreenResolution.y, "Level Editor", targetGameViewSize });
#endif
        }

        public void ChangeTargetLevelType(LevelType levelType)
        {
            targetLevelType = levelType;
        }

        public void LoadEditor_WithCreateNewLevel(int targetLevelCount = -1)
        {
            LoadEditor_CreateNewLevel(targetLevelCount);

            if (levelEditorLoadCoroutine == null)
                levelEditorLoadCoroutine = StartCoroutine(LevelEditorLoadCoroutine());
        }

        public void ReloadEditor_WithCreateNewLevel(int targetLevelCount = -1)
        {
            LoadEditor_CreateNewLevel(targetLevelCount);

            if (levelEditorLoadCoroutine == null)
                levelEditorLoadCoroutine = StartCoroutine(LevelEditorReloadCoroutine());
        }

        public void LoadEditor_WithDuplicateLevel(int targetLevel)
        {
            LoadEditor_MakeDuplicateLevel(targetLevel);

            if (levelEditorLoadCoroutine == null)
                levelEditorLoadCoroutine = StartCoroutine(LevelEditorLoadCoroutine());
        }

        public void ReloadEditor_WithDuplicateLevel(int targetLevel)
        {
            LoadEditor_MakeDuplicateLevel(targetLevel);

            if (levelEditorLoadCoroutine == null)
                levelEditorLoadCoroutine = StartCoroutine(LevelEditorReloadCoroutine());
        }

        public void LoadEditor(int targetLevel)
        {
            LoadEditor_LoadLevel(targetLevel);

            if (levelEditorLoadCoroutine == null)
                levelEditorLoadCoroutine = StartCoroutine(LevelEditorLoadCoroutine());
        }

        public void ReloadEditor(int targetLevel)
        {
            LoadEditor_LoadLevel(targetLevel);

            if (levelEditorLoadCoroutine == null)
                levelEditorLoadCoroutine = StartCoroutine(LevelEditorReloadCoroutine());
        }


        public void MakeNewLevelData(LevelData levelData)
        {
            currentLevelData = new LevelData(levelData);
        }

        public void RemoveLevelStage(int stageIndex)
        {
            if (currentLevelData.stages.Length > stageIndex)
            {
                currentLevelData.stages = currentLevelData.stages.Where((element, index) => index != stageIndex).ToArray();
            }
            LevelEditorUIManager.Instance.GetView<LevelDataEditView>().SetView();
        }

        public void AddNewStage()
        {
            if (currentLevelData.stages.Length > 0)
            {
                LevelStage lastStage = currentLevelData.stages.Last();
                LevelStage newStage = new LevelStage(lastStage);
                Array.Resize(ref currentLevelData.stages, currentLevelData.stages.Length + 1);
                currentLevelData.stages[currentLevelData.stages.Length - 1] = newStage;
            }
            else
            {
                currentLevelData.stages = new LevelStage[1];
                currentLevelData.stages[0] = new LevelStage();
            }
        }
        #endregion

        #region LEVEL_EDITOR_MAIN_METHODS
        public void Main_OnChangeLevelArrangementConfig(int arrangementId)
        {
            //List<LevelArrangementConfigDataSO> levelArrangementConfigDataSOs = _levelArrangementsListDataSO.LevelArrangementConfigDataSOs;
            //editLevelData.arrangementId = arrangementId;
            //LevelArrangementConfigDataSO levelArrangementConfigDataSO = GetCurrentArrangementConfig();
            //int targetScrewCapacity = levelArrangementConfigDataSO.arrangementCellIds.Count;

            //if (editLevelData.levelScrewDataInfos.Count > targetScrewCapacity)
            //{
            //    int removeExtraData = editLevelData.levelScrewDataInfos.Count - targetScrewCapacity;

            //    for (int i = 0; i < removeExtraData; i++)
            //    {
            //        editLevelData.levelScrewDataInfos.RemoveAt(editLevelData.levelScrewDataInfos.Count - 2); // Remove last most screw data before booster activated screw
            //    }
            //}
            //else if (editLevelData.levelScrewDataInfos.Count < targetScrewCapacity)
            //{
            //    int addExtraData = targetScrewCapacity - editLevelData.levelScrewDataInfos.Count;

            //    for (int i = 0; i < addExtraData; i++)
            //    {
            //        var duplicateData = editLevelData.levelScrewDataInfos.First().Clone();
            //        editLevelData.levelScrewDataInfos.Insert(editLevelData.levelScrewDataInfos.Count - 2, duplicateData); // Add screw data before booster activated screw
            //    }
            //}

            //List<int> removeNutsDataIndexes = new List<int>();
            //for (int i = 0; i < editLevelData.screwNutsLevelDataInfos.Count; i++) // Reset nuts data
            //{
            //    if (i < levelArrangementConfigDataSO.arrangementCellIds.Count - 1) // last is always booster activated screw so cant assign that
            //        editLevelData.screwNutsLevelDataInfos[i].targetScrewGridCellId = levelArrangementConfigDataSO.arrangementCellIds[i];
            //    else
            //        removeNutsDataIndexes.Add(i);
            //}
            //removeNutsDataIndexes.ForEach(x => editLevelData.screwNutsLevelDataInfos.RemoveAt(x));
            LevelBuilder_OnRegerateWholeLevel();
        }

        public void Main_OnScrewSelectedForEdit(int screwDataIndex)
        {
            currentSelectionScrewDataIndex = screwDataIndex;
            Main_OnResetScrewSelectedForEdit();
        }

        public void Main_OnResetScrewSelectedForEdit()
        {
            Main_ResetScrewSelection();
            LevelArrangementConfigDataSO levelArrangementConfigDataSO = GetCurrentArrangementConfig();
            if (currentSelectionScrewDataIndex < 0 || currentSelectionScrewDataIndex >= levelArrangementConfigDataSO.arrangementCellIds.Count)
                return;

            var screwCellId = levelArrangementConfigDataSO.arrangementCellIds[currentSelectionScrewDataIndex];

            var currentScrew = LevelManager.Instance.LevelScrews.Find(x => x.CellId == screwCellId);

            screwObjectSelectorSR.size = new Vector2(screwObjectSelectorSR.size.x, currentScrew.GetTotalScrewApproxHeight() + 1f);
            screwObjectSelectorSR.transform.position = currentScrew.transform.position + Vector3.down * 0.75f;
            screwObjectSelectorSR.gameObject.SetActive(true);
        }

        public void Main_ResetScrewSelection()
        {
            screwObjectSelectorSR.gameObject.SetActive(false);
        }

        public void Main_OnChangeScrewType(int screwDataIndex, int targetScrewType)
        {
            // editLevelData.levelScrewDataInfos[screwDataIndex].screwType = targetScrewType;

            LevelBuilder_OnRegerateWholeLevel();
            RaiseOnLevelEditorNutsDataCountChanged();
        }

        public void Main_OnChangeScrewCapacity(int screwDataIndex, int increaseValue, int addNewDataCount = 0)
        {
            //LevelArrangementConfigDataSO levelArrangementConfigDataSO = GetCurrentArrangementConfig();
            //var targetScrewData = editLevelData.levelScrewDataInfos[screwDataIndex];
            //var nutsData = editLevelData.screwNutsLevelDataInfos.Find(x => x.targetScrewGridCellId == levelArrangementConfigDataSO.arrangementCellIds[screwDataIndex]);

            //if (addNewDataCount > 0 && increaseValue == 0 && nutsData != null && nutsData.levelNutDataInfos.Count == targetScrewData.screwNutsCapacity) // Check if data is added beyond capacity.. if than increase the capacity and add data
            //    increaseValue = addNewDataCount;

            //int targetScrewCapacity = Mathf.Clamp(targetScrewData.screwNutsCapacity + increaseValue, LevelEditorConstants.Min_Number_Of_Nuts_In_Screw, LevelEditorConstants.Max_Number_Of_Nuts_In_Screw);

            //if (targetScrewData.screwNutsCapacity == targetScrewCapacity && addNewDataCount == 0)
            //{
            //    RaiseOnLevelEditorNutsDataCountChanged();
            //    return;
            //}

            //targetScrewData.screwNutsCapacity = targetScrewCapacity;

            //if (nutsData == null && targetScrewCapacity > 0) // Check if data exist.. if not, make one
            //{
            //    nutsData = new ScrewNutsLevelDataInfo();

            //    nutsData.targetScrewGridCellId = levelArrangementConfigDataSO.arrangementCellIds[screwDataIndex];
            //    nutsData.levelNutDataInfos = new List<BaseNutLevelDataInfo>();

            //    if (editLevelData.screwNutsLevelDataInfos.Count < screwDataIndex)
            //        editLevelData.screwNutsLevelDataInfos.Add(nutsData);
            //    else
            //        editLevelData.screwNutsLevelDataInfos.Insert(screwDataIndex, nutsData);
            //}

            //if (nutsData.levelNutDataInfos.Count > targetScrewCapacity) // If target capacity is less than current capacity
            //{
            //    int targetDifference = nutsData.levelNutDataInfos.Count - targetScrewCapacity;
            //    var removeDatas = nutsData.levelNutDataInfos.Take(targetDifference).ToList();

            //    removeDatas.ForEach(x => nutsData.levelNutDataInfos.Remove(x));
            //    if (nutsData.levelNutDataInfos.Count == 0)
            //    {
            //        editLevelData.screwNutsLevelDataInfos.Remove(nutsData);
            //    }
            //}
            //else if (nutsData.levelNutDataInfos.Count < targetScrewData.screwNutsCapacity && addNewDataCount > 0)
            //{
            //    int targetIncrease = Mathf.Min(targetScrewData.screwNutsCapacity - nutsData.levelNutDataInfos.Count, addNewDataCount);
            //    for (int i = 0; i < targetIncrease; i++)
            //    {
            //        BaseNutLevelDataInfo clonableData = new BaseNutLevelDataInfo() { nutType = 1, nutColorTypeId = 1 };
            //        if (nutsData.levelNutDataInfos.Count > 0)
            //            clonableData = nutsData.levelNutDataInfos[0].Clone();

            //        nutsData.levelNutDataInfos.Insert(0, clonableData);
            //    }
            //}

            LevelBuilder_OnRegerateWholeLevel();
            RaiseOnLevelEditorNutsDataCountChanged();
        }

        public void Main_OnRemoveNutData(int screwDataIndex, int targetNutIndex = 0)
        {
            //var targetScrewData = tempEditLevelDataSO.levelScrewDataInfos[screwDataIndex];
            //targetScrewData.screwNutsCapacity = Mathf.Max(0, targetScrewData.screwNutsCapacity - 1);
            //LevelArrangementConfigDataSO levelArrangementConfigDataSO = GetCurrentArrangementConfig();
            //var nutsData = editLevelData.screwNutsLevelDataInfos.Find(x => x.targetScrewGridCellId == levelArrangementConfigDataSO.arrangementCellIds[screwDataIndex]);
            //if (nutsData.levelNutDataInfos.Count > targetNutIndex) // If target capacity is less than current capacity
            //{
            //    nutsData.levelNutDataInfos.RemoveAt(targetNutIndex);
            //    if (nutsData.levelNutDataInfos.Count == 0)
            //        editLevelData.screwNutsLevelDataInfos.Remove(nutsData);
            //}

            LevelBuilder_OnRegerateWholeLevel();
            RaiseOnLevelEditorNutsDataCountChanged();
        }

        public void Main_OnChangeAddScrewCapacity(int screwDataIndex)
        {
            Main_OnChangeScrewCapacity(screwDataIndex, 0, 1);
        }

        public void Main_OnChangeNutType(int nutDataIndex, int nutIndex, int targetNutType)
        {
            //LevelArrangementConfigDataSO levelArrangementConfigDataSO = GetCurrentArrangementConfig();
            //var screwDataCellId = levelArrangementConfigDataSO.arrangementCellIds[nutDataIndex];
            //var nutData = editLevelData.screwNutsLevelDataInfos.Find(x => x.targetScrewGridCellId == screwDataCellId);

            //if (nutData != null && nutData.levelNutDataInfos.Count > nutIndex)
            //    nutData.levelNutDataInfos[nutIndex].nutType = targetNutType;

            LevelBuilder_OnRegerateWholeLevel();
            RaiseOnLevelEditorNutsDataCountChanged();
        }

        public void Main_OnChangeNutColorType(int nutDataIndex, int nutIndex, int targetNutColorType)
        {
            //LevelArrangementConfigDataSO levelArrangementConfigDataSO = GetCurrentArrangementConfig();
            //var screwDataCellId = levelArrangementConfigDataSO.arrangementCellIds[nutDataIndex];
            //var nutData = editLevelData.screwNutsLevelDataInfos.Find(x => x.targetScrewGridCellId == screwDataCellId);

            //if (nutData != null && nutData.levelNutDataInfos.Count > nutIndex)
            //    nutData.levelNutDataInfos[nutIndex].nutColorTypeId = targetNutColorType;

            LevelBuilder_OnRegerateWholeLevel();
            RaiseOnLevelEditorNutsDataCountChanged();
        }

        public void Main_EnableTestingMode()
        {
            if (levelEditorTestingModeCoroutine == null)
                levelEditorTestingModeCoroutine = StartCoroutine(LevelEditorTestingModeStartCoroutine());
        }

        public void Main_StopTestingMode()
        {
            if (levelEditorTestingModeCoroutine == null)
                levelEditorTestingModeCoroutine = StartCoroutine(LevelEditorStopTestingModeStartCoroutine());
        }

        public void Main_SaveToMainData()
        {
            try
            {
                ProtoLevelDataFactory.SaveLevelData(aBTestType, currentLevelData);
                LevelEditorToastsView.Instance.ShowToastMessage("Level Save Successfull !");
            }
            catch (Exception e)
            {
                LevelEditorToastsView.Instance.ShowToastMessage("Level Save Failed ! Error : " + e.Message);
            }
        }
        #endregion


        #region PRIVATE_METHODS
        private void LoadEditor_LoadLevel(int targetLevel)
        {
            this.targetLevel = targetLevel;
            LevelData levelData = GetLevelData(targetLevel, targetLevelType);
            MakeNewLevelData(levelData);
        }

        private void LoadEditor_CreateNewLevel(int targetLevel = -1)
        {
            LevelData levelData;
            if (targetLevel == -1)
            {
                int lastLevel = GetTotalLevelCount(targetLevelType);
                targetLevel = lastLevel + 1;
                levelData = ProtoLevelDataFactory.GetLevelData(aBTestType, targetLevelType, lastLevel);
                levelData.level = targetLevel;
            }
            else
            {
                levelData = new LevelData();
                levelData.level = targetLevel;
                levelData.levelType = targetLevelType;
            }
            MakeNewLevelData(levelData);
            this.targetLevel = targetLevel;
        }

        private void LoadEditor_MakeDuplicateLevel(int targetLevel)
        {
            LevelData duplicateLevelData = GetLevelData(targetLevel, targetLevelType);

            this.targetLevel = GetTotalLevelCount(targetLevelType) + 1;
            MakeNewLevelData(duplicateLevelData);
        }

        private void LevelBuilder_OnRegerateWholeLevel() // TODO : Optimize this with regenerate only part of the level that changed
        {
            GameplayManager.Instance.LoadLevel(currentLevelData);
            ResetMainCameraOrthographicSize();

            Main_OnResetScrewSelectedForEdit();
        }

        private void OnLevelEditorInitialized()
        {
            LevelEditorUIManager.Instance.GetView<LevelEditorIntroView>().Show();
        }

        private void ResetMainCameraOrthographicSize()
        {
            CameraCache.TryFetchCamera(CameraCacheType.MAIN_SCENE_CAMERA, out Camera mainCam);
            mainCam.orthographicSize = targetOrthoSize;
        }
        #endregion

        #region EVENT_HANDLERS
        public delegate void LevelEditorVoidDataChangeEvent();
        public static event LevelEditorVoidDataChangeEvent onLevelEditorNutsDataCountChanged;
        public static void RaiseOnLevelEditorNutsDataCountChanged()
        {
            if (onLevelEditorNutsDataCountChanged != null)
                onLevelEditorNutsDataCountChanged.Invoke();
        }
        #endregion

        #region COROUTINES
        IEnumerator LevelEditorManagerLoadCoroutine()
        {
            targetLevelType = LevelType.NORMAL_LEVEL;
            screwObjectSelectorSR.gameObject.SetActive(false);
            SetGameViewSize();

            yield return null;

            DontDestroyOnLoad(mainEditorParent.gameObject);
            OnLevelEditorInitialized();
        }

        IEnumerator LevelEditorTestingModeStartCoroutine()
        {
            isTestingMode = true;

            LevelProgressManager.Instance.ResetLevelProgress();
            GameplayManager.Instance.LoadLevel(currentLevelData);

            ResetMainCameraOrthographicSize();


            Main_ResetScrewSelection();
            LevelEditorUIManager.Instance.GetView<LevelEditorMainEditView>().SetTestingMode(isTestingMode);

            yield return null;

            LevelEditorUIManager.Instance.SetGameplayInputBlocker(false);
            levelEditorTestingModeCoroutine = null;
        }

        IEnumerator LevelEditorStopTestingModeStartCoroutine()
        {
            isTestingMode = false;

            LevelProgressManager.Instance.ResetLevelProgress(); // Set current level progress null
            GameplayManager.Instance.LoadLevel(currentLevelData);
            ResetMainCameraOrthographicSize();

            GameplayManager.Instance.GameplayStateData.gameplayStateType = GameplayStateType.NONE;

            LevelEditorUIManager.Instance.SetGameplayInputBlocker(true);
            LevelEditorUIManager.Instance.GetView<LevelEditorMainEditView>().SetTestingMode(isTestingMode);

            yield return null;
            levelEditorTestingModeCoroutine = null;
        }

        IEnumerator LevelEditorLoadCoroutine()
        {
            LevelEditorUIManager.Instance.GetView<LevelEditorLoadingView>().Show();
            yield return LoadSceneCoroutine(SceneType.Loading.ToString());

            while (TutorialManager.Instance == null)
            {
                yield return null;
            }

            TutorialManager.Instance.CanPlayTutorial = false;

            while (GameplayManager.Instance == null || !GameplayManager.Instance.IsPlayingLevel)
            {
                yield return null;
            }

            DailyGoalsManager.Instance.StopSystem();
            LevelProgressManager.Instance.ResetLevelProgress(); // Set current level progress null
            GameplayManager.Instance.LoadLevel(currentLevelData);

            ResetMainCameraOrthographicSize();
            MainSceneUIManager.Instance.GetComponent<Canvas>().enabled = false;
            MainSceneUIManager.Instance.GetView<BannerAdsView>().Hide();

            yield return new WaitForSeconds(0.5f);

            LevelManager.Instance.RegisterOnLevelUnlod(Main_StopTestingMode);

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

        IEnumerator LevelEditorReloadCoroutine()
        {
            LevelEditorUIManager.Instance.GetView<LevelEditorLoadingView>().Show();

            GameplayManager.Instance.LoadLevel(currentLevelData);

            ResetMainCameraOrthographicSize();
            MainSceneUIManager.Instance.GetComponent<Canvas>().enabled = false;
            MainSceneUIManager.Instance.GetView<BannerAdsView>().Hide();

            yield return new WaitForSeconds(0.5f);

            LevelEditorUIManager.Instance.GetView<LevelEditorLoadingView>().Hide();
            LevelEditorUIManager.Instance.GetView<LevelEditorMainEditView>().Show();

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

        #region EDITOR_FUNCTIONS
        #endregion
    }

    public class LevelEditorConstants
    {
        public static string Level_Editor_Temp_Folder_Path = Application.dataPath + Level_Editor_Temp_Folder_Raw_Path;
        public static string Level_Editor_Temp_Folder_Relative_Path = "Assets" + Level_Editor_Temp_Folder_Raw_Path;

        public const string Level_Editor_Temp_Folder_Raw_Path = "/Level Editor/Data/Temp/";

        public const string Screw_Types_Key_Mapping_Ids_Path = EditorConstant.MAPPING_IDS_PATH + "/ScrewTypeIdMappings.asset";
        public const string Nut_Types_Key_Mapping_Ids_Path = EditorConstant.MAPPING_IDS_PATH + "/NutTypeIdMappings.asset";
        public const string NutColor_Types_Key_Mapping_Ids_Path = EditorConstant.MAPPING_IDS_PATH + "/NutColorIdMappings.asset";
        public const string Level_Arrangement_Key_Mapping_Ids_Path = EditorConstant.MAPPING_IDS_PATH + "/LevelArrangementIdMapping.asset";

        public const int Min_Number_Of_Nuts_In_Screw = 1;
        public const int Max_Number_Of_Nuts_In_Screw = 10;
    }
}