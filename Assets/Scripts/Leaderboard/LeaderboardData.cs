using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "LeaderboardData", menuName = Constant.GAME_NAME + "/Leaderboard/Leaderboard Data")]
    public class LeaderboardData : SerializedScriptableObject
    {
        #region PUBLIC_VARIABLES
        public int startAtLevel;

        [Space]
        public DayOfWeek startDay;
        public int leaderboardRunTimeInDays;
        public int minimumTargetLevel = 10;

        [Space]
        public StringListDataSO botNamesList;
        public StringListDataSO botsMultiplierList;
        public int numberOfTotalParticipants;
        public Vector2Int botUpdateCountRange;
        public Vector2Int randomSeedRange;

        [Space]
        public List<RewardsDataSO> leaderboardRankRewards;
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public RewardsDataSO GetRankReward(int rank)
        {
            if (rank > 0 && rank <= leaderboardRankRewards.Count)
                return leaderboardRankRewards[rank - 1];
            return null;
        }

        public float GetBotMultiplierAtIndex(int index)
        {
            int finalIndex = Mathf.Clamp(index, 0, botsMultiplierList.data.Count - 1);
            return float.Parse(botsMultiplierList.data[finalIndex]);
        }
        #endregion

        #region PRIVATE_METHODS
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
#if UNITY_EDITOR
        [Space, Header("TST")]
        public string eventStartTimeString;
        public string eventEndTimeString;
        public string currentTimeString;
        public float targetMultiplier;
        public int targetScore;
        public int randomSeed;

        [Button]
        public void Editor_GetPoints()
        {
            Debug.Log(GetCurrentPoints());
        }

        [Button]
        public void Editor_SetCurrentTime()
        {
            currentTimeString = TimeManager.Now.GetPlayerPrefsSaveString();
        }

        public int GetCurrentPoints()
        {
            int botFinalScore = Mathf.FloorToInt(targetMultiplier * targetScore);

            currentTimeString.TryParseDateTime(out DateTime currentTime);
            eventStartTimeString.TryParseDateTime(out DateTime eventStartTime);
            eventEndTimeString.TryParseDateTime(out DateTime eventEndTime);

            // If event hasn't started
            if (currentTime < eventStartTime)
                return 0;

            // If event has ended, return final score
            if (currentTime >= eventEndTime)
                return botFinalScore;

            Debug.Log("Final Score :> " + botFinalScore);

            // Initialize random with seed for this calculation
            Random.InitState(randomSeed);

            // Generate random update times (in hours from start)
            float totalEventHours = (float)(eventEndTime - eventStartTime).TotalHours;
            int numberOfUpdates = Random.Range(botUpdateCountRange.x, botUpdateCountRange.y); // x-y updates during the event

            List<float> updateHours = new List<float>();
            for (int i = 0; i < numberOfUpdates; i++)
            {
                updateHours.Add(Random.Range(0, totalEventHours));
            }
            updateHours.Sort();

            Debug.Log($"Update Hours >>> \n {updateHours.PrintList()}");

            // Generate random scores for each update time
            List<int> updateScores = new List<int>();
            for (int i = 0; i < numberOfUpdates - 1; i++)
            {
                updateScores.Add(Random.Range(0, botFinalScore));
            }
            updateScores.Add(botFinalScore); // Ensure final update reaches target score
            updateScores.Sort();

            Debug.Log($"Update Scores >>> \n {updateScores.PrintList()}");

            // Find current score based on elapsed time
            float elapsedHours = (float)(currentTime - eventStartTime).TotalHours;
            int currentScorePoints = 0;

            Debug.Log("Elapsed : " + elapsedHours);

            for (int i = 0; i < updateHours.Count; i++)
            {
                if (elapsedHours >= updateHours[i])
                {
                    currentScorePoints = updateScores[i];
                }
                else
                {
                    break;
                }
            }

            // Reset random seed
            Random.InitState(Utility.GetNewRandomSeed());

            return currentScorePoints;
        }
#endif
        #endregion
    }
}