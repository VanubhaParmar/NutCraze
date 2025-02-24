using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Tag.NutSort.Editor
{
    public class LevelEditorWindow : OdinEditorWindow
    {
        #region PRIVATE_VARIABLES
        [SerializeField] private ABTestType aBTestType = ABTestType.Default;

        private const string LEVEL_NUMBER = "Level";
        private const string LEVEL_TYPE = "LevelType";
        private const string LEVEL_ARRANGEMENT = "LevelArrangement";
        private const string SCREW_TYPES = "ScrewTypes";
        private const string SCREW_CAPACITIES = "ScrewCapacities";
        private const string SCREW_POSITIONS = "ScrewPositions";
        private const string NUT_TYPES = "NutTypes";
        private const string NUT_COLORS = "NutColors";

        private const string CSV_PATH = "Assets/CSVFiles/";
        private const string LEVEL_DATA_PATH = "Assets/Data/LevelData/";
        private const string EXPORTED_LEVELS_CSV = "Exported_Levels_{0}.csv";
        private const string LEVELS_CSV = "Levels_{0}.csv";
        #endregion

        #region LINKS
        private static Dictionary<ABTestType, string> linkMapping = new Dictionary<ABTestType, string>()
        {
            { ABTestType.Default, "https://docs.google.com/spreadsheets/d/e/2PACX-1vTdIC-xXtc6irTCego98N1J-a04n72lAFcVHp48luVyktgrMGoG3v6OOXbkw7dBaTLQpQygcmljaUu4/pub?gid=739587967&single=true&output=csv"},
            { ABTestType.AB1, "https://docs.google.com/spreadsheets/d/e/2PACX-1vTdIC-xXtc6irTCego98N1J-a04n72lAFcVHp48luVyktgrMGoG3v6OOXbkw7dBaTLQpQygcmljaUu4/pub?gid=739587967&single=true&output=csv"}
        };
        #endregion

        #region PRIVATE_METHODS
        [MenuItem("Tools/NutSort/Level Editor")]
        private static void OpenWindow()
        {
            GetWindow<LevelEditorWindow>().Show();
        }

        private string GetCurrentCsvFileName()
        {
            return string.Format(LEVELS_CSV, aBTestType.ToString());
        }

        private string GetExportedCsvFileName()
        {
            return string.Format(EXPORTED_LEVELS_CSV, aBTestType.ToString());
        }

        private string GetLevelDataPath(bool isSpecialLevel)
        {
            string abTestPath = Path.Combine(LEVEL_DATA_PATH, aBTestType.ToString());
            string levelsFolder = isSpecialLevel ? "Special Levels" : "Levels";
            return Path.Combine(abTestPath, levelsFolder);
        }

        [Button]
        public void LoadLevelCSV()
        {
            string filePath = Path.Combine(CSV_PATH, GetCurrentCsvFileName());
            if (File.Exists(filePath))
                File.Delete(filePath);

            if (!linkMapping.ContainsKey(aBTestType))
            {
                Debug.LogError($"No CSV link found for AB Test Type: {aBTestType}");
                return;
            }

            WebRequestInEditor editor = new WebRequestInEditor();
            editor.Request(linkMapping[aBTestType], request =>
            {
                if (string.IsNullOrEmpty(request.error))
                {
                    WriteFile(filePath, request.downloadHandler.data).ContinueWith(task =>
                    {
                        if (task.IsCompleted)
                        {
                            Debug.Log($"File Writing done for {aBTestType}");
                            RefreshUnity();
                        }

                        if (task.IsCanceled || task.IsFaulted)
                        {
                            Debug.Log("Something went wrong");
                        }
                    });
                }
                else
                {
                    Debug.LogError(request.error);
                }
            });
        }

        private async Task WriteFile(string filePath, byte[] data)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            using (FileStream file = File.Create(filePath, 4096, FileOptions.Asynchronous))
            {
                await file.WriteAsync(data, 0, data.Length);
            }
        }

        [Button("Generate Levels")]
        public void GenerateLevels()
        {
            string csvPath = Path.Combine(CSV_PATH, GetCurrentCsvFileName());
            if (!File.Exists(csvPath))
            {
                Debug.LogError($"CSV file not found at path: {csvPath}");
                return;
            }

            List<Dictionary<string, object>> list = CSVConverter.ReadCSV(csvPath);
            foreach (var levelData in list)
                GenerateLevelDataSO(levelData);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void GenerateLevelDataSO(Dictionary<string, object> levelData)
        {
            LevelType levelType = (LevelType)ParseInt(levelData, LEVEL_TYPE);
            var levelDataSO = GetLevelDataSO(ParseInt(levelData, LEVEL_NUMBER), levelType);
            levelDataSO.levelType = (LevelType)ParseInt(levelData, LEVEL_TYPE);
            levelDataSO.ArrangementId = ParseInt(levelData, LEVEL_ARRANGEMENT);
            levelDataSO.levelScrewDataInfos = ParseScrewData(levelData);
            levelDataSO.screwNutsLevelDataInfos = ParseNutsData(levelData);
            EditorUtility.SetDirty(levelDataSO);
            AssetDatabase.SaveAssetIfDirty(levelDataSO);
        }

        public LevelDataSO GetLevelDataSO(int level, LevelType leveltype)
        {
            bool isSpecialLevel = leveltype == LevelType.SPECIAL_LEVEL;
            string levelPath = GetLevelDataPath(isSpecialLevel);

            if (!Directory.Exists(levelPath))
                Directory.CreateDirectory(levelPath);

            string assetPath = Path.Combine(levelPath, $"Level {level}.asset");

            LevelDataSO asset = AssetDatabase.LoadAssetAtPath<LevelDataSO>(assetPath);

            if (asset == null)
            {
                asset = CreateInstance<LevelDataSO>();
                asset.level = level;
                AssetDatabase.CreateAsset(asset, assetPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"Created new level asset at: {assetPath}");
            }

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            return asset;
        }

        private List<BaseScrewLevelDataInfo> ParseScrewData(Dictionary<string, object> levelData)
        {
            var screwDataList = new List<BaseScrewLevelDataInfo>();
            string[] screwTypes = ParseString(levelData, SCREW_TYPES).Split(',');
            string[] screwCapacities = ParseString(levelData, SCREW_CAPACITIES).Split(',');

            for (int i = 0; i < screwTypes.Length; i++)
            {
                if (string.IsNullOrEmpty(screwTypes[i])) continue;

                int screwType = int.Parse(screwTypes[i].Trim());
                int screwNutCapacity = int.Parse(screwCapacities[i].Trim());
                var screwData = new BaseScrewLevelDataInfo
                {
                    screwType = screwType,
                    screwNutsCapacity = screwNutCapacity
                };
                screwDataList.Add(screwData);
            }
            return screwDataList;
        }

        private List<ScrewNutsLevelDataInfo> ParseNutsData(Dictionary<string, object> levelData)
        {
            var nutsDataList = new List<ScrewNutsLevelDataInfo>();
            string[] positions = ParseString(levelData, SCREW_POSITIONS).Split(';');
            string[] types = ParseString(levelData, NUT_TYPES).Split(';');
            string[] colors = ParseString(levelData, NUT_COLORS).Split(';');

            for (int i = 0; i < positions.Length; i++)
            {
                string pos = positions[i];
                string type = types[i];
                string color = colors[i];

                if (!string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(color))
                {
                    var nutPosition = pos.Split(',');
                    var nutTypes = type.Split(',');
                    var nutColors = color.Split(',');
                    var screwNutsData = new ScrewNutsLevelDataInfo
                    {
                        targetScrewGridCellId = new GridCellId(int.Parse(nutPosition[0].Trim()), int.Parse(nutPosition[1].Trim())),
                        levelNutDataInfos = new List<BaseNutLevelDataInfo>()
                    };

                    for (int j = 0; j < nutTypes.Length; j++)
                    {
                        var nutData = new BaseNutLevelDataInfo
                        {
                            nutType = int.Parse(nutTypes[j].Trim()),
                            nutColorTypeId = int.Parse(nutColors[j].Trim())
                        };
                        screwNutsData.levelNutDataInfos.Add(nutData);
                    }
                    nutsDataList.Add(screwNutsData);
                }
            }
            return nutsDataList;
        }

        [Button("Export Levels to CSV")]
        public void ExportLevelsToCSV()
        {
            string csvPath = Path.Combine(CSV_PATH, GetExportedCsvFileName());
            Debug.Log("csvPath- " + csvPath);
            if (File.Exists(csvPath))
                File.Delete(csvPath);

            var levelDataList = LoadAllLevelData();
            var csvData = ConvertLevelDataToCSVFormat(levelDataList);

            Directory.CreateDirectory(Path.GetDirectoryName(csvPath));
            CSVConverter.ConvertToCSV(csvData, csvPath);
            Debug.Log($"Successfully exported level data to: {csvPath}");
            RefreshUnity();
        }

        private List<LevelDataSO> LoadAllLevelData()
        {
            var levelDataList = new List<LevelDataSO>();

            string normalLevelsPath = GetLevelDataPath(false);
            string specialLevelsPath = GetLevelDataPath(true);

            LoadLevelsFromPath(normalLevelsPath, levelDataList);
            LoadLevelsFromPath(specialLevelsPath, levelDataList);

            return levelDataList.OrderBy(x => x.level).ToList();
        }

        private void LoadLevelsFromPath(string path, List<LevelDataSO> levelDataList)
        {
            if (!Directory.Exists(path))
            {
                Debug.LogWarning($"Directory not found: {path}");
                return;
            }

            var guids = AssetDatabase.FindAssets("t:LevelDataSO", new[] { path });
            foreach (var guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var levelData = AssetDatabase.LoadAssetAtPath<LevelDataSO>(assetPath);
                if (levelData != null)
                {
                    levelDataList.Add(levelData);
                }
            }
        }

        private List<Dictionary<string, object>> ConvertLevelDataToCSVFormat(List<LevelDataSO> levelDataList)
        {
            var csvData = new List<Dictionary<string, object>>();

            foreach (var levelData in levelDataList)
            {
                var entry = new Dictionary<string, object>
                {
                    { LEVEL_NUMBER, levelData.level },
                    { LEVEL_TYPE, (int)levelData.levelType },
                    { LEVEL_ARRANGEMENT, levelData.ArrangementId}
                };

                entry[SCREW_TYPES] = string.Join(",", levelData.levelScrewDataInfos.Select(x => x.screwType));
                entry[SCREW_CAPACITIES] = string.Join(",", levelData.levelScrewDataInfos.Select(x => x.screwNutsCapacity));

                var screwPositions = new List<string>();
                var nutTypes = new List<string>();
                var nutColors = new List<string>();

                foreach (var screwNuts in levelData.screwNutsLevelDataInfos)
                {
                    screwPositions.Add($"{screwNuts.targetScrewGridCellId.rowNumber},{screwNuts.targetScrewGridCellId.colNumber}");
                    nutTypes.Add(string.Join(",", screwNuts.levelNutDataInfos.Select(x => x.nutType)));
                    nutColors.Add(string.Join(",", screwNuts.levelNutDataInfos.Select(x => x.nutColorTypeId)));
                }

                entry[SCREW_POSITIONS] = string.Join(";", screwPositions);
                entry[NUT_TYPES] = string.Join(";", nutTypes);
                entry[NUT_COLORS] = string.Join(";", nutColors);

                csvData.Add(entry);
            }

            return csvData;
        }

        private int ParseInt(Dictionary<string, object> data, string key)
        {
            return data.ContainsKey(key) ? System.Convert.ToInt32(data[key].ToString()) : 0;
        }

        private string ParseString(Dictionary<string, object> data, string key)
        {
            return data.ContainsKey(key) ? data[key].ToString() : string.Empty;
        }

        [Button]
        public void RefreshUnity()
        {
            AssetDatabase.Refresh();
        }
        #endregion
    }
}