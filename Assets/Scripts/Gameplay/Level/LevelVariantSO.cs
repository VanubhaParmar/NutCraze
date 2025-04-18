using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "LevelVariant", menuName = Constant.GAME_NAME + "/ABTesting/Level Variant")]
    public class LevelVariantSO : SerializedScriptableObject
    {
        #region PRIVATE_VARIABLES
        [SerializeField] private NutColorThemeTemplateDataSO nutColorThemeDataSO;

        [DictionaryDrawerSettings(KeyLabel = "Level Number", ValueLabel = "Level SO", DisplayMode = DictionaryDisplayOptions.OneLine)]
        [SerializeField, Header("Normal Levels (AB Variant)")]
        private Dictionary<int, LevelDataSO> normalLevels = new Dictionary<int, LevelDataSO>();

        [SerializeField] private int repeatLastLevelsCountAfterGameFinish = 50;
        [SerializeField] private int playSpecialLevelAfterEveryLevelsCount = 8;
        [DictionaryDrawerSettings(KeyLabel = "Level Number", ValueLabel = "Level SO", DisplayMode = DictionaryDisplayOptions.OneLine)]
        [SerializeField, Header("Special Levels (AB Variant)")]
        private Dictionary<int, LevelDataSO> specialLevels = new Dictionary<int, LevelDataSO>();

        #endregion

        #region PROPERTIES

        public int RepeatLastLevelsCountAfterGameFinish => repeatLastLevelsCountAfterGameFinish;
        public int PlaySpecialLevelAfterEveryLevelsCount => playSpecialLevelAfterEveryLevelsCount;

        #endregion

        #region PUBLIC_METHODS
        public LevelDataSO GetNormalLevel(int levelNumber)
        {
            if (normalLevels.TryGetValue(levelNumber, out LevelDataSO so))
                return so;
            return null;
        }

        public LevelDataSO GetSpecialLevel(int levelNumber)
        {
            if (specialLevels.TryGetValue(levelNumber, out LevelDataSO so))
                return so;
            return null;
        }

        public List<LevelDataSO> GetAllNormalLevels()
        {
            return new List<LevelDataSO>(normalLevels.Values);
        }

        public List<LevelDataSO> GetAllSpecialLevels()
        {
            return new List<LevelDataSO>(specialLevels.Values);
        }

        public int GetNormalLevelCount()
        {
            return normalLevels.Count;
        }

        public int GetSpecailLevelCount()
        {
            return specialLevels.Count;
        }

        public bool HasSpecialLevel(int specialLevelNumber)
        {
            return specialLevels.ContainsKey(specialLevelNumber);
        }

        public bool CanLoadSpecialLevel(int currentLevel)
        {
            return (currentLevel - 1) % playSpecialLevelAfterEveryLevelsCount == 0 && currentLevel > 1;
        }

        public int GetSpecialLevelNumberCountToLoad(int currentLevel)
        {
            if (CanLoadSpecialLevel(currentLevel))
                return Mathf.FloorToInt((currentLevel - 1) / playSpecialLevelAfterEveryLevelsCount);
            return 0;
        }

        public NutColorThemeInfo GetNutTheme(int nutColorId)
        {
            return nutColorThemeDataSO.GetNutTheme(nutColorId);
        }
        #endregion

        #region EDITOR
#if UNITY_EDITOR
        [Button]
        public void SetLevels(LevelABTestType aBTestType)
        {
            this.normalLevels.Clear();
            var normalLevels = GetLevelDataSOs(Editor.ResourcesConstants.LEVELS_PATH + aBTestType.ToString() + "/Levels/");
            for (int i = 0; i < normalLevels.Count; i++)
                this.normalLevels.Add(normalLevels[i].level, normalLevels[i]);


            this.specialLevels.Clear();
            var specialLevels = GetLevelDataSOs(Editor.ResourcesConstants.LEVELS_PATH + aBTestType.ToString() + "/Special Levels/");
            for (int i = 0; i < specialLevels.Count; i++)
                this.specialLevels.Add(specialLevels[i].level, specialLevels[i]);

            UnityEditor.EditorUtility.SetDirty(this);
        }

        private List<LevelDataSO> GetLevelDataSOs(string path)
        {
            var levelDataList = new List<LevelDataSO>();
            var guids = UnityEditor.AssetDatabase.FindAssets("t:LevelDataSO", new[] { path });

            foreach (var guid in guids)
            {
                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var levelData = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelDataSO>(assetPath);
                if (levelData != null)
                {
                    levelDataList.Add(levelData);
                }
            }
            return levelDataList;
        }

        [Button]
        public void FindLevelErrors()
        {
            foreach (var item in normalLevels)
                item.Value.ValidateLevelData();

            foreach (var item in specialLevels)
                item.Value.ValidateLevelData();
        }

        [Button]
        public void FindArrangementErrors()
        {
            foreach (var item in normalLevels)
            {
                if (item.Value.ArrangementId == 0)
                    Debug.Log("ArrangementId is not valid for normal Level " + item.Value.level);
            }

            foreach (var item in specialLevels)
            {
                if (item.Value.ArrangementId == 0)
                    Debug.Log("ArrangementId is not valid for specail Level " + item.Value.level);
            }
        }

        [Button]
        public void VarifyScrewData()
        {
            string path = "Assets/Data/Level Arrangement Configs/1.LevelArrangementsDataSO.asset";
            var list = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelArrangementsListDataSO>(path);
            foreach (var item in normalLevels)
            {
                LevelArrangementConfigDataSO so = list.GetLevelArrangementConfig(item.Value.ArrangementId);
                if (item.Value.levelScrewDataInfos.Count != so.arrangementCellIds.Count)
                {
                    Debug.LogError("Screw data count is not valid for normal Level " + item.Value.level);
                }
            }

            foreach (var item in specialLevels)
            {
                LevelArrangementConfigDataSO so = list.GetLevelArrangementConfig(item.Value.ArrangementId);
                if (item.Value.levelScrewDataInfos.Count != so.arrangementCellIds.Count)
                {
                    Debug.LogError("Screw data count is not valid for specail Level " + item.Value.level);
                }
            }

        }
#endif
        #endregion
    }
}
