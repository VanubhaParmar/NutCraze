using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace com.tag.nut_sort {
    public class LeaderBoardBotPlayer : BaseLeaderBoardPlayer
    {
        #region PUBLIC_VARS
        #endregion

        #region PRIVATE_VARS
        protected int randomSeed;
        protected float targetMultiplier;
        #endregion

        #region KEY
        #endregion

        #region Propertices
        #endregion

        #region Overrided_Method
        public void InitSeedData(int randomSeed, float targetMultiplier)
        {
            this.randomSeed = randomSeed;
            this.targetMultiplier = targetMultiplier;
        }

        public override int GetCurrentPoints()
        {
            int targetScore = LeaderboardManager.Instance.GetBotTargetScore();
            int botFinalScore = Mathf.FloorToInt(targetMultiplier * targetScore);

            DateTime currentTime = TimeManager.Now;
            DateTime eventStartTime = LeaderboardManager.Instance.GetRecentLeaderboardEventStartTime();
            DateTime eventEndTime = LeaderboardManager.Instance.GetRecentLeaderboardEventEndTime();

            // If event hasn't started
            if (currentTime < eventStartTime)
                return 0;

            // If event has ended, return final score
            if (currentTime >= eventEndTime)
                return botFinalScore;

            // Initialize random with seed for this calculation
            Random.InitState(randomSeed);

            // Generate random update times (in hours from start)
            float totalEventHours = (float)(eventEndTime - eventStartTime).TotalHours;
            int numberOfUpdates = Random.Range(LeaderboardManager.Instance.LeaderboardData.botUpdateCountRange.x, LeaderboardManager.Instance.LeaderboardData.botUpdateCountRange.y); // x-y updates during the event
            
            List<float> updateHours = new List<float>();
            for (int i = 0; i < numberOfUpdates; i++)
            {
                updateHours.Add(Random.Range(0, totalEventHours));
            }
            updateHours.Sort();

            // Generate random scores for each update time
            List<int> updateScores = new List<int>();
            for (int i = 0; i < numberOfUpdates - 1; i++)
            {
                updateScores.Add(Random.Range(0, botFinalScore));
            }
            updateScores.Add(botFinalScore); // Ensure final update reaches target score
            updateScores.Sort();

            // Find current score based on elapsed time
            float elapsedHours = (float)(currentTime - eventStartTime).TotalHours;
            int currentScorePoints = 0;

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
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_FUNCTIONS
        #endregion

        #region PRIVATE_FUNCTIONS
        #endregion

        #region CO-ROUTINES
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}