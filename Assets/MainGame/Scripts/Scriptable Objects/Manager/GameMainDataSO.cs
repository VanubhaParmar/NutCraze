using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "GameMainDataSO", menuName = Constant.GAME_NAME + "/Managers/GameMainDataSO")]
    public class GameMainDataSO : SerializedScriptableObject
    {
        #region PUBLIC_VARIABLES
        [SerializeField] private int levelFailReviveCoinCost;
        public int undoBoostersCountToAddOnAdWatch = 5;
        public int extraScrewBoostersCountToAddOnAdWatch = 2;

        [Space]
        public int repeatLastLevelsCountAfterGameFinish = 50;

        [Space]
        public BaseReward levelCompleteReward;

        [Space]
        public string playStoreLink;
        public string privacyPolicyLink;
        public string termsLink;

        [Space]
        public List<int> showRateUsAtLevels;

        [Space]
        public int totalLevelsInBuild; // Set by pre-processing build
        #endregion

        #region PRIVATE_VARIABLES
        private const string RandomLevelGenerationSeedPrefsKey = "RandomLevelGenerationSeedPrefs";
        private const string LastGenerationSeedLevelNumberPrefsKey = "LastGenerationSeedLevelNumberPrefs";
        #endregion

        #region PROPERTIES
        private int RandomLevelsGenerationSeed
        {
            get { return PlayerPrefbsHelper.GetInt(RandomLevelGenerationSeedPrefsKey, Utility.GetNewRandomSeed()); }
            set { PlayerPrefbsHelper.SetInt(RandomLevelGenerationSeedPrefsKey, value); }
        }

        private int LastGenerationSeedLevelNumber
        {
            get { return PlayerPrefbsHelper.GetInt(LastGenerationSeedLevelNumberPrefsKey, 0); }
            set { PlayerPrefbsHelper.SetInt(LastGenerationSeedLevelNumberPrefsKey, value); }
        }

        public int LevelFailReviveCoinCost { get => levelFailReviveCoinCost; }
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void InitializeDataSO()
        {
#if UNITY_EDITOR
            Editor_SetTotalLevels();
#endif
        }

        public int GetCappedLevel(int currentLevel)
        {
            if (currentLevel > totalLevelsInBuild)
            {

                int index = (currentLevel - totalLevelsInBuild) % repeatLastLevelsCountAfterGameFinish;
                
                if ((index == 0 && LastGenerationSeedLevelNumber != currentLevel) || LastGenerationSeedLevelNumber == 0)
                {
                    RandomLevelsGenerationSeed = Utility.GetNewRandomSeed();
                    LastGenerationSeedLevelNumber = currentLevel;
                    Debug.Log("<color=red>Set New Seed : " + RandomLevelsGenerationSeed + " " + LastGenerationSeedLevelNumber + "</color>");
                }

                return GetCappedRandomLevel(index);
            }

            return currentLevel;
        }

        public bool CanShowRateUsPopUp()
        {
            return showRateUsAtLevels.Contains(DataManager.PlayerLevel.Value - 1);
        }
        #endregion

        #region PRIVATE_METHODS
        private int GetCappedRandomLevel(int index)
        {
            int randomSeed = RandomLevelsGenerationSeed;

            Debug.Log("<color=red>Set Seed : " + randomSeed + "</color>");
            Random.InitState(randomSeed);

            List<int> levels = Enumerable.Range(totalLevelsInBuild - repeatLastLevelsCountAfterGameFinish + 1, repeatLastLevelsCountAfterGameFinish).ToList();
            levels.Shuffle();
            int randomLevel = index >= 0 && index < levels.Count ? levels[index] : levels.GetRandomItemFromList();

            Random.InitState(Utility.GetNewRandomSeed());
            return randomLevel;
        }

        [Button]
        public void TST_GetCappedLevel(int startLevel, int count)
        {
            List<int> ints = new List<int>();
            string levelString = "";
            for (int i = 0; i < count; i++)
            {
                int level = GetCappedLevel(startLevel + i);

                ints.Add(level);
                levelString += level + " ";
            }

            ints.Sort();
            string sortedString = "";
            ints.ForEach(x => sortedString += x + " ");

            Debug.Log(levelString);
            Debug.Log(sortedString);
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion

        #region UNITY_EDITOR_FUNCTIONS
#if UNITY_EDITOR
        public int GetTotalNumberOfLevels(LevelType levelType = LevelType.NORMAL_LEVEL)
        {
            string levelPath = GetLevelsPath(levelType);

            string directoryPath = Application.dataPath + ResourcesConstants.MAIN_RESOURCE_PATH_FROM_PERSISTANT_PATH + levelPath;
            string[] files = System.IO.Directory.GetFiles(directoryPath);

            int levelNumber = 0;

            if (files.Length == 0)
                return 0;

            for (int i = 0; i < files.Length; i++)
            {
                string fileLevelNumber = files[i].Split("/").ToList().GetLastItemFromList().Remove(".asset").Remove(".meta");
                string finalSub = fileLevelNumber.Substring(ResourcesConstants.LEVEL_SO_NAME_FORMAT.IndexOf("{0}"));
                if (int.TryParse(finalSub, out int newLevel) && newLevel > levelNumber)
                    levelNumber = newLevel;
            }

            return levelNumber;
        }

        private string GetLevelsPath(LevelType levelType)
        {
            return levelType == LevelType.NORMAL_LEVEL ? ResourcesConstants.LEVELS_PATH : ResourcesConstants.SPECIAL_LEVELS_PATH;
        }

        [Button]
        public void Editor_SetTotalLevels()
        {
            totalLevelsInBuild = GetTotalNumberOfLevels();

            LevelEditorUtility.SetDirty(this);
            LevelEditorUtility.SaveAssets();
            LevelEditorUtility.Refresh();
        }
#endif
        #endregion
    }
}