using Firebase.Extensions;
using GameAnalyticsSDK;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;

namespace Tag.NutSort
{
    public class IAPManager : SerializedManager<IAPManager>, IStoreListener, IDetailedStoreListener
    {
        #region private veriables
        public IAPProductsDataSO IAPProducts => iapProducts;

        [SerializeField] private IAPProductsDataSO iapProducts;

        private static IStoreController m_StoreController; // The Unity Purchasing system.

        private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.
        private Action<string> successAction, failedAction;

        public int IapPurchaseCount
        {
            get { return PlayerPrefs.GetInt("IapPurchaseCountt", 0); }
            private set { PlayerPrefs.SetInt("IapPurchaseCountt", value); }
        }

        #endregion

        #region unity callback

        public override void Awake()
        {
            base.Awake();
            InitializeIAPManager();
        }

        private void InitializeIAPManager()
        {
            //iapProducts = FirebaseManager.Instance.FirebaseRemoteConfig.GetRemoteData(FireBaseRemoteConfigConstant.IAP_PRODUCT_IDS, iapProducts);
            InternetManager.Instance.CheckNetConnection(OnNetCheckFinish);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            m_StoreController = null;
            m_StoreExtensionProvider = null;
        }
        #endregion

        #region private methods

        private void OnNetCheckFinish()
        {
            InitializeGamingService(OnSuccess, OnError);

            void OnSuccess()
            {
                var text = "Congratulations!\nUnity Gaming Services has been successfully initialized.";
                Debug.Log(text);
            }

            void OnError(string message)
            {
                var text = $"Unity Gaming Services failed to initialize with error: {message}.";
                Debug.Log(text);
            }

            if (m_StoreController == null)
            {
                InitializePurchasing();
            }
        }

        private bool IsInitialized()
        {
            // Only say we are initialized if both the Purchasing references are set.
            return m_StoreController != null && m_StoreExtensionProvider != null;
        }

        private void BuyProductID(string productId)
        {
            // If Purchasing has been initialized ...
            if (IsInitialized())
            {
                // ... look up the Product reference with the general product identifier and the Purchasing 
                // system's products collection.
                Product product = m_StoreController.products.WithID(productId);

                // If the look up found a product for this device's store and that product is ready to be sold ... 
                if (product != null && product.availableToPurchase)
                {
                    Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                    // ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
                    // asynchronously.
                    m_StoreController.InitiatePurchase(product);
                }
                // Otherwise ...
                else
                {
                    // ... report the product look-up failure situation  
                    Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            }
            // Otherwise ...
            else
            {
                ShowInfoToast(UserPromptMessageConstants.PurchaseFailedMessage);
                OnPurchaseFailed(productId);
                failedAction = null;
                successAction = null;
                // ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or 
                // retrying initiailization.
                Debug.Log("BuyProductID FAIL. Not initialized.");
            }
        }

        //        private void SendIAPInfoToAdmost(string transactionID, decimal localizedPrice, string isoCurrencyCode, string tag)
        //        {
        //            Debug.Log("SendIAPInfoToAdmost" + transactionID);
        //            if (AMR.AMRSDK.initialized())
        //            {
        //                string[] tags = new string[] { tag };
        //                if (transactionID != string.Empty)
        //                {
        //#if UNITY_IOS
        //                    AMR.AMRSDK.trackIAPForIOS(transactionID, localizedPrice, isoCurrencyCode, tags);
        //#elif UNITY_ANDROID
        //                    AMR.AMRSDK.trackIAPForAndroid(transactionID, localizedPrice, isoCurrencyCode, tags);
        //#endif
        //                }
        //            }
        //        }
        private void OnPurchaseSuccess(PurchaseEventArgs productId)
        {
            if (productId.purchasedProduct.hasReceipt)
            {
                if (successAction != null)
                    successAction?.Invoke(productId.purchasedProduct.definition.id);
                //CoroutineRunner.instance.Wait(3, PlayfabManager.Instance.ForceSaveLocalDataToServer);
                HideInGameLoadingView();
#if UNITY_EDITOR
                return;
#else
                //CheckS2s(productId);
                CrossPlatformValidator(productId);
#endif
            }
            else
            {
                Debug.LogError("Purchase does not have a receipt");
            }
        }
        private void CrossPlatformValidator(PurchaseEventArgs productId)
        {
            bool validPurchase = true; // Presume valid for platforms with no R.V.
            // Unity IAP's validation logic is only included on these platforms.
            // Prepare the validator with the secrets we prepared in the Editor
            // obfuscation window.
            try
            {
                var validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), DevProfileHandler.Instance.MainBuildSettingsDataSO.AndroidBundleIdentifier);
                var result = validator.Validate(productId.purchasedProduct.receipt);
                foreach (IPurchaseReceipt productReceipt in result)
                {
                    //Debug.LogError("productReceipt.productID   " + productReceipt.productID);
                    //Debug.LogError("productReceipt.purchaseDate  " + productReceipt.purchaseDate);
                    //Debug.LogError("productReceipt.transactionID  " + productReceipt.transactionID);
                    GooglePlayReceipt google = productReceipt as GooglePlayReceipt;
                    if (null != google)
                    {
                        // This is Google's Order ID.
                        // Note that it is null when testing in the sandbox
                        // because Google's sandbox does not provide Order IDs.
                        //Debug.LogError("google.transactionID " + google.transactionID);
                        //Debug.LogError("google.purchaseState " + google.purchaseState);
                        //Debug.LogError("google.purchaseToken " + google.purchaseToken);
                        validPurchase = true;
                    }
                    else
                    {
                        Debug.LogError("GooglePlayReceipt is Null");
                        validPurchase = false;
                    }
                }
                // On Google Play, result has a single product ID.
                // On Apple stores, receipts contain multiple products.
                // For informational purposes, we list the receipt(s)
                Debug.Log("Receipt is valid. Contents:");
                //foreach (IPurchaseReceipt productReceipt in result)
                //{
                //    Debug.LogError(productReceipt.productID);
                //    Debug.LogError(productReceipt.purchaseDate);
                //    Debug.LogError(productReceipt.transactionID);
                //}
            }
            catch (IAPSecurityException)
            {
                Debug.LogError("Invalid receipt, not unlocking content");
                validPurchase = false;
            }
            if (validPurchase)
            {
                // Unlock the appropriate content here.
                //CheckPlayfabValidation(productId);
                CheckS2s(productId);
            }
        }

        //private void CheckPlayfabValidation(PurchaseEventArgs purchaseEventArgs)
        //{
        //    string receipt = purchaseEventArgs.purchasedProduct.receipt;
        //    var wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(receipt);
        //    //if (wrapper == null)
        //    //    return PurchaseProcessingResult.Complete;
        //    string payload = ""; // This will hold the payload part of the receipt
        //    if (wrapper.TryGetValue("Payload", out object payloadObj))
        //    {
        //        payload = payloadObj.ToString();

        //        // For Google Play Store purchases, the payload string contains the JSON receipt
        //        var payloadDict = (Dictionary<string, object>)MiniJson.JsonDecode(payload);

        //        // The payload for Google purchases includes a 'json' object and a 'signature'
        //        if (payloadDict.TryGetValue("json", out object json) && payloadDict.TryGetValue("signature", out object signature))
        //        {
        //            // Now you have the signature, purchase token, and product ID
        //            var purchaseDetails = (Dictionary<string, object>)MiniJson.JsonDecode(json.ToString());

        //            string purchaseToken = purchaseDetails["purchaseToken"].ToString();
        //            string productId = purchaseDetails["productId"].ToString();
        //            string signatureStr = signature.ToString();

        //            // Use these values for validation with PlayFab
        //            Debug.Log($"Product ID: {productId}, Purchase Token: {purchaseToken}, Signature: {signatureStr}");
        //            PlayfabManager.Instance.ValidateGooglePlayPurchase(purchaseToken, signatureStr, (isSuccess) =>
        //            {
        //                if (isSuccess)
        //                {
        //                    CheckS2s(purchaseEventArgs);
        //                }
        //                else
        //                {
        //                    CheckS2s(purchaseEventArgs);
        //                }
        //            });

        //        }
        //    }
        //}
        private void CheckS2s(PurchaseEventArgs productId)
        {
            PurchaseReceipt purchaseReceipt = GetPurchaseReceipt(productId);
            Debug.Log("CheckS2s productId:  " + purchaseReceipt.productId);
            Debug.Log("CheckS2s purchaseToken:  " + purchaseReceipt.purchaseToken);
            //Debug.LogError("CheckS2s developerPayload:  " + purchaseReceipt.developerPayload);
            if (!string.IsNullOrEmpty(purchaseReceipt.transactionId))
            {
                string transactionId = purchaseReceipt.transactionId + "___";
                transactionId += TimeManager.Now.Day + "_" + TimeManager.Now.Month + "_" + TimeManager.Now.Year + "_" + TimeManager.Now.Hour + "_" + TimeManager.Now.Minute + "_" + TimeManager.Now.Second + "_" + TimeManager.Now.Millisecond;
                Log_Event(productId, purchaseReceipt.productId, transactionId);
                Debug.Log("CheckS2s transactionId:  " + transactionId);
            }
            else
                Debug.Log("CheckS2s transactionId is Null");
        }
        private void Log_Event(PurchaseEventArgs productId, string id, string orderIdIDWithTime)
        {
            //AnalyticsManager.Instance.LogEvent_IAP_PurchaseRepeat();
            //if (PlayfabPlayerStatisticsManager.Instance != null)
            //    PlayfabPlayerStatisticsManager.Instance.AddPlayerState(PlayerStatisticsNameConstant.IapPurchesCount);
            IapPurchaseCount++;
            AnalyticsManager.Instance.LogEvent_NewBusinessEvent(GetIAPISOCode(productId), GetIAPPrice(productId), id, productId.purchasedProduct.receipt);
            AdjustManager.Instance.Adjust_IAP_Event(id);
            //AdjustManager.Instance.TrackIapTotalEvent(GetIAPPrice(productId), GetIAPISOCode(productId), productId.purchasedProduct.transactionID);
            AdjustManager.Instance.LogEventInServerSide(productId, orderIdIDWithTime);
        }
        private PurchaseReceipt GetPurchaseReceipt(PurchaseEventArgs args)
        {
            // args is received from the processPurchase methos
            string receipt = args.purchasedProduct.receipt;
            string productId = args.purchasedProduct.definition.id;
            string transactionId = args.purchasedProduct.transactionID;
            string purchaseToken = args.purchasedProduct.transactionID;
            //string developerPayload = string.Empty;
            Dictionary<string, object> receiptDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(receipt);
            if (receiptDict.ContainsKey("Payload"))
            {
                string payloadStr = (string)receiptDict["Payload"];
                Dictionary<string, object> payload = JsonConvert.DeserializeObject<Dictionary<string, object>>(payloadStr);

                if (payload.ContainsKey("json"))
                {
                    string payloadJsonStr = (string)payload["json"];
                    Dictionary<string, object> payloadJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(payloadJsonStr);
                    if (payloadJson.ContainsKey("orderId"))
                    {
                        transactionId = (string)payloadJson["orderId"];
                    }
                }
            }
            return new PurchaseReceipt(productId, transactionId, purchaseToken);//, developerPayload);
        }
        private void OnPurchaseFailed(string productId)
        {
            if (failedAction != null)
                failedAction?.Invoke(productId);
            HideInGameLoadingView();
        }

        double GetIAPPrice(PurchaseEventArgs args)
        {
            return (double)args.purchasedProduct.metadata.localizedPrice;
        }
        public string GetIAPPrice(string productId)
        {
#if !UNITY_EDITOR
            if (IsInitialized())
            {
                ProductMetadata productMetadata = m_StoreController.products.WithID(productId).metadata;
                return productMetadata.isoCurrencyCode + productMetadata.localizedPrice;
            }
#endif
            return GetDefaultPrice(productId);
        }

        public string GetDefaultPrice(string productId)
        {
            for (int i = 0; i < iapProducts.IAPProducts.Count; i++)
            {
                if (iapProducts.IAPProducts[i].packId == productId)
                {
                    return iapProducts.IAPProducts[i].defaultPriceText;
                }
            }
            return "$ 0.00";
        }

        string GetIAPISOCode(PurchaseEventArgs args)
        {
            return args.purchasedProduct.metadata.isoCurrencyCode;
        }
        #endregion

        #region public methods

        private void InitializeGamingService(Action onSuccess, Action<string> onError)
        {
            try
            {
                string k_Environment = DevProfileHandler.Instance.IsProductionBuild() ? "production" : "sandbox";
                var options = new InitializationOptions().SetEnvironmentName(k_Environment);

                UnityServices.InitializeAsync(options).ContinueWith(task => onSuccess());
            }
            catch (Exception exception)
            {
                onError(exception.Message);
            }
        }


        public void InitializePurchasing()
        {
            // If we have already connected to Purchasing ...
            if (IsInitialized())
            {
                // ... we are done here.
                OnLoadingDone();
                return;
            }

            // Create a builder, first passing in a suite of Unity provided stores.
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            for (int i = 0; i < iapProducts.IAPProducts.Count; i++)
            {
                builder.AddProduct(iapProducts.IAPProducts[i].packId, iapProducts.IAPProducts[i].productType);
            }

            // Add a product to sell / restore by way of its identifier, associating the general identifier
            // with its store-specific identifiers.
            //for (int i = 0; i < iapProducts.consumableProductIds.Count; i++)
            //{
            //    builder.AddProduct(iapProducts.consumableProductIds[i], ProductType.Consumable);
            //}

            // Continue adding the non-consumable product.
            //for (int i = 0; i < iapProducts.nonConsumableProductIds.Count; i++)
            //{
            //    builder.AddProduct(iapProducts.nonConsumableProductIds[i], ProductType.NonConsumable);
            //}

            // Kick off the remainder of the set-up with an asynchrounous call, passing the configuration 
            // and this class' instance. Expect a response either in OnInitialized or OnInitializeFailed.
            UnityPurchasing.Initialize(this, builder);
        }


        // Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google. 
        // Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
        public void RestorePurchases()
        {
            // If Purchasing has not yet been set up ...
            if (!IsInitialized())
            {
                // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
                Debug.Log("RestorePurchases FAIL. Not initialized.");
                return;
            }

            // If we are running on an Apple device ... 
            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                // ... begin restoring purchases
                Debug.Log("RestorePurchases started ...");

                // Fetch the Apple store-specific subsystem.
                var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
                // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
                // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
                apple.RestoreTransactions((resultBool, resultString) =>
                {
                    // The first phase of restoration. If no more responses are received on ProcessPurchase then 
                    // no purchases are available to be restored.
                    Debug.Log("RestorePurchases continuing: " + resultString + ". If no further messages, no purchases available to restore.");
                });
            }
            // Otherwise ...
            else
            {
                // We are not running on an Apple device. No work is necessary to restore purchases.
                Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
            }
        }


        //  
        // --- IStoreListener
        //

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            // Purchasing has succeeded initializing. Collect our Purchasing references.
            Debug.Log("OnInitialized: PASS");

            // Overall Purchasing system, configured with products for this application.
            m_StoreController = controller;
            // Store specific subsystem, for accessing device-specific store features.
            m_StoreExtensionProvider = extensions;

            OnLoadingDone();
        }


        public KeyValuePair<string, string> GetLocalPrice(string productId)
        {
            if (m_StoreController != null)
            {
                Product product = m_StoreController.products.WithID(productId);
                if (product == null)
                    return new KeyValuePair<string, string>("None", "None");
                return new KeyValuePair<string, string>(product.metadata.localizedPrice + "", product.metadata.isoCurrencyCode);
            }

            return new KeyValuePair<string, string>("None", "None");
        }


        public void OnInitializeFailed(InitializationFailureReason error)
        {
            // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
            Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
            OnLoadingDone();
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
            OnLoadingDone();
        }
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            bool isIDValid = false;
            // A consumable product has been purchased by this user.

            for (int i = 0; i < iapProducts.IAPProducts.Count; i++)
            {
                if (iapProducts.IAPProducts[i].packId.Equals(args.purchasedProduct.definition.id))
                {
                    isIDValid = true;
                    OnPurchaseSuccess(args);
                    break;
                }
            }

            // Or ... a non-consumable product has been purchased by this user.
            //for (int i = 0; i < iapProducts.nonConsumableProductIds.Count; i++)
            //{
            //    if (iapProducts.nonConsumableProductIds[i].Equals(args.purchasedProduct.definition.id))
            //    {
            //        isIDValid = true;
            //        OnPurchaseSuccess(args);
            //        break;
            //    }
            //}

            if (!isIDValid)
            {
                ShowInfoToast(UserPromptMessageConstants.PurchaseFailedMessage);
                OnPurchaseFailed(args.purchasedProduct.definition.id);
            }

            failedAction = null;
            successAction = null;

            // Return a flag indicating whether this product has completely been received, or if the application needs 
            // to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
            // saving purchased products to the cloud, and when that save is delayed. 
            return PurchaseProcessingResult.Complete;
        }


        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
            // this reason with the user to guide their troubleshooting actions.
            ShowInfoToast(UserPromptMessageConstants.PurchaseFailedMessage);
            OnPurchaseFailed(product.definition.id);
            failedAction = null;
            successAction = null;
            Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
            // this reason with the user to guide their troubleshooting actions.
            ShowInfoToast(UserPromptMessageConstants.PurchaseFailedMessage);
            OnPurchaseFailed(product.definition.id);
            failedAction = null;
            successAction = null;
            Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureDescription.message));
        }

        public void PurchaseProduct(string productId, Action<string> onSuccess, Action<string> onFailed)
        {
            Debug.Log("DeviceId: " + DeviceManager.Instance.GetDeviceID());
            Debug.Log("productId: " + productId);
            if (DevProfileHandler.Instance.CanDirectPurchaseInTestingBuild())
            {
                onSuccess?.Invoke(productId);
                //PlayfabManager.Instance.ForceSaveLocalDataToServer();
                IapPurchaseCount++;
                Debug.Log("Purchase Product With DeviceIds: " + DeviceManager.Instance.GetDeviceID());
                return;
            }
            //if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialRunning)
            //{
            //    GameplayUIManager.Instance.GetView<ToastMessageView>().Show(ToastMessageConstant.CompleteTutorial);
            //    return;
            //}
            //if (MetaTutorialManager.Instance != null && MetaTutorialManager.Instance.IsTutorialRunning)
            //{
            //    GameplayUIManager.Instance.GetView<ToastMessageView>().Show(ToastMessageConstant.CompleteTutorial);
            //    return;
            //}
            ShowInGameLoadingView(UserPromptMessageConstants.ConnectingToStoreMessage);
            if (IsInitialized())
            {
                successAction = onSuccess;
                failedAction = onFailed;
                BuyProductID(productId);
            }
            else
            {
                InitializePurchasing();
                OnPurchaseFailed(productId);
            }
        }
        #endregion

        private void ShowInGameLoadingView(string message = "")
        {
            GlobalUIManager.Instance.GetView<InGameLoadingView>().Show(message);
        }

        private void HideInGameLoadingView()
        {
            GlobalUIManager.Instance.GetView<InGameLoadingView>().OnForceHideOnly();
        }

        private void ShowInfoToast(string message, Action actionToCallOnOk = null)
        {
            GlobalUIManager.Instance.GetView<UserPromptView>().Show(message, actionToCallOnOk);
        }

#if UNITY_EDITOR
        [Button]
        public void GetJosn()
        {
            Debug.Log(JsonConvert.SerializeObject(iapProducts));
        }

        //[Button]
        //public void SetJosn(string json)
        //{
        //    iapProducts = JsonConvert.DeserializeObject<IapProductIdsConfig>(json);
        //}
#endif
    }

    //public enum PurchaseType { None, Free, Currency, Iap, Ad, DynamicCurrency, MultiCurrency };
    //public abstract class BasePurchase
    //{
    //    public abstract PurchaseType PurchaseType { get; }
    //}

    [Serializable]
    public class IAPPurchaseData
    {
        public string defaultPriceText;
        public ProductType productType;
        public string packId;
        public RewardsDataSO rewardsDataSO;
    }

    //public class IapProductIdsConfig
    //{
    //    public List<string> consumableProductIds = new List<string>();
    //    public List<string> nonConsumableProductIds = new List<string>();
    //}
    [Serializable]
    public class GABusinessEventDataMapping
    {
        public string name;
        [IAPProductId] public string productId;
        public string itemType;
        public string itemId;
        public string cartType;
    }
    public struct PurchaseReceipt
    {
        public readonly string productId;
        public readonly string transactionId;
        public readonly string purchaseToken;
        //public readonly string developerPayload;
        public PurchaseReceipt(string productId, string transactionId, string purchaseToken)//, string developerPayload)
        {
            this.productId = productId;
            this.transactionId = transactionId;
            this.purchaseToken = purchaseToken;
            //this.developerPayload = developerPayload;
        }
    }
}