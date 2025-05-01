using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "ScrewArrangementsDataSO", menuName = Constant.GAME_NAME + "/Level Data/ScrewArrangementsDataSO")]
    public class ScrewArrangementsDataSO : SerializedScriptableObject
    {
        #region PUBLIC_VARIABLES
        [SerializeField] private Dictionary<int, ScrewArrangementConfigSO> arrangementConfigMapping = new Dictionary<int, ScrewArrangementConfigSO>();
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public ScrewArrangementConfigSO GetScrewArrangementConfigSO(int arrangementId)
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

            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ScrewArrangementConfigSO", new[] { folderPath });

            foreach (string guid in guids)
            {
                string configPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                ScrewArrangementConfigSO config = UnityEditor.AssetDatabase.LoadAssetAtPath<ScrewArrangementConfigSO>(configPath);

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