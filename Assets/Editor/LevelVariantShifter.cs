using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Tag.NutSort.Editor
{
    public class LevelVariantShifter : EditorWindow
    {
        private LevelVariantSO sourceVariant;
        private LevelVariantSO targetVariant;
        private List<int> normalLevelsToExclude = new List<int> { 30, 45, 95, 106 };
        private List<int> specailLevelsToExclude = new List<int>();
        private List<int> normalLevelsToCopyToTarget = new List<int>();
        private List<int> specialLevelsToCopyToTarget = new List<int>();

        private LevelABTestType abTestTypeName = LevelABTestType.EasyLevels;

        [MenuItem("Tools/Level Variant Shifter")]
        public static void ShowWindow()
        {
            GetWindow<LevelVariantShifter>("Level Variant Shifter");
        }

        private void OnGUI()
        {
            GUILayout.Label("Level Variant Tools", EditorStyles.boldLabel);

            sourceVariant = EditorGUILayout.ObjectField("Source Variant", sourceVariant, typeof(LevelVariantSO), false) as LevelVariantSO;
            targetVariant = EditorGUILayout.ObjectField("Target Variant", targetVariant, typeof(LevelVariantSO), false) as LevelVariantSO;

            EditorGUILayout.Space();
            abTestTypeName = (LevelABTestType)EditorGUILayout.EnumPopup("AB Test Type", abTestTypeName);
            EditorGUILayout.Space();

            GUILayout.Label("Shifting Functionality", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Normal Levels to exclude (comma separated):");
            string normalLevelsToRemoveStr = EditorGUILayout.TextField(string.Join(",", normalLevelsToExclude));

            try
            {
                string[] levelStrings = normalLevelsToRemoveStr.Split(',');
                normalLevelsToExclude.Clear();
                foreach (string levelStr in levelStrings)
                {
                    if (int.TryParse(levelStr.Trim(), out int level))
                    {
                        normalLevelsToExclude.Add(level);
                    }
                }
            }
            catch
            {
                EditorGUILayout.HelpBox("Invalid level format for exclusion. Use comma-separated numbers.", MessageType.Error);
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Special Levels to exclude (comma separated):");
            string specailLevelsToRemoveStr = EditorGUILayout.TextField(string.Join(",", specailLevelsToExclude));

            try
            {
                string[] levelStrings = specailLevelsToRemoveStr.Split(',');
                specailLevelsToExclude.Clear();
                foreach (string levelStr in levelStrings)
                {
                    if (int.TryParse(levelStr.Trim(), out int level))
                    {
                        specailLevelsToExclude.Add(level);
                    }
                }
            }
            catch
            {
                EditorGUILayout.HelpBox("Invalid level format for exclusion. Use comma-separated numbers.", MessageType.Error);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Perform Shift and Renumber"))
            {
                if (sourceVariant == null)
                {
                    EditorUtility.DisplayDialog("Error", "Source variant is not set!", "OK");
                    return;
                }

                if (targetVariant == null)
                {
                    EditorUtility.DisplayDialog("Error", "Target variant is not set!", "OK");
                    return;
                }
                CreateShiftedVariant();
                EditorUtility.DisplayDialog("Success", "Levels shifted and renumbered.", "OK");
            }

            EditorGUILayout.Space(20);
            GUILayout.Label("Copy Specific Levels Functionality", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Normal Levels to copy (comma separated):");
            string normalLevelsToCopyStr = EditorGUILayout.TextField(string.Join(",", normalLevelsToCopyToTarget));
            try
            {
                string[] levelStrings = normalLevelsToCopyStr.Split(',');
                normalLevelsToCopyToTarget.Clear();
                foreach (string levelStr in levelStrings)
                {
                    if (int.TryParse(levelStr.Trim(), out int level))
                    {
                        normalLevelsToCopyToTarget.Add(level);
                    }
                }
                normalLevelsToCopyToTarget = normalLevelsToCopyToTarget.Distinct().ToList();
            }
            catch
            {
                EditorGUILayout.HelpBox("Invalid level format for copying. Use comma-separated numbers.", MessageType.Error);
            }


            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Special Levels to copy (comma separated):");
            string specialLevelsToCopyStr = EditorGUILayout.TextField(string.Join(",", specialLevelsToCopyToTarget));
            try
            {
                string[] levelStrings = specialLevelsToCopyStr.Split(',');
                specialLevelsToCopyToTarget.Clear();
                foreach (string levelStr in levelStrings)
                {
                    if (int.TryParse(levelStr.Trim(), out int level))
                    {
                        specialLevelsToCopyToTarget.Add(level);
                    }
                }
                specialLevelsToCopyToTarget = specialLevelsToCopyToTarget.Distinct().ToList();
            }
            catch
            {
                EditorGUILayout.HelpBox("Invalid level format for copying. Use comma-separated numbers.", MessageType.Error);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Copy Specific Levels to End"))
            {
                if (sourceVariant == null)
                {
                    EditorUtility.DisplayDialog("Error", "Source variant is not set!", "OK");
                    return;
                }

                if (targetVariant == null)
                {
                    EditorUtility.DisplayDialog("Error", "Target variant is not set!", "OK");
                    return;
                }

                CopySpecificLevels();
                EditorUtility.DisplayDialog("Success", "Specific levels copied to the end.", "OK");
            }
        }

        private void CreateShiftedVariant()
        {
            CreateNormalLevelsShifted();
            CreateSpecialLevelsShifted();
            targetVariant.SetLevels(abTestTypeName);
            EditorUtility.SetDirty(targetVariant);
            AssetDatabase.SaveAssetIfDirty(targetVariant);
            AssetDatabase.Refresh();
        }

        private void CreateSpecialLevelsShifted()
        {
            List<LevelDataSO> levelDataSOs = sourceVariant.GetAllSpecialLevels();
            int level = 1;
            for (int i = 0; i < levelDataSOs.Count; i++)
            {
                LevelDataSO levelDataSO = levelDataSOs[i];
                if (specailLevelsToExclude.Contains(levelDataSO.level))
                {
                    continue;
                }
                int targetLevel = level;
                string assetPath = Editor.ResourcesConstants.LEVELS_PATH + abTestTypeName.ToString() + "/Special Levels/Level-" + targetLevel + ".asset";
                LevelDataSO newLevelDataSO = GetOrCreateLevelDataSOAtPath(assetPath);
                levelDataSO.CloneTo(newLevelDataSO);
                newLevelDataSO.level = targetLevel;
                EditorUtility.SetDirty(newLevelDataSO);
                AssetDatabase.SaveAssetIfDirty(newLevelDataSO);
                level++;
            }
        }

        private void CreateNormalLevelsShifted()
        {
            List<LevelDataSO> levelDataSOs = sourceVariant.GetAllNormalLevels();
            int level = 1;
            for (int i = 0; i < levelDataSOs.Count; i++)
            {
                LevelDataSO levelDataSO = levelDataSOs[i];
                if (normalLevelsToExclude.Contains(levelDataSO.level))
                {
                    continue;
                }
                int targetLevel = level;
                string assetPath = Editor.ResourcesConstants.LEVELS_PATH + abTestTypeName.ToString() + "/Levels/Level-" + targetLevel + ".asset";
                LevelDataSO newLevelDataSO = GetOrCreateLevelDataSOAtPath(assetPath);
                levelDataSO.CloneTo(newLevelDataSO);
                newLevelDataSO.level = targetLevel;
                EditorUtility.SetDirty(newLevelDataSO);
                AssetDatabase.SaveAssetIfDirty(newLevelDataSO);
                level++;
            }
        }

        private void CopySpecificLevels()
        {
            string normalLevelsDirectory = Editor.ResourcesConstants.LEVELS_PATH + abTestTypeName.ToString() + "/Levels/";
            string specialLevelsDirectory = Editor.ResourcesConstants.LEVELS_PATH + abTestTypeName.ToString() + "/Special Levels/";

            int nextNormalLevelNumber = FindMaxLevelNumberInDirectory(normalLevelsDirectory) + 1;
            int nextSpecialLevelNumber = FindMaxLevelNumberInDirectory(specialLevelsDirectory) + 1;

            Dictionary<int, LevelDataSO> sourceNormalDict = sourceVariant.GetAllNormalLevels().ToDictionary(lvl => lvl.level);
            Dictionary<int, LevelDataSO> sourceSpecialDict = sourceVariant.GetAllSpecialLevels().ToDictionary(lvl => lvl.level);

            foreach (int levelToCopy in normalLevelsToCopyToTarget.OrderBy(l => l))
            {
                if (sourceNormalDict.TryGetValue(levelToCopy, out LevelDataSO sourceLevelSO))
                {
                    string assetPath = normalLevelsDirectory + "Level-" + nextNormalLevelNumber + ".asset";
                    LevelDataSO newLevelDataSO = GetOrCreateLevelDataSOAtPath(assetPath);
                    sourceLevelSO.CloneTo(newLevelDataSO);
                    newLevelDataSO.level = nextNormalLevelNumber;
                    EditorUtility.SetDirty(newLevelDataSO);
                    AssetDatabase.SaveAssetIfDirty(newLevelDataSO);
                    nextNormalLevelNumber++;
                }
                else
                {
                    Debug.LogWarning($"Normal level {levelToCopy} not found in source variant {sourceVariant.name}. Skipping.");
                }
            }

            foreach (int levelToCopy in specialLevelsToCopyToTarget.OrderBy(l => l))
            {
                if (sourceSpecialDict.TryGetValue(levelToCopy, out LevelDataSO sourceLevelSO))
                {
                    string assetPath = specialLevelsDirectory + "Level-" + nextSpecialLevelNumber + ".asset";
                    LevelDataSO newLevelDataSO = GetOrCreateLevelDataSOAtPath(assetPath);
                    sourceLevelSO.CloneTo(newLevelDataSO);
                    newLevelDataSO.level = nextSpecialLevelNumber;
                    EditorUtility.SetDirty(newLevelDataSO);
                    AssetDatabase.SaveAssetIfDirty(newLevelDataSO);
                    nextSpecialLevelNumber++;
                }
                else
                {
                    Debug.LogWarning($"Special level {levelToCopy} not found in source variant {sourceVariant.name}. Skipping.");
                }
            }

            targetVariant.SetLevels(abTestTypeName);
            EditorUtility.SetDirty(targetVariant);
            AssetDatabase.SaveAssetIfDirty(targetVariant);
            AssetDatabase.Refresh();
        }


        private int FindMaxLevelNumberInDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return 0;
            }

            int maxLevel = 0;
            string[] guids = AssetDatabase.FindAssets("t:LevelDataSO", new[] { directoryPath });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string filename = Path.GetFileNameWithoutExtension(path); // e.g., "Level-123"

                if (filename.StartsWith("Level-"))
                {
                    string levelNumberStr = filename.Substring("Level-".Length);
                    if (int.TryParse(levelNumberStr, out int level))
                    {
                        maxLevel = Mathf.Max(maxLevel, level);
                    }
                }
            }
            return maxLevel;
        }


        public static LevelDataSO GetOrCreateLevelDataSOAtPath(string assetPath)
        {
            LevelDataSO existingLevelData = AssetDatabase.LoadAssetAtPath<LevelDataSO>(assetPath);

            if (existingLevelData != null)
            {
                return existingLevelData;
            }
            else
            {
                string directoryPath = Path.GetDirectoryName(assetPath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    AssetDatabase.Refresh();
                }
                LevelDataSO newInstance = ScriptableObject.CreateInstance<LevelDataSO>();
                AssetDatabase.CreateAsset(newInstance, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return newInstance;

            }
        }
    }
}