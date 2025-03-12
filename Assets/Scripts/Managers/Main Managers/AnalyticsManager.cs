using Firebase.Analytics;
using GameAnalyticsSDK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class AnalyticsManager : Manager<AnalyticsManager>
    {
        #region PUBLIC_VARIABLES
        public bool isDebugLogEvent;
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private List<GABusinessEventDataMapping> gABusinessEventDataMapping = new List<GABusinessEventDataMapping>();
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        public override void Awake()
        {
            base.Awake();
            OnLoadingDone();
        }
        #endregion

        #region PUBLIC_METHODS
        public void LogEvent_IAPData(string iapId)
        {
            FirebaseManager.Instance.FirebaseAnalytics.Log_Event(AnalyticsConstants.IAP_DATA,
                new Dictionary<string, string>()
                {
                    {"IAPID", iapId},
                });

            GameAnalytics.NewDesignEvent(AnalyticsConstants.IAP_DATA + ":" + iapId);
            DebugLogEvent(AnalyticsConstants.IAP_DATA + ":" + iapId);
        }
        public void LogEvent_NewBusinessEvent(string iSOCurrencyCode, double iapPrice, string id, string recipt)
        {
            //GameAnalytics.NewBusinessEventGooglePlay(iSOCurrencyCode, (int)(iapPrice * 100), GetItemType(id), GetItemID(id), GetCardType(id), recipt, null);
        }
        public void LogEvent_AdGAEvent(GAAdAction action, GAAdType type, string rewardAdShowCallType)
        {
            GameAnalytics.NewAdEvent(action, type, "ApplovinMax", rewardAdShowCallType);
            DebugLogEvent("<color=red>" + action + "___" + type + "___" + "ApplovinMax___" + rewardAdShowCallType + "</color>");
        }
        public void LogEvent_FirebaseAdRevanueEvent(string key, double threshold, double ecpmValue)
        {
            if (ecpmValue <= threshold)
            {
                return;
            }
            double ecpmInDollars = ecpmValue / (1000 * 100);
            DebugLogEvent("LogEvent_AdRevanueEventFirebase: " + key + "_" + ecpmValue + "_" + ecpmInDollars);
            FirebaseManager.Instance.FirebaseAnalytics.LogEvent(key,
                new Parameter[] {
                    new Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterCurrency, "USD"),
                    new Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterValue, ecpmInDollars),
                    new Parameter("revenue", ((int) ecpmValue).ToString()),
                    new Parameter("revenue_cumulative", ecpmInDollars)
            });
        }
        public void LogEvent_FirebaseAdRevenueAppLovin(string platform, string ad_source, string ad_unit_name, string ad_format, double value, string currency)
        {
            string EVENT_NAME = "ad_impression";
            DebugLogEvent("LogEvent_AdRevenueAppLovin: " + ad_source + "  " + ad_unit_name + "  " + ad_format + "  " + value);
            List<Parameter> parameters = new List<Parameter>
            {
                new Parameter("ad_platform", platform),
                new Parameter("ad_source", ad_source),
                new Parameter("ad_unit_name", ad_unit_name),
                new Parameter("ad_format", ad_format),
                new Parameter("value", value),
                new Parameter("currency", currency)
            };
            FirebaseManager.Instance.FirebaseAnalytics.LogEvent(EVENT_NAME, parameters.ToArray());
        }

        public void LogEvent_AdjustS2sInfo(string log)
        {
            DebugLogEvent(AnalyticsConstants.Adjust_Info + ":" + log + ":" + Application.identifier);
            GameAnalytics.NewDesignEvent(AnalyticsConstants.Adjust_Info + ":" + log + ":" + Application.identifier);
        }

        public void LogLevelDataEvent(string levelTriggerType)
        {
            LogEvent("LevelData", "Event", levelTriggerType, "LevelNumber", PlayerPersistantData.GetMainPlayerProgressData().playerGameplayLevel.ToString());
        }

        public void LogLeaderboardRankEvent(int rank)
        {
            LogEvent("Leaderboard_Info", "Rank", rank.ToString());
        }

        public void LogSpecialLevelDataEvent(string levelTriggerType)
        {
            int playerLevel = PlayerPersistantData.GetMainPlayerProgressData().playerGameplayLevel;
            int specialLevelCount = GameManager.Instance.GameMainDataSO.GetSpecialLevelNumberCountToLoad(playerLevel);

            string levelString = specialLevelCount + "_" + (playerLevel - 1);

            LogEvent("SpecialLevelData", "Event", levelTriggerType, "LevelNumber", levelString);
        }

        public void LogAdsDataEvent(string boosterName)
        {
            LogEvent("AdsData", "BoosterType", boosterName, "LevelNumber", PlayerPersistantData.GetMainPlayerProgressData().playerGameplayLevel.ToString());
        }

        public void LogResourceEvent(GAResourceFlowType flowType, string currency, float amount, string itemType, string itemId)
        {
            GameAnalytics.NewResourceEvent(flowType, currency, amount, itemType, itemId);
            DebugLogEvent("<color=brown>-- GA_RESOURCE_EVENT : " + flowType + " " + currency + " " + amount + " " + itemType + " " + itemId + "</color>");
        }

        public void LogProgressionEvent(GAProgressionStatus status)
        {
            string level = PlayerPersistantData.GetMainPlayerProgressData().playerGameplayLevel.ToString();
            GameAnalytics.NewProgressionEvent(status, level);
            DebugLogEvent("<color=lime>-- GA_PROGRESSION_EVENT : " + status + " " + level + "</color>");
        }

        public void LogEvent(string eventName)
        {
            FirebaseManager.Instance.FirebaseAnalytics.Log_Event(eventName);
            GameAnalyticsManager.Instance.Log_Event(GAEventType.Design, eventName);

            DebugLogEvent(eventName);
        }

        public void LogEvent(string eventName, params string[] parameters)
        {
            var gaParams = eventName + ":" + GetCommaSeperatedParams(parameters);
            var kvpParams = GetKVPParams(parameters);

            FirebaseManager.Instance.FirebaseAnalytics.Log_Event(eventName, kvpParams);
            GameAnalyticsManager.Instance.Log_Event(GAEventType.Design, gaParams);

            DebugLogEvent(gaParams);
        }
        #endregion

        #region PRIVATE_METHODS
        private void DebugKVP(Dictionary<string, string> keyValuePairs)
        {
            foreach (var kv in keyValuePairs)
            {
                Debug.Log(kv.Key + ":" + kv.Value);
            }
        }

        private Dictionary<string, string> GetKVPParams(params string[] parameters)
        {
            Dictionary<string, string> firebaseKVPS = new Dictionary<string, string>();
            for (int i = 0; i < parameters.Length; i++)
            {
                firebaseKVPS.Add(parameters[i], parameters[i + 1]);
                i++;
            }

            return firebaseKVPS;
        }

        private string GetCommaSeperatedParams(params string[] parameters)
        {
            string value = "";
            for (int i = 0; i < parameters.Length; i++)
            {
                i++;

                value += parameters[i];
                if (i < parameters.Length - 1)
                    value += ":";
            }

            return value;
        }

        private void DebugLogEvent(string eventName)
        {
            if (isDebugLogEvent)
            {
                Debug.Log("<color=#FFD700>Analytics Event : " + eventName + "</color>");
            }
        }

        private string GetItemType(string productId)
        {
            var mapData = gABusinessEventDataMapping.Find(x => x.productId == productId);
            if (mapData != null)
                return mapData.itemType;
            return "";
        }
        private string GetItemID(string productId)
        {
            var mapData = gABusinessEventDataMapping.Find(x => x.productId == productId);
            if (mapData != null)
                return mapData.itemId;
            return "";
        }
        private string GetCardType(string productId)
        {
            var mapData = gABusinessEventDataMapping.Find(x => x.productId == productId);
            if (mapData != null)
                return mapData.cartType;
            return "";
        }
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }

    public class AnalyticsConstants
    {
        public const string GA_UndoRewardedBoosterAdPlace = "UndoBooster";
        public const string GA_ExtraBoltRewardedBoosterAdPlace = "ExtraBooster";
        public const string GA_GameWinInterstitialAdPlace = "GameWinInterstitial";
        public const string GA_GameReloadInterstitialAdPlace = "GameRestartInterstitial";

        public const string Adjust_Info = "Adjust_Info";
        public const string IAP_DATA = "IapData";

        public const string LevelData_StartTrigger = "Start";
        public const string LevelData_EndTrigger = "Finish";
        public const string LevelData_RestartTrigger = "Restart";

        public const string SpecialLevelData_SkipTrigger = "Skip";

        public const string AdsData_UndoBoosterName = "Undo";
        public const string AdsData_ExtraBoltBoosterName = "ExtraBolt";

        public const string CoinCurrency = "coin";
        public const string ItemType_Trade = "trade";
        public const string ItemType_Reward = "reward";
        public const string ItemType_Purchase = "purchase";

        public const string ItemId_ExtraBolt = "boosterExtraBolt";
        public const string ItemId_Undo = "boosterUndo";
        public const string ItemId_Default = "default";
        public const string ItemId_LevelWin = "levelWin";
        public const string ItemId_DailyTaskReward = "taskChest";
        public const string ItemId_Leaderboard = "leaderboardRank";
        public const string ItemId_DailyRewards = "dailyReward";
        public const string ItemId_Pack1 = "pack1";
        public const string ItemId_Pack2 = "pack2";
        public const string ItemId_Pack3 = "pack3";
    }
}