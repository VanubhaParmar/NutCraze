using GameAnalyticsSDK;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Tag.NutSort
{
    public class LevelProgressManager : SerializedManager<LevelProgressManager>
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [ShowInInspector] private LevelSaveData levelSaveData;
        private Timer currentLevelTimer;
        #endregion

        #region PROPERTIES
        public bool IsLevelProgressDataExist => levelSaveData != null;
        public int CurrentLevel => levelSaveData?.level ?? -1;
        public LevelType CurrentLevelType => levelSaveData?.levelType ?? LevelType.NORMAL_LEVEL;
        public LevelSaveData LevelSaveData => levelSaveData;
        public bool IsAnyMoveDone => levelSaveData?.moves.Count > 0;
        public ScrewArrangementConfig ArrangementConfig => levelSaveData.arrangementConfig;

        #endregion

        #region UNITY_CALLBACKS

        public override void Awake()
        {
            base.Awake();
            currentLevelTimer = new Timer();
            LoadSaveData();
            BoosterManager.RegisterOnBoosterUse(OnBoosterUse);
        }


        public override void OnDestroy()
        {
            SaveData();//timer data will be saved in level save data
            currentLevelTimer.StopTimer();
            BoosterManager.DeRegisterOnBoosterUse(OnBoosterUse);
            base.OnDestroy();
        }

        #endregion

        #region PUBLIC_METHODS
        public LevelSaveData CreateAndSaveInitialLevelProgress(LevelDataSO levelData)
        {
            levelSaveData = new LevelSaveData(levelData);
            return levelSaveData;
        }

        public void StartLevelTimer()
        {
            currentLevelTimer.StartTimer(LevelSaveData.runTime);
        }

        public void StopLevelTimer()
        {
            SaveData();
            currentLevelTimer.StopTimer();
        }

        public void PauseLevelTimer()
        {
            currentLevelTimer.PauseTimer();
            SaveData();
        }

        public void ResumeLevelTimer()
        {
            currentLevelTimer.ResumeTimer();
            SaveData();
        }

        public void SaveScrewData(GridCellId gridCellId, ScrewConfig screwConfig)
        {
            if (levelSaveData == null)
                return;
            string key = gridCellId.ToString();
            if (!levelSaveData.screws.ContainsKey(key))
                levelSaveData.screws.Add(key, screwConfig);
            else
                levelSaveData.screws[key] = screwConfig;
            SaveData();
        }

        public void RemoveScrewData(GridCellId gridCellId)
        {
            if (levelSaveData == null)
                return;
            string key = gridCellId.ToString();
            if (levelSaveData.screws.ContainsKey(key))
                levelSaveData.screws.Remove(key);
            SaveData();
        }

        public void ClearAllScrewData()
        {
            if (levelSaveData == null)
                return;
            levelSaveData.screws.Clear();
            SaveData();
        }

        public void OnBoosterUse(int boosterId)
        {
            if (levelSaveData == null)
                return;

            bool requiresSave = false;

            if (!levelSaveData.boostersUsed.ContainsKey(boosterId))
            {
                levelSaveData.boostersUsed.Add(boosterId, 1);
                requiresSave = true;
            }
            else
            {
                levelSaveData.boostersUsed[boosterId]++;
                requiresSave = true;
            }

            if (requiresSave)
                SaveData();
        }

        public void OnWatchAdSuccess()
        {
            if (levelSaveData == null) return;
            levelSaveData.adWatched++;
            SaveData();
        }

        public void OnPlayerMoveConfirmed(MoveData move)
        {
            if (levelSaveData == null)
                return;

            if (levelSaveData.moves == null)
                levelSaveData.moves = new List<MoveData>();

            levelSaveData.moves.Add(move);
            SaveData();
        }

        public MoveData PopLastMove()
        {
            MoveData moveData = levelSaveData.moves.PopLast();
            SaveData();
            return moveData;
        }

        public MoveData PeekLastMove()
        {
            if (levelSaveData.moves.Count <= 0)
                return null;
            return levelSaveData.moves.Last();
        }

        public void ResetLevelProgress()
        {
            levelSaveData = null;
            SaveData();
        }

        public void LogLevelOverEvents()
        {
            if (levelSaveData == null) return;

            Dictionary<int, int> boosterUseData = levelSaveData.boostersUsed;
            LevelType levelType = levelSaveData.levelType;
            int level = levelSaveData.level;

            if (boosterUseData != null)
            {
                foreach (var item in boosterUseData)
                {
                    BaseBooster baseBooster = BoosterManager.Instance.GetBooster(item.Key);
                    if (baseBooster != null)
                    {
                        string boosteName = baseBooster.BoosterName;
                        string eventData = $"{boosteName} Used : {levelType.ToString()} : {level.ToString()} : {item.Value.ToString()}";
                        AnalyticsManager.Instance.LogEvent(eventData);
                    }
                }
            }

            if (levelSaveData.adWatched > 0)
            {
                string eventName = $"RewardAdWatch : {levelType.ToString()} : {level.ToString()} : {levelSaveData.adWatched.ToString()}";
                AnalyticsManager.Instance.LogEvent(eventName);
            }
            if (levelType == LevelType.NORMAL_LEVEL)
            {
                UnityEngine.Debug.Log($"Level Completed : {level} : {levelSaveData.runTime}");
                AdjustManager.Instance.Adjust_LevelCompleteEvent(DataManager.PlayerLevel, levelSaveData.runTime);
                AnalyticsManager.Instance.LogLevelDataEvent(AnalyticsConstants.LevelData_EndTrigger);
                AnalyticsManager.Instance.LogProgressionEvent(GAProgressionStatus.Complete);
            }
            else
            {
                AnalyticsManager.Instance.LogSpecialLevelDataEvent(AnalyticsConstants.LevelData_EndTrigger, level);
            }
        }

        public void UpdateMovesAfterGridChange(Dictionary<GridCellId, GridCellId> gridCellMapping)
        {
            List<MoveData> moves = levelSaveData.moves;
            for (int i = 0; i < moves.Count; i++)
            {
                GridCellId fromKey = moves[i].fromScrew;
                GridCellId toKey = moves[i].toScrew;
                if (gridCellMapping.ContainsKey(fromKey))
                {
                    moves[i].fromScrew = gridCellMapping[fromKey];
                }
                if (gridCellMapping.ContainsKey(toKey))
                {
                    moves[i].toScrew = gridCellMapping[toKey];
                }
            }
        }
        #endregion

        #region PRIVATE_METHODS
        private void SaveData()
        {
            if (levelSaveData != null)
                levelSaveData.runTime = currentLevelTimer.ElapsedTimeSeconds;
            PlayerPersistantData.SetPlayerLevelProgressData(levelSaveData);
        }

        public void LoadSaveData()
        {
            levelSaveData = PlayerPersistantData.GetPlayerLevelProgressData();
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