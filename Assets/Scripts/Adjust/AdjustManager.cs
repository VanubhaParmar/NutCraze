//using AdjustSdk;
using GameCoreSDK.Iap;
using GameCoreSDK.Puzzle;
using Mediation.Runtime.Scripts.Track;
using Mediation.Runtime.Scripts.Track.config;
using Mediation.Runtime.Scripts.Track.model;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Purchasing;

namespace Tag.NutSort
{
    public class AdjustManager : SerializedManager<AdjustManager>
    {
        //[SerializeField] private Adjust adjust;
        [ShowInInspector, ReadOnly] private AdJustRemoteConfig adJustRemoteConfig = new AdJustRemoteConfig();
        [SerializeField] private AdjustRemoteConfigDataSO adjustRemoteConfigDataSO;

        [Title("AdJust EventIds")]
        [SerializeField] private string firstGameOpenToken;
        [SerializeField] private Dictionary<int, string> levelCompleteEventTokens = new Dictionary<int, string>();
        //[SerializeField] private Dictionary<int, AdJustEventConfig> adJustLevelEventConfigs = new Dictionary<int, AdJustEventConfig>();
        //[SerializeField] private Dictionary<int, Dictionary<int, AdJustEventConfig>> adJustTutorialIDs = new Dictionary<int, Dictionary<int, AdJustEventConfig>>();
        [SerializeField] private Dictionary<string, string> adjustIAPIds = new Dictionary<string, string>();
        [SerializeField] private Dictionary<int, string> rewardedAdWatchEventMapping = new Dictionary<int, string>();

        private void OnEnable()
        {
            FirebaseRemoteConfigManager.onRCValuesFetched += FirebaseRemoteConfigManager_onRCValuesFetched;
        }

        private void OnDisable()
        {
            FirebaseRemoteConfigManager.onRCValuesFetched -= FirebaseRemoteConfigManager_onRCValuesFetched;
        }

        public override void Awake()
        {
            base.Awake();
            InitializedAdjustManager();
        }

        public void InitializedAdjustManager()
        {
            //adjust.InitializeAdjustSDK();
            adJustRemoteConfig = adjustRemoteConfigDataSO.GetValue<AdJustRemoteConfig>();

            //if (DataManager.Instance.isFirstSession)
            //    Adjust_FirstOpenEvent();

            InitializePlayerPersistantData();
            OnLoadingDone();
        }

        //public void Start()
        //{
        //    StartCoroutine(WaitForInit());
        //}

        //public override void OnDestroy()
        //{
        //    if (DataManager.Instance)
        //        DataManager.Level.RemoveOnCurrencyChangeEvent(AdJust_Level_Event);
        //    if (TutorialManager.Instance)
        //        TutorialManager.Instance.DeregisterOnTutorialStepStart(AdJust_Tutorial_Start_Event);
        //    if (TutorialManager.Instance)
        //        TutorialManager.Instance.DeregisterOnTutorialStepComplete(AdJust_Tutorial_End_Event);
        //    base.OnDestroy();
        //}
       
        //private void AdJust_Level_Event(int level)
        //{
        //    level++;
        //    if (adJustLevelEventConfigs.ContainsKey(level))
        //    {
        //        if (!string.IsNullOrEmpty(adJustLevelEventConfigs[level].startId))
        //            TrackEvent(adJustLevelEventConfigs[level].startId);
        //        if (adJustLevelEventConfigs.ContainsKey(level - 1) && !string.IsNullOrEmpty(adJustLevelEventConfigs[level - 1].completeId))
        //        {
        //            TrackEvent(adJustLevelEventConfigs[level - 1].completeId);
        //        }
        //    }
        //}

        //private void AdJust_Tutorial_Start_Event(int tutorialId, int stepId)
        //{
        //    if (adJustTutorialIDs.ContainsKey(tutorialId) && adJustTutorialIDs[tutorialId].ContainsKey(stepId) && !string.IsNullOrEmpty(adJustTutorialIDs[tutorialId][stepId].startId))
        //    {
        //        TrackEvent(adJustTutorialIDs[tutorialId][stepId].startId);
        //    }
        //}

        //private void AdJust_Tutorial_End_Event(int tutorialId, int stepId)
        //{
        //    if (adJustTutorialIDs.ContainsKey(tutorialId) && adJustTutorialIDs[tutorialId].ContainsKey(stepId) && !string.IsNullOrEmpty(adJustTutorialIDs[tutorialId][stepId].completeId))
        //    {
        //        TrackEvent(adJustTutorialIDs[tutorialId][stepId].completeId);
        //    }
        //}

        //public void TrackRewardedAdWatchEvent(int ad_Watched_Count)
        //{
        //    if (rewardedAdWatchEventMapping.ContainsKey(ad_Watched_Count))
        //        TrackEvent(rewardedAdWatchEventMapping[ad_Watched_Count]);

        //}
        public void Adjust_IAP_Event(string iapId, double dollerValue, string currency = "USD")
        {
            IapController.GetInstance().SendPurchaseInfo(dollerValue, currency);
            DebugLogEvent($"IAP Purchased {dollerValue} {currency}");
            //if (adjustIAPIds.ContainsKey(iapId) && !string.IsNullOrEmpty(adjustIAPIds[iapId]))
            //{
            //    TrackEvent(adjustIAPIds[iapId]);
            //}
        }

        //public void Adjust_FirstOpenEvent()
        //{
        //    TrackEvent(firstGameOpenToken);
        //}

        public void Adjust_GameSessionStart()
        {
            var statsData = PlayerPersistantData.GetGameStatsPlayerData();

            // Start Wallet Balance
            var coinsData = DataManager.Instance.GetCurrency((int)CurrencyType.Coin);
            TrackingBridge.Instance.SetExtraParameter(SessionTrackerConstants.TrackSessionEventStartWalletBalance, coinsData.Value);
            //

            // Lapsed days
            bool isLastPlayedSessionTimeAvailable = GameStatsCollector.Instance.LastPlayedSessionTimeString.TryParseDateTime(out DateTime lastPlayedSessionTime);
            int numberOfPassedDays = 0;
            int numberOfPassedSeconds = 0;
            if (isLastPlayedSessionTimeAvailable)
            {
                var currentDateTime = TimeManager.Now;
                numberOfPassedDays = Mathf.Max((int)(currentDateTime - lastPlayedSessionTime).TotalDays, 0);
                numberOfPassedSeconds = Mathf.Max((int)(currentDateTime - lastPlayedSessionTime).TotalSeconds, 0);
            }
            TrackingBridge.Instance.SetExtraParameter(SessionTrackerConstants.TrackSessionEventLapsedDays, numberOfPassedDays);
            //

            // Lapsed seconds
            TrackingBridge.Instance.SetExtraParameter(SessionTrackerConstants.TrackSessionEventTimeFromLastSession, numberOfPassedSeconds);
            //

            DebugLogEvent($"Session Start Wallet = {coinsData.Value} Lapsed Days = {numberOfPassedDays} Lapsed Seconds = {numberOfPassedSeconds}");
        }

        public void Adjust_GamePauseEvent()
        {
            if (MainSceneLoader.Instance == null || !MainSceneLoader.Instance.IsLoaded)
                return;

            var statsData = PlayerPersistantData.GetGameStatsPlayerData();

            // GameCurrenciesData
            List<CurrencyInfo> currencyInfos = new List<CurrencyInfo>();
            foreach (var kvp in statsData.sessionBasedCurrencyData)
            {
                GameStatCurrencyInfo gameStatCurrencyInfo = SerializeUtility.DeserializeObject<GameStatCurrencyInfo>(kvp.Value);
                var currentData = DataManager.Instance.GetCurrency(kvp.Key);

                if (currentData != null)
                    gameStatCurrencyInfo.finalValue = currentData.Value;

                if (gameStatCurrencyInfo != null)
                    currencyInfos.Add(ConvertToAdjustCurrencyInfo(gameStatCurrencyInfo));
            }
            TrackingBridge.Instance.SetExtraParameter(SessionTrackerConstants.TrackSessionEventGameCurrencyInfo, currencyInfos);
            //

            // Iap Currencies Data
            List<CurrencyInfo> iapCurrencyInfos = new List<CurrencyInfo>();
            foreach (var kvp in statsData.sessionBasedCurrencyData)
            {
                GameStatCurrencyInfo gameStatCurrencyInfo = SerializeUtility.DeserializeObject<GameStatCurrencyInfo>(kvp.Value);
                gameStatCurrencyInfo.startValue = 0;
                gameStatCurrencyInfo.finalValue = 0;
                gameStatCurrencyInfo.freeEarn = 0;
                gameStatCurrencyInfo.spend = 0;

                if (gameStatCurrencyInfo != null)
                    iapCurrencyInfos.Add(ConvertToAdjustCurrencyInfo(gameStatCurrencyInfo));
            }
            TrackingBridge.Instance.SetExtraParameter(SessionTrackerConstants.TrackSessionEventIapCurrenciesEarned, iapCurrencyInfos);
            //

            // End Wallet Balance
            var coinsData = DataManager.Instance.GetCurrency((int)CurrencyType.Coin);
            TrackingBridge.Instance.SetExtraParameter(SessionTrackerConstants.TrackSessionEventEndWalletBalance, coinsData.Value);
            //

            // Lowest Wallet Balance
            TrackingBridge.Instance.SetExtraParameter(SessionTrackerConstants.TrackSessionEventLowestWalletInSession, statsData.lowestCoinBalanceDuringSession);
            //

            // Earn Action
            TrackingBridge.Instance.SetExtraParameter(SessionTrackerConstants.TrackSessionEventTotalEarnActions, statsData.numberOfEarnActionsInSession);
            //

            // Puzzle Cleared and Failed
            string clearedFailedParameter = statsData.numberOfPassedLevelsInSession + "_" + statsData.numberOfFailedLevelsInSession;
            TrackingBridge.Instance.SetExtraParameter(SessionTrackerConstants.TrackSessionEventNumPuzzlesClearedAndFailed, clearedFailedParameter);
            //

            // Running events
            var runnningEvents = GameplayManager.Instance.GetListOfRunningEvents();
            string runningEventParameter = "";
            for (int i = 0; i < runnningEvents.Count; i++)
            {
                if (i > 0)
                    runningEventParameter += "_";
                runningEventParameter += runnningEvents[i];
            }
            TrackingBridge.Instance.SetExtraParameter(SessionTrackerConstants.TrackSessionEventRunningEventNames, runningEventParameter);
            //

            // Time spent on puzzle screen
            var timeSpentOnPuzzle = MainSceneUIManager.Instance.GetView<GameplayView>().TotalTimeSpentOnScreen;
            TrackingBridge.Instance.SetExtraParameter(SessionTrackerConstants.TrackSessionEventTimeSpentOnPuzzle, timeSpentOnPuzzle);
            //

            DebugLogEvent($"Session Currencies Data {currencyInfos.Select(x => SerializeUtility.SerializeObject(x)).ToList().PrintList()}");
            DebugLogEvent($"Session Currencies IAP Data {iapCurrencyInfos.Select(x => SerializeUtility.SerializeObject(x)).ToList().PrintList()}");
            DebugLogEvent($"Session End Wallet = {coinsData.Value} Lowest Wallet = {statsData.lowestCoinBalanceDuringSession} Earn Actions = {statsData.numberOfEarnActionsInSession} " +
                $"ClearFailed = {clearedFailedParameter} RunningEvents = {runningEventParameter} TimeSpentOnGameplay = {timeSpentOnPuzzle}");
        }

        public void Adjust_OutOfCoinsEvent()
        {
            TrackingBridge.Instance.SetExtraParameter(PuzzleTrackerConstants.TrackPuzzleEventOocView, 1);
            TrackingBridge.Instance.SetExtraParameter(SessionTrackerConstants.TrackSessionEventOocView, 1);
            DebugLogEvent($"OOC View");
        }

        public void Adjust_ChokePointEvent(int numberOfPossibleMoves)
        {
            if (numberOfPossibleMoves > 2 || numberOfPossibleMoves <= 0)
                return;

            string eventKey = numberOfPossibleMoves == 1 ? PuzzleTrackerConstants.TrackPuzzleEventChokePoint2InPuzzle : PuzzleTrackerConstants.TrackPuzzleEventChokePoint1InPuzzle;
            TrackingBridge.Instance.SetExtraParameter(eventKey, 1);

            DebugLogEvent($"Choke Point {(numberOfPossibleMoves == 1 ? 2 : 1)}");
        }

        public void Adjust_LevelStartEvent(int levelNumber, LevelType levelType)
        {
            // return back if start event is already logged
            var adjustEventData = PlayerPersistantData.GetAdjustEventPlayerPersistantData();
            if (levelType == LevelType.NORMAL_LEVEL && levelNumber == adjustEventData.lastNormalLevelStartEventNumber)
                return;
            if (levelType == LevelType.SPECIAL_LEVEL && levelNumber == adjustEventData.lastSpecialLevelStartEventNumber)
                return;

            GameStatsCollector.Instance.GameCurrenciesData_MarkNewLevel(); // new level mark for currencies data

            // OnLevelStart event
            PuzzleController.GetInstance().OnLevelStart(levelNumber);
            //

            // PuzzleType event
            string adjustParameter = levelType == LevelType.NORMAL_LEVEL ? AdjustConstant.Normal_Level_Event_Parameter : AdjustConstant.Special_Level_Event_Parameter;
            TrackingBridge.Instance.SetExtraParameter(PuzzleTrackerConstants.TrackPuzzleEventPuzzleType, adjustParameter);
            //

            if (levelType == LevelType.NORMAL_LEVEL)
                adjustEventData.lastNormalLevelStartEventNumber = levelNumber;
            else if (levelType == LevelType.SPECIAL_LEVEL)
                adjustEventData.lastSpecialLevelStartEventNumber = levelNumber;

            PlayerPersistantData.SetAdjustEventPlayerPersistantData(adjustEventData);

            DebugLogEvent($"Level Start {levelNumber} {adjustParameter}");
            //if (levelCompleteEventTokens.ContainsKey(completedLevel))
            //    TrackEvent(levelCompleteEventTokens[completedLevel]);
        }

        public void Adjust_LevelCompleteEvent(int completedLevel, int levelRunningTimeInSeconds)
        {
            GameStatsCollector.Instance.GameCurrenciesData_MarkLevelEnd(); // level end mark for currencies data
            var statsData = PlayerPersistantData.GetGameStatsPlayerData();

            // PuzzleRetryCount
            int totalRetriesDoneOnDay = statsData.totalNumberOfRetriesDoneInDay;
            TrackingBridge.Instance.SetExtraParameter(PuzzleTrackerConstants.TrackPuzzleEventPuzzleRetryCount, totalRetriesDoneOnDay);
            //

            // Lives/Chances used in a puzzle
            int totalRetriesDoneOnPuzzle = statsData.totalNumberOfRetriesDoneForLevel;
            TrackingBridge.Instance.SetExtraParameter(PuzzleTrackerConstants.TrackPuzzleEventLifeUsedInPuzzle, totalRetriesDoneOnPuzzle);
            //

            // GameCurrenciesData
            List<CurrencyInfo> currencyInfos = new List<CurrencyInfo>();
            foreach (var kvp in statsData.levelBasedCurrencyData)
            {
                GameStatCurrencyInfo gameStatCurrencyInfo = SerializeUtility.DeserializeObject<GameStatCurrencyInfo>(kvp.Value);
                if (gameStatCurrencyInfo != null)
                    currencyInfos.Add(ConvertToAdjustCurrencyInfo(gameStatCurrencyInfo));
            }
            TrackingBridge.Instance.SetExtraParameter(PuzzleTrackerConstants.TrackPuzzleEventGameCurrencyInfo, currencyInfos);
            //

            // Popups surfaced on puzzle
            int userTriggeredPopUps = statsData.numberOfUserTriggeredPopups;
            int systemTriggeredPopUps = statsData.numberOfSystemTriggeredPopups;
            string triggeredPopUpsCountParameter = userTriggeredPopUps + "_" + systemTriggeredPopUps;
            TrackingBridge.Instance.SetExtraParameter(PuzzleTrackerConstants.TrackPuzzleEventPopupSurfacedOnPuzzle, triggeredPopUpsCountParameter);
            //

            // Lowest wallet balance
            int lowestCoinBalance = statsData.lowestCoinBalanceDuringLevel;
            TrackingBridge.Instance.SetExtraParameter(PuzzleTrackerConstants.TrackPuzzleEventLowestWalletInPuzzle, lowestCoinBalance);
            //

            // OnLevelComplete event
            PuzzleController.GetInstance().OnLevelComplete(completedLevel, levelRunningTimeInSeconds);
            //

            DebugLogEvent($"Level Complete {completedLevel} - {levelRunningTimeInSeconds}s RetryDoneInDay = {totalRetriesDoneOnDay} RetryDoneInLevel = {totalRetriesDoneOnPuzzle} " +
                $"Triggered Popups = {triggeredPopUpsCountParameter} Lowest Balance = {lowestCoinBalance}");
            DebugLogEvent($"Level Currencies Data {currencyInfos.Select(x => SerializeUtility.SerializeObject(x)).ToList().PrintList()}");
            //if (levelCompleteEventTokens.ContainsKey(completedLevel))
            //    TrackEvent(levelCompleteEventTokens[completedLevel]);
        }

        public CurrencyInfo ConvertToAdjustCurrencyInfo(GameStatCurrencyInfo gameStatCurrencyInfo)
        {
            CurrencyInfo currencyInfo = new CurrencyInfo()
            {
                CurrencyName = gameStatCurrencyInfo.currencyName,
                StartValue = gameStatCurrencyInfo.startValue.ToString(),
                FreeEarn = gameStatCurrencyInfo.freeEarn.ToString(),
                Earn = gameStatCurrencyInfo.earn.ToString(),
                Spend = gameStatCurrencyInfo.spend.ToString(),
                FinalValue = gameStatCurrencyInfo.finalValue.ToString()
            };

            return currencyInfo;
        }

        //public void TrackEvent(string id)
        //{
        //    AdjustEvent adjustEvent = new AdjustEvent(id);
        //    DebugLogEvent(id);
        //    Adjust.TrackEvent(adjustEvent);
        //}
        // public void TrackAdRevenue(MaxSdkBase.AdInfo adInfo)
        // {
        //     AdjustAdRevenue adjustAdRevenue = new AdjustAdRevenue("applovin_max_sdk");
        //     adjustAdRevenue.SetRevenue(adInfo.Revenue, "USD");
        //     adjustAdRevenue.AdRevenueNetwork = adInfo.NetworkName;
        //     adjustAdRevenue.AdRevenueUnit = adInfo.AdUnitIdentifier;
        //     adjustAdRevenue.AdRevenuePlacement = adInfo.Placement;
        //     Adjust.TrackAdRevenue(adjustAdRevenue);
        // }
        //public void TrackIapTotalEvent(double price, string currency, string trancationId)
        //{
        //    //Adjust Purchase track
        //    AdjustEvent adjustEvent = new AdjustEvent(AdjustConstant.IAP_NET_REVENUE);
        //    //if (adJustRemoteConfig.isSetRevenueEventEnable)
        //    //{
        //    //    adjustEvent.setRevenue((float)(price), currency);
        //    //    adjustEvent.setTransactionId(trancationId);
        //    //}
        //    Adjust.TrackEvent(adjustEvent);
        //}
        [Button]
        public void LogEventInServerSide(PurchaseEventArgs args, string transactionID)
        {
#if UNITY_ANDROID
            if (adJustRemoteConfig.iss2sOn)
            {
                StartCoroutine(GetAdvertisingId(args, transactionID));
            }
#endif
        }
        private void DebugLogEvent(string eventName)
        {
            Debug.Log("<color=#FFD700>Adjust Event : " + eventName + "</color>");
        }
        private void FirebaseRemoteConfigManager_onRCValuesFetched()
        {
            adJustRemoteConfig = adjustRemoteConfigDataSO.GetValue<AdJustRemoteConfig>();
        }

        private void InitializePlayerPersistantData()
        {
            var adjustEventPlayerData = PlayerPersistantData.GetAdjustEventPlayerPersistantData();
            if (adjustEventPlayerData == null)
            {
                adjustEventPlayerData = new AdjustEventPlayerData();
                PlayerPersistantData.SetAdjustEventPlayerPersistantData(adjustEventPlayerData);
            }
        }

        private IEnumerator GetAdvertisingId(PurchaseEventArgs args, string transactionID)
        {
            string advertisingId = "";
            bool limitAdTracking = false;
            AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass clientInfo = new AndroidJavaClass("com.google.android.gms.ads.identifier.AdvertisingIdClient");
            AndroidJavaObject adInfo = null;

            // Run in background thread to not freeze Unity
            yield return new UnityEngine.WaitUntil(() =>
            {
                try
                {
                    adInfo = clientInfo.CallStatic<AndroidJavaObject>("getAdvertisingIdInfo", currentActivity);
                    return true;
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Could not get Advertising ID: " + e.Message);
                    return false;
                }
            });

            if (adInfo != null)
            {
                advertisingId = adInfo.Call<string>("getId");
                limitAdTracking = adInfo.Call<bool>("isLimitAdTrackingEnabled");

                Debug.Log("Advertising ID: " + advertisingId);
                //StartCoroutine(SendEventToAdjust(args, advertisingId));
                StartCoroutine(VerifyPurchase(args, advertisingId, transactionID));

            }
        }
        IEnumerator VerifyPurchase(PurchaseEventArgs args, string advertisingId, string transactionID)
        {
            string url = adJustRemoteConfig.s2sURL;
            string packageName = DevProfileHandler.Instance.MainBuildSettingsDataSO.AndroidBundleIdentifier;
            string productId = args.purchasedProduct.definition.id;
            string token = args.purchasedProduct.transactionID;

            string s2s = "1";
            string eventToken = AdjustConstant.IAP_NET_REVENUE_S2s;
            string appToken = "i0dznwr1glxc";// adjust.appToken; => Set Adjust app token here
            string gps_adid = advertisingId;
            DateTime dateTime = new DateTime(TimeManager.Now.Year, TimeManager.Now.Month, TimeManager.Now.Day, TimeManager.Now.Hour, TimeManager.Now.Minute, TimeManager.Now.Second, DateTimeKind.Utc);
            DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime, TimeSpan.Zero);
            string formattedDateTime = dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
            string createdAt = formattedDateTime.Replace(":", "");
            string currency = args.purchasedProduct.metadata.isoCurrencyCode;
            string revenue = ((double)args.purchasedProduct.metadata.localizedPrice).ToString();
            string environment;
            if (DevProfileHandler.Instance.IsProductionBuild())
                environment = "production";
            else
                environment = "sandbox";
            var adjustParameters = new Dictionary<string, object>
            {
                ["s2s"] = s2s,
                ["event_token"] = eventToken,
                ["app_token"] = appToken,
                ["gps_adid"] = gps_adid,
                ["created_at"] = createdAt,
                ["currency"] = currency,
                ["revenue"] = revenue,
                ["environment"] = environment
            };

            var data = new Dictionary<string, object>
            {
                ["packageName"] = packageName,
                ["productId"] = productId,
                ["token"] = token,
                ["adjustParameters"] = adjustParameters
            };

            string jsonBody = JsonConvert.SerializeObject(data);

            Debug.LogError(url);
            Debug.LogError(jsonBody);

            using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
            {
                byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonBody);
                webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
                webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

                // Send the request and wait for the response
                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Adjust Error: " + webRequest.error);
                }
                else
                {
                    // Handle the response
                    Debug.LogError("Adjust Response: " + webRequest.downloadHandler.text);
                    var dict = MiniJson.JsonDecode(webRequest.downloadHandler.text) as Dictionary<string, object>;
                    if (dict != null)
                    {
                        if (dict.TryGetValue("valid", out object validValue) && validValue is bool valid && valid)
                        {
                            AnalyticsManager.Instance.LogEvent_AdjustS2sInfo("Purchase is valid");
                            AnalyticsManager.Instance.LogEvent_IAPData(productId);
                        }
                        else
                        {
                            AnalyticsManager.Instance.LogEvent_AdjustS2sInfo("Purchase is not valid");
                        }
                    }
                }
            }
        }
       
#if UNITY_EDITOR
        [Button]
        private string GetJson()
        {
            return JsonConvert.SerializeObject(adJustRemoteConfig);
        }

        [Button]
        private void AddLevelKeys(string keys)
        {
            levelCompleteEventTokens.Clear();
            string[] parseKey = keys.Split(' ');
            for (int i = 0; i < parseKey.Length; i++)
            {
                levelCompleteEventTokens.Add(i + 1, parseKey[i]);
            }
        }


        [Button]
        public void Editor_PrintAdjustEventData()
        {
            var data = SerializeUtility.SerializeObject(PlayerPersistantData.GetAdjustEventPlayerPersistantData());
            Debug.Log(data);
            GUIUtility.systemCopyBuffer = data;
        }

        [Button]
        public void Editor_ClearData()
        {
            PlayerPersistantData.SetAdjustEventPlayerPersistantData(null);
        }

#endif
    }
    public class AdjustConstant
    {
        //public const string IAP_NET_REVENUE = "";
        public const string IAP_NET_REVENUE_S2s = "s2aea7";
        //public const string IAP_NET_REVENUE_Server_Side_Test = "k1raww";

        public const string Normal_Level_Event_Parameter = "main_puzzle";
        public const string Special_Level_Event_Parameter = "special_puzzle";
        public const string Leader_Board_Event_Name = "leaderboard";
    }
    public class AdJustRemoteConfig
    {
        public bool isSetRevenueEventEnable;
        public bool iss2sOn;
        public string s2sURL;
    }

    public class AdjustEventPlayerData
    {
        [JsonProperty("lnlsen")] public int lastNormalLevelStartEventNumber;
        [JsonProperty("lslsen")] public int lastSpecialLevelStartEventNumber;
    }
}