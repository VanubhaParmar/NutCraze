#if UNITY_EDITOR
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Scene = UnityEngine.SceneManagement.SceneManager;

namespace Tag.NutSort.Editor
{
    public class LevelEditorManager : SerializedManager<LevelEditorManager>
    {
        #region PUBLIC_VARIABLES
        [SerializeField] private LevelVariantMasterSO levelVariantMasterSO;
        public Transform mainEditorParent;
        public RectTransform mainEditorUIParent;
        public float gameWidth = 0.4f;
        public int targetGameWindowResolution;
        public Vector2Int targetScreenResolution;
        public float targetOrthoSize = 8f;

        [Space]
        public SpriteRenderer screwObjectSelectorSR;
        public LevelGridSetter levelGridSetter;

        [Space]
        [SerializeField, ScrewTypeId] private List<int> removeSelectionOfScrewTypesFromData = new List<int>();
        [SerializeField, NutColorId] private List<int> removeSelectionOfNutColorTypesFromData = new List<int>();
        [SerializeField, NutTypeId] private List<int> removeSelectionOfNutTypesFromData = new List<int>();

        [Space]
        [SerializeField] private ScrewArrangementsDataSO _levelArrangementsListDataSO;
        [SerializeField] private BaseIDMappingConfig levelArrangementIdMaaping;
        [SerializeField] private LevelDataSO _defaultLevelDataSO;
        [SerializeField] private LevelABTestType aBTestType = LevelABTestType.Default;
        #endregion

        #region PROPERTIES
        public int TargetLevel => targetLevel;
        public LevelDataSO TempEditLevelDataSO => tempEditLevelDataSO;
        public ScrewArrangementsDataSO LevelArrangementsListDataSO => _levelArrangementsListDataSO;
        public BaseIDMappingConfig LevelArrangementIdMaaping => levelArrangementIdMaaping;
        public LevelType TargetLevelType => targetLevelType;

        public LevelVariantSO LevelVariantSO
        {
            get
            {
                Debug.Log("LevelVariantSO " + aBTestType);
                return levelVariantMasterSO.GetLevelVariant(ABTestType);
            }
        }

        public LevelABTestType ABTestType { get => aBTestType; set => aBTestType = value; }
        #endregion

        #region PRIVATE_VARIABLES
        private int targetLevel;
        private LevelType targetLevelType;
        [ShowInInspector] private LevelDataSO targetLevelDataSO;
        [ShowInInspector] private LevelDataSO tempEditLevelDataSO;
        private Coroutine levelEditorLoadCoroutine;
        private Coroutine levelEditorTestingModeCoroutine;

        private bool isTestingMode;
        private int currentSelectionScrewDataIndex = -1;
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
        public List<LevelABTestType> GetAvailableABVariants()
        {
            return levelVariantMasterSO.GetAvailableABVariants();
        }

        public List<string> GetAllArrangementOptions()
        {
            List<string> options = new List<string>();
            List<int> list = _levelArrangementsListDataSO.GetAllArrangementIds();
            for (int i = 0; i < list.Count; i++)
            {
                options.Add(levelArrangementIdMaaping.GetNameFromId(list[i]));
            }
            return options;
        }

        public ScrewArrangementConfigSO GetArrangementConfigSO(int arrangementID)
        {
            return _levelArrangementsListDataSO.GetScrewArrangementConfigSO(arrangementID);
        }

        public ScrewArrangementConfigSO GetCurrentArrangementConfig()
        {
            return GetArrangementConfigSO(tempEditLevelDataSO.ArrangementId);
        }

        public void ShowGrid(ScrewArrangementConfigSO arrangementConfig)
        {
            levelGridSetter.ShowGrid(arrangementConfig);
        }

        public void ShowGrid(ScrewArrangementConfig arrangementConfig)
        {
            levelGridSetter.ShowGrid(arrangementConfig);
        }

        public void ShowCurrentLevelGrid()
        {
            ShowGrid(GetCurrentArrangementConfig());
        }

        public void InitializeLevelEditorManager()
        {
            StartCoroutine(LevelEditorManagerLoadCoroutine());
        }

        public bool DoesLevelExist(int level, LevelType levelType = LevelType.NORMAL_LEVEL)
        {
            return GetLevelDataSOOfLevel(level, levelType) != null;
        }

        public LevelDataSO GetLevelDataSOOfLevel(int level, LevelType levelType = LevelType.NORMAL_LEVEL)
        {
            if (levelType == LevelType.NORMAL_LEVEL)
                return LevelVariantSO.GetNormalLevel(level);
            return LevelVariantSO.GetSpecialLevel(level);
        }

        public int GetTotalNumberOfLevels(LevelType levelType = LevelType.NORMAL_LEVEL)
        {
            if (levelType == LevelType.SPECIAL_LEVEL)
                LevelVariantSO.GetSpecailLevelCount();
            return LevelVariantSO.GetNormalLevelCount();
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

        [Button]
        public void ClearTempFolder()
        {
            string directoryPath = LevelEditorConstants.Level_Editor_Temp_Folder_Path;
            string[] files = System.IO.Directory.GetFiles(directoryPath);

            foreach (string file in files)
            {
                Debug.Log("Deleted file : " + file);
                System.IO.File.Delete(file);
            }

            SaveAssets();
        }

        public void MakeTempLevelDataSo()
        {
            tempEditLevelDataSO = Instantiate(targetLevelDataSO);
            LevelEditorUtility.CreateAsset(tempEditLevelDataSO, LevelEditorConstants.Level_Editor_Temp_Folder_Relative_Path + targetLevelDataSO.name + ".asset");
            SaveAssets();
        }

        public void MakeTempLevelDataSo(LevelDataSO levelDataSO, string soName)
        {
            tempEditLevelDataSO = Instantiate(levelDataSO);
            LevelEditorUtility.CreateAsset(tempEditLevelDataSO, LevelEditorConstants.Level_Editor_Temp_Folder_Relative_Path + soName + ".asset");
            SaveAssets();
        }

        public LevelDataSO MakeResourceLevelDataSo(LevelDataSO levelDataSO, string soName = "")
        {
            if (string.IsNullOrEmpty(soName))
            {
                if (levelDataSO.levelType == LevelType.NORMAL_LEVEL)
                    soName = string.Format(ResourcesConstants.LEVEL_SO_NAME_FORMAT, levelDataSO.level);
                else
                    soName = string.Format(ResourcesConstants.SPECIAL_LEVEL_SO_NAME_FORMAT, levelDataSO.level);
            }

            var resourceLevelDataSO = Instantiate(levelDataSO);
            LevelEditorUtility.CreateAsset(resourceLevelDataSO, GetLevelsPath(levelDataSO.levelType) + soName + ".asset");
            SaveAssets();

            return resourceLevelDataSO;
        }

        public List<string> GetMappingIds<T>() where T : BaseIdAttribute
        {
            List<string> ids = new List<string>();
            List<int> removeList = new List<int>();
            BaseIDMappingConfig keyIds = null;

            if (typeof(ScrewTypeIdAttribute).IsAssignableFrom(typeof(T)))
            {
                keyIds = LevelEditorUtility.LoadAssetAtPath<BaseIDMappingConfig>(LevelEditorConstants.Screw_Types_Key_Mapping_Ids_Path);
                removeList = removeSelectionOfScrewTypesFromData;
            }
            else if (typeof(NutTypeIdAttribute).IsAssignableFrom(typeof(T)))
            {
                removeList = removeSelectionOfNutTypesFromData;
                keyIds = LevelEditorUtility.LoadAssetAtPath<BaseIDMappingConfig>(LevelEditorConstants.Nut_Types_Key_Mapping_Ids_Path);
            }
            else if (typeof(NutColorIdAttribute).IsAssignableFrom(typeof(T)))
            {
                removeList = removeSelectionOfNutColorTypesFromData;
                keyIds = LevelEditorUtility.LoadAssetAtPath<BaseIDMappingConfig>(LevelEditorConstants.NutColor_Types_Key_Mapping_Ids_Path);
            }

            if (keyIds != null)
                keyIds.idMapping.ForEach(x =>
                {
                    if (!removeList.Contains(x.Key))
                        ids.Add(x.Key + "-" + x.Value);
                });

            return ids;
        }
        #endregion

        #region LEVEL_EDITOR_MAIN_METHODS
        public void Main_OnChangeLevelArrangementConfig(int arrangementId)
        {
            tempEditLevelDataSO.ArrangementId = arrangementId;
            ScrewArrangementConfigSO levelArrangementConfigDataSO = GetCurrentArrangementConfig();
            int targetScrewCapacity = levelArrangementConfigDataSO.arrangementCellIds.Count;

            if (tempEditLevelDataSO.levelScrewDataInfos.Count > targetScrewCapacity)
            {
                int removeExtraData = tempEditLevelDataSO.levelScrewDataInfos.Count - targetScrewCapacity;

                for (int i = 0; i < removeExtraData; i++)
                {
                    tempEditLevelDataSO.levelScrewDataInfos.RemoveAt(tempEditLevelDataSO.levelScrewDataInfos.Count - 2); // Remove last most screw data before booster activated screw
                }
            }
            else if (tempEditLevelDataSO.levelScrewDataInfos.Count < targetScrewCapacity)
            {
                int addExtraData = targetScrewCapacity - tempEditLevelDataSO.levelScrewDataInfos.Count;

                for (int i = 0; i < addExtraData; i++)
                {
                    var duplicateData = tempEditLevelDataSO.levelScrewDataInfos.First().Clone();
                    tempEditLevelDataSO.levelScrewDataInfos.Insert(tempEditLevelDataSO.levelScrewDataInfos.Count - 2, duplicateData); // Add screw data before booster activated screw
                }
            }

            List<int> removeNutsDataIndexes = new List<int>();
            for (int i = 0; i < tempEditLevelDataSO.screwNutsLevelDataInfos.Count; i++) // Reset nuts data
            {
                if (i < levelArrangementConfigDataSO.arrangementCellIds.Count - 1) // last is always booster activated screw so cant assign that
                    tempEditLevelDataSO.screwNutsLevelDataInfos[i].targetScrewGridCellId = levelArrangementConfigDataSO.arrangementCellIds[i];
                else
                    removeNutsDataIndexes.Add(i);
            }
            removeNutsDataIndexes.ForEach(x => tempEditLevelDataSO.screwNutsLevelDataInfos.RemoveAt(x));
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
            ScrewArrangementConfigSO levelArrangementConfigDataSO = GetCurrentArrangementConfig();
            if (currentSelectionScrewDataIndex < 0 || currentSelectionScrewDataIndex >= levelArrangementConfigDataSO.arrangementCellIds.Count)
                return;

            var screwCellId = levelArrangementConfigDataSO.arrangementCellIds[currentSelectionScrewDataIndex];

            var currentScrew = ScrewManager.Instance.GetScrewByCell(screwCellId);

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
            tempEditLevelDataSO.levelScrewDataInfos[screwDataIndex].screwType = targetScrewType;

            LevelBuilder_OnRegerateWholeLevel();
            RaiseOnLevelEditorNutsDataCountChanged();
        }

        public void Main_OnChangeScrewCapacity(int screwDataIndex, int increaseValue, int addNewDataCount = 0)
        {
            ScrewArrangementConfigSO levelArrangementConfigDataSO = GetCurrentArrangementConfig();
            var targetScrewData = tempEditLevelDataSO.levelScrewDataInfos[screwDataIndex];
            var nutsData = tempEditLevelDataSO.screwNutsLevelDataInfos.Find(x => x.targetScrewGridCellId == levelArrangementConfigDataSO.arrangementCellIds[screwDataIndex]);

            if (addNewDataCount > 0 && increaseValue == 0 && nutsData != null && nutsData.levelNutDataInfos.Count == targetScrewData.screwNutsCapacity) // Check if data is added beyond capacity.. if than increase the capacity and add data
                increaseValue = addNewDataCount;

            int targetScrewCapacity = Mathf.Clamp(targetScrewData.screwNutsCapacity + increaseValue, LevelEditorConstants.Min_Number_Of_Nuts_In_Screw, LevelEditorConstants.Max_Number_Of_Nuts_In_Screw);

            if (targetScrewData.screwNutsCapacity == targetScrewCapacity && addNewDataCount == 0)
            {
                RaiseOnLevelEditorNutsDataCountChanged();
                return;
            }

            targetScrewData.screwNutsCapacity = targetScrewCapacity;

            if (nutsData == null && targetScrewCapacity > 0) // Check if data exist.. if not, make one
            {
                nutsData = new ScrewNutsLevelDataInfo();

                nutsData.targetScrewGridCellId = levelArrangementConfigDataSO.arrangementCellIds[screwDataIndex];
                nutsData.levelNutDataInfos = new List<BaseNutLevelDataInfo>();

                if (tempEditLevelDataSO.screwNutsLevelDataInfos.Count < screwDataIndex)
                    tempEditLevelDataSO.screwNutsLevelDataInfos.Add(nutsData);
                else
                    tempEditLevelDataSO.screwNutsLevelDataInfos.Insert(screwDataIndex, nutsData);
            }

            if (nutsData.levelNutDataInfos.Count > targetScrewCapacity) // If target capacity is less than current capacity
            {
                int targetDifference = nutsData.levelNutDataInfos.Count - targetScrewCapacity;
                var removeDatas = nutsData.levelNutDataInfos.Take(targetDifference).ToList();

                removeDatas.ForEach(x => nutsData.levelNutDataInfos.Remove(x));
                if (nutsData.levelNutDataInfos.Count == 0)
                {
                    tempEditLevelDataSO.screwNutsLevelDataInfos.Remove(nutsData);
                }
            }
            else if (nutsData.levelNutDataInfos.Count < targetScrewData.screwNutsCapacity && addNewDataCount > 0)
            {
                int targetIncrease = Mathf.Min(targetScrewData.screwNutsCapacity - nutsData.levelNutDataInfos.Count, addNewDataCount);
                for (int i = 0; i < targetIncrease; i++)
                {
                    BaseNutLevelDataInfo clonableData = new BaseNutLevelDataInfo() { nutType = 1, nutColorTypeId = 1 };
                    if (nutsData.levelNutDataInfos.Count > 0)
                        clonableData = nutsData.levelNutDataInfos[0].Clone();

                    nutsData.levelNutDataInfos.Insert(0, clonableData);
                }
            }

            LevelBuilder_OnRegerateWholeLevel();
            RaiseOnLevelEditorNutsDataCountChanged();
        }

        public void Main_OnRemoveNutData(int screwDataIndex, int targetNutIndex = 0)
        {
            //var targetScrewData = tempEditLevelDataSO.levelScrewDataInfos[screwDataIndex];
            //targetScrewData.screwNutsCapacity = Mathf.Max(0, targetScrewData.screwNutsCapacity - 1);
            ScrewArrangementConfigSO levelArrangementConfigDataSO = GetCurrentArrangementConfig();
            var nutsData = tempEditLevelDataSO.screwNutsLevelDataInfos.Find(x => x.targetScrewGridCellId == levelArrangementConfigDataSO.arrangementCellIds[screwDataIndex]);
            if (nutsData.levelNutDataInfos.Count > targetNutIndex) // If target capacity is less than current capacity
            {
                nutsData.levelNutDataInfos.RemoveAt(targetNutIndex);
                if (nutsData.levelNutDataInfos.Count == 0)
                    tempEditLevelDataSO.screwNutsLevelDataInfos.Remove(nutsData);
            }

            LevelBuilder_OnRegerateWholeLevel();
            RaiseOnLevelEditorNutsDataCountChanged();
        }

        public void Main_OnChangeAddScrewCapacity(int screwDataIndex)
        {
            Main_OnChangeScrewCapacity(screwDataIndex, 0, 1);
        }

        public void Main_OnChangeNutType(int nutDataIndex, int nutIndex, int targetNutType)
        {
            ScrewArrangementConfigSO screwArrangementConfigSO = GetCurrentArrangementConfig();
            var screwDataCellId = screwArrangementConfigSO.arrangementCellIds[nutDataIndex];
            var nutData = tempEditLevelDataSO.screwNutsLevelDataInfos.Find(x => x.targetScrewGridCellId == screwDataCellId);

            if (nutData != null && nutData.levelNutDataInfos.Count > nutIndex)
                nutData.levelNutDataInfos[nutIndex].nutType = targetNutType;

            LevelBuilder_OnRegerateWholeLevel();
            RaiseOnLevelEditorNutsDataCountChanged();
        }

        public void Main_OnChangeNutColorType(int nutDataIndex, int nutIndex, int targetNutColorType)
        {
            ScrewArrangementConfigSO levelArrangementConfigDataSO = GetCurrentArrangementConfig();
            var screwDataCellId = levelArrangementConfigDataSO.arrangementCellIds[nutDataIndex];
            var nutData = tempEditLevelDataSO.screwNutsLevelDataInfos.Find(x => x.targetScrewGridCellId == screwDataCellId);

            if (nutData != null && nutData.levelNutDataInfos.Count > nutIndex)
                nutData.levelNutDataInfos[nutIndex].nutColorTypeId = targetNutColorType;

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
                if (tempEditLevelDataSO.ValidateLevelData())
                {
                    tempEditLevelDataSO.CloneTo(targetLevelDataSO);
                    LevelEditorToastsView.Instance.ShowToastMessage("Level Save Successfull !");
                    SaveAssets(targetLevelDataSO);
                }
                else
                {
                    LevelEditorToastsView.Instance.ShowToastMessage("<color=red>Level Save Failed ! Error : Level Data Validation Failed</color>");
                }
            }
            catch (Exception e)
            {
                LevelEditorToastsView.Instance.ShowToastMessage($"<color=red>Level Save Failed ! Error : {e.Message}</color>");
            }
        }
        #endregion

        #region PRIVATE_METHODS
        private string GetLevelsPath(LevelType levelType)
        {
            string path;
            if (levelType == LevelType.NORMAL_LEVEL)
            {
                path = ResourcesConstants.LEVELS_PATH + ABTestType.ToString() + "/Levels/";
            }
            else
            {
                path = ResourcesConstants.LEVELS_PATH + ABTestType.ToString() + "/Special Levels/";
            }
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        private void SaveAssets(UnityEngine.Object targetChangeObject = null)
        {
            if (targetChangeObject != null)
                LevelEditorUtility.SetDirty(targetChangeObject);
            LevelVariantSO levelVariantSO1 = LevelEditorUtility.LoadAssetAtPath<LevelVariantSO>("Assets/Data/LevelData/LevelVariant-" + ABTestType + ".asset");
            levelVariantSO1.SetLevels(ABTestType);
            LevelEditorUtility.SaveAssets();
            LevelEditorUtility.Refresh();
        }

        private void LoadEditor_LoadLevel(int targetLevel)
        {
            this.targetLevel = targetLevel;
            targetLevelDataSO = GetLevelDataSOOfLevel(targetLevel, targetLevelType);
            MakeTempLevelDataSo();
        }

        private void LoadEditor_CreateNewLevel(int targetLevelCount = -1)
        {
            if (targetLevelCount <= 0 || DoesLevelExist(targetLevelCount, targetLevelType))
                targetLevelCount = GetTotalNumberOfLevels(targetLevelType) + 1;

            this.targetLevel = targetLevelCount;
            MakeTempLevelDataSo(_defaultLevelDataSO, string.Format(ResourcesConstants.LEVEL_SO_NAME_FORMAT, targetLevel));

            tempEditLevelDataSO.level = targetLevel;
            tempEditLevelDataSO.levelType = targetLevelType;

            SaveAssets(tempEditLevelDataSO);

            targetLevelDataSO = MakeResourceLevelDataSo(tempEditLevelDataSO);
        }

        private void LoadEditor_MakeDuplicateLevel(int targetLevel)
        {
            var duplicateLevelTarget = GetLevelDataSOOfLevel(targetLevel, targetLevelType);

            this.targetLevel = GetTotalNumberOfLevels(targetLevelType) + 1;
            MakeTempLevelDataSo(duplicateLevelTarget, string.Format(ResourcesConstants.LEVEL_SO_NAME_FORMAT, this.targetLevel));

            tempEditLevelDataSO.level = this.targetLevel;
            tempEditLevelDataSO.levelType = targetLevelType;

            SaveAssets(tempEditLevelDataSO);

            targetLevelDataSO = MakeResourceLevelDataSo(tempEditLevelDataSO);
        }

        private void LevelBuilder_OnRegerateWholeLevel() // TODO : Optimize this with regenerate only part of the level that changed
        {
            GameplayManager.Instance.LoadLevel(tempEditLevelDataSO);
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
            ClearTempFolder();
            SetGameViewSize();

            yield return null;

            DontDestroyOnLoad(mainEditorParent.gameObject);
            OnLevelEditorInitialized();
        }

        IEnumerator LevelEditorTestingModeStartCoroutine()
        {
            isTestingMode = true;

            LevelProgressManager.Instance.ResetLevelProgress();
            GameplayManager.Instance.LoadLevel(tempEditLevelDataSO);

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
            GameplayManager.Instance.LoadLevel(tempEditLevelDataSO);
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

            while (MainSceneLoader.Instance == null)
            {
                yield return null;
            }

            while (!MainSceneLoader.Instance.IsInitialized)
            {
                yield return null;
            }

            TutorialManager.Instance.CanPlayTutorial = false;

            DailyGoalsManager.Instance.StopSystem();
            LevelProgressManager.Instance.ResetLevelProgress(); // Set current level progress null
            GameplayManager.Instance.LoadLevel(tempEditLevelDataSO);

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

            GameplayManager.Instance.LoadLevel(tempEditLevelDataSO);

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
        [Button]
        public void SetBoosterActivatedScrewMaxCapacity(int startLevel, int tillLevel, int capacity, [ScrewTypeId] int targetScrewType)
        {
            for (int i = startLevel; i <= tillLevel; i++)
            {
                var levelData = GetLevelDataSOOfLevel(i);
                if (levelData != null)
                {
                    var boosterScrewData = levelData.levelScrewDataInfos.Find(x => x.screwType == targetScrewType);
                    if (boosterScrewData != null)
                    {
                        boosterScrewData.screwNutsCapacity = capacity;

                        SaveAssets(levelData);
                    }
                }
            }
        }

        [Button]
        public void RenameLevels(int startLevel, int tillLevel, int startNameCount, LevelType targetLevelType)
        {
            for (int i = startLevel; i <= tillLevel; i++)
            {
                var levelData = GetLevelDataSOOfLevel(i, targetLevelType);
                if (levelData != null)
                {
                    levelData.level = startNameCount;
                    LevelEditorUtility.RenameAsset(LevelEditorUtility.GetAssetPath(levelData), string.Format(ResourcesConstants.LEVEL_SO_NAME_FORMAT, startNameCount));

                    LevelEditorUtility.SetDirty(levelData);
                    LevelEditorUtility.SaveAssets();
                    LevelEditorUtility.Refresh();
                }

                startNameCount++;
            }
        }
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

        public const int Min_Number_Of_Nuts_In_Screw = 1;
        public const int Max_Number_Of_Nuts_In_Screw = 10;
    }
}

#endif