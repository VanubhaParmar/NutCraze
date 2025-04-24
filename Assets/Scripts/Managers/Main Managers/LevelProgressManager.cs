using GameAnalyticsSDK;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class LevelProgressManager : SerializedManager<LevelProgressManager>
    {
        #region PRIVATE_VARIABLES
        private LevelSaveData levelSaveData;
        private const int Store_Gameplay_Data_Every_X_Seconds = 5;
        #endregion

        #region PUBLIC_VARIABLES
        #endregion

        #region PROPERTIES
        public bool IsLevelProgressDataExist => levelSaveData != null;
        public int CurrentLevel => levelSaveData?.level ?? -1;
        public LevelType CurrentLevelType => levelSaveData?.levelType ?? LevelType.NORMAL_LEVEL; // Default
        public int CurrentLevelStageIndex => /*levelSaveData?.currentStage ??*/ 0;
        public ABTestType CurrentABTestType => levelSaveData?.aBTestType ?? default(ABTestType); // Provide default or handle null
        public LevelSaveData LevelSaveData => levelSaveData;
        #endregion

        #region UNITY_CALLBACKS

        public override void Awake()
        {
            base.Awake();
            LoadSaveData();
        }

        private void Start()
        {
            NutTransferHelper.Instance.RegisterOnNutTransferComplete(OnNutTransferComplete);
            LevelManager.Instance.RegisterOnLevelLoad(OnLevelLoad);
            LevelManager.Instance.RegisterOnLevelComplete(OnLevelComplete);
            BoosterManager.RegisterOnBoosterUse(OnBoosterUse);
        }

        public override void OnDestroy()
        {
            NutTransferHelper.Instance.DeRegisterOnNutTransferComplete(OnNutTransferComplete);
            LevelManager.Instance.DeRegisterOnLevelLoad(OnLevelLoad);
            LevelManager.Instance.DeRegisterOnLevelComplete(OnLevelComplete);
            BoosterManager.DeRegisterOnBoosterUse(OnBoosterUse);
            base.OnDestroy();
        }

        #endregion

        #region PUBLIC_METHODS
        public void OnBoosterUse(int boosterId)
        {
            if (levelSaveData == null)
                return;

            if (boosterId == BoosterIdConstant.UNDO)
            {
                if (levelSaveData.moves.Length > 0)
                {
                    Array.Resize(ref levelSaveData.moves, levelSaveData.moves.Length - 1);
                }
            }
            else if (boosterId == BoosterIdConstant.EXTRA_SCREW)
            {
                // playerLevelProgressData.boosterScrewCapacityUpgrade++;
            }
            SaveData();
        }

        private void OnLevelLoad()
        {
            TimeManager.Instance.RegisterTimerTickEvent(OnLevelTimerTick);
            AdjustManager.Instance.Adjust_LevelStartEvent(levelSaveData.level, levelSaveData.levelType);
        }

        private void OnLevelComplete()
        {
            LogLevelCompleteEvents();
            TimeManager.Instance.DeRegisterTimerTickEvent(OnLevelTimerTick);
        }

        public void OnLevelTimerTick(DateTime dt)
        {
            if (GameplayManager.Instance.IsPlayingLevel)
            {
                levelSaveData.runTime++;
                if (levelSaveData.runTime % Store_Gameplay_Data_Every_X_Seconds == 0)
                    SaveData();
            }
        }

        private void OnNutTransferComplete(BaseScrew fromScrew, BaseScrew toScrew, int transferredNuts)
        {
            if (levelSaveData.moves == null)
                levelSaveData.moves = new MoveData[0];

            Array.Resize(ref levelSaveData.moves, levelSaveData.moves.Length + 1);
            levelSaveData.moves[levelSaveData.moves.Length - 1] = new MoveData
            {
                fromScrew = fromScrew.Id,
                toScrew = toScrew.Id,
                transferedNuts = transferredNuts
            };
            SaveData();
        }

        public bool RemoveLastMoveData(out MoveData moveData)
        {
            if (levelSaveData.moves == null || levelSaveData.moves.Length == 0)
            {
                moveData = null;
                return false;
            }
            moveData = levelSaveData.moves[levelSaveData.moves.Length - 1];
            Array.Resize(ref levelSaveData.moves, levelSaveData.moves.Length - 1);
            SaveData();
            return true;
        }

        public bool IsAnyNutMoved()
        {
            return IsLevelProgressDataExist && levelSaveData.moves?.Length > 0;
        }

        public void ResetLevelProgress()
        {
            levelSaveData = null;
            SaveData();
        }
        #endregion

        #region PRIVATE_METHODS
        private void LogLevelCompleteEvents()
        {
            AnalyticsManager.Instance.LogLevelDataEvent(AnalyticsConstants.LevelData_EndTrigger);
            AnalyticsManager.Instance.LogProgressionEvent(GAProgressionStatus.Complete);
            AdjustManager.Instance.Adjust_LevelCompleteEvent(DataManager.PlayerLevel, levelSaveData.runTime);
        }
        private void LoadSaveData()
        {
            levelSaveData = PlayerPersistantData.GetLevelSaveData();
        }

        private void SaveData()
        {
            PlayerPersistantData.SetLevelSaveData(levelSaveData);
        }
        #endregion

        #region PUBLIC_METHODS: State Management
        public LevelSaveData CreateAndSaveInitialLevelProgress(LevelData levelData, ABTestType abTestType)
        {
            levelSaveData = new LevelSaveData(abTestType, levelData);

            AdjustManager.Instance.Adjust_LevelStartEvent(levelSaveData.level, levelSaveData.levelType);
            AnalyticsManager.Instance.LogProgressionEvent(GAProgressionStatus.Start);
            return levelSaveData;
        }
        public void AdvanceLevelStage()
        {
            if (!IsLevelProgressDataExist)
                return;
            //levelSaveData.currentStage++;
            SaveData();
        }

        public void AdvanceScrewStage(int screwId)
        {
            if (!IsLevelProgressDataExist) return;

            ScrewConfig screwConfig = FindScrewSaveConfig(screwId);
            if (screwConfig != null)
            {
                //screwConfig.currentStage++;
                Debug.Log($"Advanced Screw {screwId} to Stage {screwConfig.currentStage} in LevelSaveData");
                SaveData(); 
            }
            else
            {
                Debug.LogError($"Could not find ScrewSaveConfig with ID {screwId} to advance stage in LevelSaveData!");
            }
        }

        public void UpdateScrewNutsInSaveData(int screwId, List<BaseNut> currentNuts)
        {
            //if (!IsLevelProgressDataExist) 
            //    return;

            //ScrewConfig screwConfig = FindScrewSaveConfig(screwId);
            //if (screwConfig != null)
            //{
            //    if (screwConfig.currentStage < 0 || screwConfig.currentStage >= screwConfig.pendingStages.Length)
            //    {
            //        Debug.LogError($"Cannot update nuts for Screw {screwId}: Invalid screw stage index {screwConfig.currentStage} in save data.");
            //        return;
            //    }

            //    ScrewStageConfig stageSaveConfig = screwConfig.pendingStages[screwConfig.currentStage];

            //    stageSaveConfig.nutDatas = currentNuts
            //        .Select(nut => new NutConfig { nutType = nut.NutType, nutColorTypeId = nut.GetNutColorType() })
            //        .ToArray();

            //    SaveData();
            //}
            //else
            //{
            //    Debug.LogWarning($"Could not find ScrewSaveConfig with ID {screwId} to update nuts in LevelSaveData!");
            //}
        }

        #endregion

        #region PUBLIC_METHODS: Event Handlers & Gameplay Loop
        private void OnNewLevelStageLoad()
        {
            if (!IsLevelProgressDataExist) return;
            // The stage index is already updated in SaveData via AdvanceLevelStage.
            // No specific action needed here unless resetting stage-specific timers/counters.
            Debug.Log($"Level Progress Manager acknowledged new stage load: Stage {CurrentLevelStageIndex}");
        }

        #endregion

        #region PRIVATE_METHODS: Saving, Loading, Helpers
        // Helper to find the ScrewSaveConfig within the current stage's save data
        private ScrewConfig FindScrewSaveConfig(int screwId)
        {
            //if (!IsLevelProgressDataExist) return null;

            //int stageIdx = levelSaveData.currentStage;
            //if (levelSaveData.pendingLevelStage == null || stageIdx < 0 || stageIdx >= levelSaveData.pendingLevelStage.Length)
            //{
            //    Debug.LogError($"Invalid current stage index ({stageIdx}) or null stage data in LevelSaveData.");
            //    return null;
            //}

            //LevelStageConfig stageSave = levelSaveData.pendingLevelStage[stageIdx];
            //if (stageSave.screwConfigs == null)
            //{
            //    Debug.LogError($"screwSaveConfigs is null for stage {stageIdx}.");
            //    return null;
            //}


            //// Use Linq's FirstOrDefault for cleaner search
            //ScrewConfig config = stageSave.screwConfigs.FirstOrDefault(sc => sc.id == screwId);

            //// Original loop version (for reference):
            //// foreach (var screwConfig in stageSave.screwSaveConfigs)
            //// {
            ////     if (screwConfig.id == screwId)
            ////     {
            ////         return screwConfig;
            ////     }
            //// }

            //if (config == null)
            //{
            //    // This might happen legitimately if a screw is only present in later stages
            //    // Debug.LogWarning($"ScrewSaveConfig with ID {screwId} not found in current stage ({stageIdx}).");
            //}

            //return config;
            return null;
        }

        // Helper to get the NutSaveConfig array for a specific screw in its current stage
        public NutConfig[] GetScrewNutsFromSaveData(int screwId)
        {
            //ScrewConfig screwConfig = FindScrewSaveConfig(screwId);
            //if (screwConfig != null && screwConfig.currentStage >= 0 && screwConfig.currentStage < screwConfig.pendingStages.Length)
            //{
            //    return screwConfig.pendingStages[screwConfig.currentStage]?.nutDatas;
            //}
            return null; // Return null if not found or invalid state
        }


        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}