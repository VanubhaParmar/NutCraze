using Mediation.Runtime.Scripts;

namespace GameCoreSDK.Iap
{
    public class IapController
    {
        private static IapController _instance;

        private IapController()
        {
        }

        public static IapController GetInstance()
        {
            return _instance ??= new IapController();
        }

        public void SendPurchaseInfo(double dollarValue, string currency)
        {
            IapNative.Instance.SendPurchaseInfo(dollarValue, currency);
        }
    }
}