namespace GameCoreSDK.Iap
{
    public class IapController
    {
        private static IapController _instance;
        private readonly IapNativeBridge _iapNativeBridge;

        private IapController()
        {
            _iapNativeBridge = new IapNativeBridge();
        }

        public static IapController GetInstance()
        {
            return _instance ??= new IapController();
        }

        public void SendPurchaseInfo(double dollarValue, string currency)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            _iapNativeBridge.SendPurchaseInfo(dollarValue, currency);
#endif
        }
    }
}