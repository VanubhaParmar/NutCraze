using AdjustSdk;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Purchasing;

namespace Tag.NutSort
{
    public class AdjustManager : SerializedManager<AdjustManager>
    {
        [SerializeField] private Adjust adjust;
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
            adjust.InitializeAdjustSDK();
            adJustRemoteConfig = adjustRemoteConfigDataSO.GetValue<AdJustRemoteConfig>();

            if (DataManager.Instance.isFirstSession)
                Adjust_FirstOpenEvent();

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

        public void TrackRewardedAdWatchEvent(int ad_Watched_Count)
        {
            if (rewardedAdWatchEventMapping.ContainsKey(ad_Watched_Count))
                TrackEvent(rewardedAdWatchEventMapping[ad_Watched_Count]);

        }
        public void Adjust_IAP_Event(string iapId)
        {
            if (adjustIAPIds.ContainsKey(iapId) && !string.IsNullOrEmpty(adjustIAPIds[iapId]))
            {
                TrackEvent(adjustIAPIds[iapId]);
            }
        }
        public void Adjust_FirstOpenEvent()
        {
            TrackEvent(firstGameOpenToken);
        }
        public void Adjust_LevelCompleteEvent(int completedLevel)
        {
            if (levelCompleteEventTokens.ContainsKey(completedLevel))
                TrackEvent(levelCompleteEventTokens[completedLevel]);
        }
        public void TrackEvent(string id)
        {
            AdjustEvent adjustEvent = new AdjustEvent(id);
            DebugLogEvent(id);
            Adjust.TrackEvent(adjustEvent);
        }
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
            string appToken = adjust.appToken;
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
#endif
    }
    public class AdjustConstant
    {
        //public const string IAP_NET_REVENUE = "";
        public const string IAP_NET_REVENUE_S2s = "s2aea7";
        //public const string IAP_NET_REVENUE_Server_Side_Test = "k1raww";
    }
    public class AdJustRemoteConfig
    {
        public bool isSetRevenueEventEnable;
        public bool iss2sOn;
        public string s2sURL;
    }
}