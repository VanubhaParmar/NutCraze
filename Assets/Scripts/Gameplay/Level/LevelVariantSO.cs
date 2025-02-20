using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "LevelVariant", menuName = Constant.GAME_NAME + "/ABTesting/Level Variant")]
    public class LevelVariantSO : SerializedScriptableObject
    {
        #region PRIVATE_VARIABLES
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
        #endregion

        #region EDITOR
#if UNITY_EDITOR
        [Button]
        private void AddNormalLevels(List<LevelDataSO> levels)
        {
            for (int i = 0; i < levels.Count; i++)
            {
                LevelDataSO levelDataSO = levels[i];
                if (!normalLevels.ContainsKey(levelDataSO.level))
                    normalLevels.Add(levelDataSO.level, levelDataSO);
                else
                    normalLevels[levelDataSO.level] = levelDataSO;
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }

        [Button]
        private void AddSpecialLevels(List<LevelDataSO> levels)
        {
            for (int i = 0; i < levels.Count; i++)
            {
                LevelDataSO levelDataSO = levels[i];
                if (!specialLevels.ContainsKey(levelDataSO.level))
                    specialLevels.Add(levelDataSO.level, levelDataSO);
                else
                    specialLevels[levelDataSO.level] = levelDataSO;
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }

        [Button("Clear Normal Levels")]
        private void ClearNormalLevels()
        {
            if (UnityEditor.EditorUtility.DisplayDialog("Clear Normal Levels",
                "Are you sure you want to clear all normal level paths?", "Yes", "Cancel"))
            {
                normalLevels.Clear();
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }

        [Button("Clear Special Levels")]
        private void ClearSpecialLevels()
        {
            if (UnityEditor.EditorUtility.DisplayDialog("Clear Special Levels",
                "Are you sure you want to clear all special level paths?", "Yes", "Cancel"))
            {
                specialLevels.Clear();
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif
        #endregion
    }
}
