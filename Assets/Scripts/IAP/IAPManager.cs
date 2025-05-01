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
        #endregion

        #region unity callback

        public override void Awake()
        {
            base.Awake();
            InitializeIAPManager();
        }

        private void InitializeIAPManager()
        {
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
            return m_StoreController != null && m_StoreExtensionProvider != null;
        }

        private void BuyProductID(string productId)
        {
            if (IsInitialized())
            {
                Product product = m_StoreController.products.WithID(productId);

                if (product != null && product.availableToPurchase)
                {
                    Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                    m_StoreController.InitiatePurchase(product);
                }
                else
                {
                    Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            }
            else
            {
                ShowInfoToast(UserPromptMessageConstants.PurchaseFailedMessage);
                OnPurchaseFailed(productId);
                failedAction = null;
                successAction = null;
                Debug.Log("BuyProductID FAIL. Not initialized.");
            }
        }

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
            bool validPurchase = true; 

            try
            {
                var validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), DevProfileHandler.Instance.MainBuildSettingsDataSO.AndroidBundleIdentifier);
                var result = validator.Validate(productId.purchasedProduct.receipt);
                foreach (IPurchaseReceipt productReceipt in result)
                {
                    GooglePlayReceipt google = productReceipt as GooglePlayReceipt;
                    if (null != google)
                    {
                        validPurchase = true;
                    }
                    else
                    {
                        Debug.LogError("GooglePlayReceipt is Null");
                        validPurchase = false;
                    }
                }
                Debug.Log("Receipt is valid. Contents:");
            }
            catch (IAPSecurityException)
            {
                Debug.LogError("Invalid receipt, not unlocking content");
                validPurchase = false;
            }
            if (validPurchase)
            {
                CheckS2s(productId);
            }
        }
      
        private void CheckS2s(PurchaseEventArgs productId)
        {
            PurchaseReceipt purchaseReceipt = GetPurchaseReceipt(productId);
            Debug.Log("CheckS2s productId:  " + purchaseReceipt.productId);
            Debug.Log("CheckS2s purchaseToken:  " + purchaseReceipt.purchaseToken);
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
            var purchaseData = iapProducts.IAPProducts.Find(x => x.packId == id);
            double dollerValue = GetIAPPrice(productId);
            if (purchaseData != null)
                dollerValue = double.Parse(purchaseData.GetPriceInUSD());

            AnalyticsManager.Instance.LogEvent_NewBusinessEvent(GetIAPISOCode(productId), GetIAPPrice(productId), id, productId.purchasedProduct.receipt);
            AdjustManager.Instance.Adjust_IAP_Event(id, dollerValue);
            AnalyticsManager.Instance.LogEvent_IAPData(productId.purchasedProduct.definition.id);
        }

        private PurchaseReceipt GetPurchaseReceipt(PurchaseEventArgs args)
        {
            string receipt = args.purchasedProduct.receipt;
            string productId = args.purchasedProduct.definition.id;
            string transactionId = args.purchasedProduct.transactionID;
            string purchaseToken = args.purchasedProduct.transactionID;
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
            if (IsInitialized())
            {
                OnLoadingDone();
                return;
            }

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            for (int i = 0; i < iapProducts.IAPProducts.Count; i++)
            {
                builder.AddProduct(iapProducts.IAPProducts[i].packId, iapProducts.IAPProducts[i].productType);
            }
            UnityPurchasing.Initialize(this, builder);
        }


        public void RestorePurchases()
        {
            if (!IsInitialized())
            {
                Debug.Log("RestorePurchases FAIL. Not initialized.");
                return;
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                Debug.Log("RestorePurchases started ...");

                var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
                apple.RestoreTransactions((resultBool, resultString) =>
                {
                    Debug.Log("RestorePurchases continuing: " + resultString + ". If no further messages, no purchases available to restore.");
                });
            }
            else
            {
                Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
            }
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Debug.Log("OnInitialized: PASS");

            m_StoreController = controller;
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
            for (int i = 0; i < iapProducts.IAPProducts.Count; i++)
            {
                if (iapProducts.IAPProducts[i].packId.Equals(args.purchasedProduct.definition.id))
                {
                    isIDValid = true;
                    OnPurchaseSuccess(args);
                    break;
                }
            }

            if (!isIDValid)
            {
                ShowInfoToast(UserPromptMessageConstants.PurchaseFailedMessage);
                OnPurchaseFailed(args.purchasedProduct.definition.id);
            }

            failedAction = null;
            successAction = null;

            return PurchaseProcessingResult.Complete;
        }


        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            ShowInfoToast(UserPromptMessageConstants.PurchaseFailedMessage);
            OnPurchaseFailed(product.definition.id);
            failedAction = null;
            successAction = null;
            Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            ShowInfoToast(UserPromptMessageConstants.PurchaseFailedMessage);
            OnPurchaseFailed(product.definition.id);
            failedAction = null;
            successAction = null;
            Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureDescription.message));
        }

        public void PurchaseProduct(string productId, Action<string> onSuccess, Action<string> onFailed)
        {
            Debug.LogError("DeviceId: " + DeviceManager.Instance.GetDeviceID());
            Debug.Log("productId: " + productId);
            if (DevProfileHandler.Instance.CanDirectPurchaseInTestingBuild())
            {
                onSuccess?.Invoke(productId);
                Debug.Log("Purchase Product With DeviceIds: " + DeviceManager.Instance.GetDeviceID());
                return;
            }
            ShowInGameLoadingView(UserPromptMessageConstants.ConnectingToStoreMessage);
            if (IsInitialized())
            {
                successAction = onSuccess;
                failedAction = onFailed;
                BuyProductID(productId);
            }
            else
            {
                ShowInfoToast(UserPromptMessageConstants.PurchaseFailedMessage);
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
#endif
    }

 

    [Serializable]
    public class IAPPurchaseData
    {
        public string defaultPriceText;
        public ProductType productType;
        public string packId;
        public RewardsDataSO rewardsDataSO;

        public string GetPriceInUSD()
        {
            return defaultPriceText.Replace("$", "");
        }
    }
    
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
        public PurchaseReceipt(string productId, string transactionId, string purchaseToken)//, string developerPayload)
        {
            this.productId = productId;
            this.transactionId = transactionId;
            this.purchaseToken = purchaseToken;
        }
    }
}