using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "LevelArrangementsDataSO", menuName = Constant.GAME_NAME + "/Level Data/LevelArrangementsListDataSO")]
    public class LevelArrangementsListDataSO : SerializedScriptableObject
    {
        #region PUBLIC_VARIABLES
        [SerializeField] private Dictionary<int, LevelArrangementConfigDataSO> arrangementConfigMapping = new Dictionary<int, LevelArrangementConfigDataSO>();
        private List<LevelArrangementConfigDataSO> levelArrangementConfigDataSOs;
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        public List<LevelArrangementConfigDataSO> LevelArrangementConfigDataSOs
        {
            get
            {
                if (levelArrangementConfigDataSOs == null || levelArrangementConfigDataSOs.Count == 0)
                    levelArrangementConfigDataSOs = arrangementConfigMapping.Values.ToList();
                return levelArrangementConfigDataSOs;
            }
        }
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public LevelArrangementConfigDataSO GetLevelArrangementConfig(int arrangementId)
        {
            if (arrangementConfigMapping.ContainsKey(arrangementId))
                return arrangementConfigMapping[arrangementId];
            return null;
        }

        public List<int> GetAllArrangementIds()
        {
            return arrangementConfigMapping.Keys.ToList();
        }
        #endregion

        #region PRIVATE_METHODS
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion

        #region EDITOR
#if UNITY_EDITOR
        [Button]
        public void AddArrangementConfigFromAssetPath()
        {
            arrangementConfigMapping.Clear();

            string thisSOPath = UnityEditor.AssetDatabase.GetAssetPath(this);
            string folderPath = System.IO.Path.GetDirectoryName(thisSOPath);

            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogError("Could not determine the folder path for this scriptable object");
                return;
            }

            Debug.Log($"Loading arrangement configs from: {folderPath}");

            arrangementConfigMapping.Clear();
            levelArrangementConfigDataSOs = null;

            // Find all assets of type LevelArrangementConfigDataSO in the same folder
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:LevelArrangementConfigDataSO", new[] { folderPath });

            foreach (string guid in guids)
            {
                string configPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                LevelArrangementConfigDataSO config = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelArrangementConfigDataSO>(configPath);

                if (config != null)
                {
                    if (!arrangementConfigMapping.ContainsKey(config.ArrangementId))
                    {
                        arrangementConfigMapping.Add(config.ArrangementId, config);
                        Debug.Log($"Added configuration: {config.name} with ID {config.ArrangementId}");
                    }
                    else
                    {
                        Debug.LogWarning($"Duplicate ArrangementId found: {config.ArrangementId} in {configPath}");
                    }
                }
            }

            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log($"Added {arrangementConfigMapping.Count} level arrangement configurations");
        }
#endif
        #endregion
    }
}