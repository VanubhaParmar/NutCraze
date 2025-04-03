using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tag.NutSort.Editor
{
    public class LevelVariantShifter : EditorWindow
    {
        private LevelVariantSO sourceVariant;
        private LevelVariantSO targetVariant;
        private List<int> normalLevelsToExclude = new List<int> { 30, 45, 95, 106 };
        private List<int> specailLevelsToExclude = new List<int>();
        private ABTestType abTestTypeName = ABTestType.AB3;

        [MenuItem("Tools/Level Variant Shifter")]
        public static void ShowWindow()
        {
            GetWindow<LevelVariantShifter>("Level Variant Shifter");
        }

        private void OnGUI()
        {
            GUILayout.Label("Level Variant Shifter", EditorStyles.boldLabel);

            sourceVariant = EditorGUILayout.ObjectField("Source Variant", sourceVariant, typeof(LevelVariantSO), false) as LevelVariantSO;
            targetVariant = EditorGUILayout.ObjectField("Target Variant", targetVariant, typeof(LevelVariantSO), false) as LevelVariantSO;

            EditorGUILayout.LabelField("Normal Levels to remove (comma separated):");
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
                EditorGUILayout.HelpBox("Invalid level format. Use comma-separated numbers.", MessageType.Error);
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Special Levels to remove (comma separated):");
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
                EditorGUILayout.HelpBox("Invalid level format. Use comma-separated numbers.", MessageType.Error);
            }

            EditorGUILayout.Space();


            if (GUILayout.Button("Create Shifted Variant"))
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
            }
        }

        private void CreateShiftedVariant()
        {
            CreateNormalLevels();
            CreateSpecialLevels();
            targetVariant.SetLevels(abTestTypeName);
            EditorUtility.SetDirty(targetVariant);
            AssetDatabase.SaveAssetIfDirty(targetVariant);
        }

        private void CreateSpecialLevels()
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

        private void CreateNormalLevels()
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