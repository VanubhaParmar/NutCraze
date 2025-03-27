using UnityEngine;

namespace Tag.NutSort
{
    public static class Constant
    {
        public const string GAME_NAME = "Nut Sort";
        public const string PLAYER_DATA = "PlayerData";

        public static bool IsAdOn = true;

        private const string BuildVersionCodeFormat = "v{0} ({1})";

        public static string BuildVersionCodeString => string.Format(BuildVersionCodeFormat, Application.version, VersionCodeFetcher.GetBundleVersionCode());
    }

    public static class CurrencyConstant
    {
        public const int COINS = 1;
        public const int GEMS = 2;
    }

    public static class BoosterIdConstant
    {
        public const int UNDO = 1;
        public const int EXTRA_SCREW = 2;
    }
    
    public static class ScrewTypeIdConstant
    {
        public const int Simple = 1;
        public const int Booster = 2;
    }

    public class UserPromptMessageConstants
    {
        public const string PurchaseFailedMessage = "Purchase failed ! Please try again later.";
        public const string PurchaseSuccessMessage = "Purchase success !";
        public const string ConnectingToStoreMessage = "Connecting to Store !";

        public const string NoAdsPurchaseSuccess = "No ads pack purchased successfully!";
        public const string NoAdsAlreadyPurchase = "No ads pack already purchased and active!";

        public const string NotEnoughCoins = "Not enough coins !";

        public const string RateUsDoneMessage = "Thank you !";

        public const string RewardedAdLoadingMessage = "The video ad will play soon";
        public const string RewardedNotAvailableMessage = "No video ads available";
        public const string NoInternetConnection = "No internet connection!";

        public const string CantUseExtraBoltBoosterMessage = "Bolt cannot be extended anymore!";
        public const string CantUseUndoBoosterMessage = "No moves to undo!";

        public const string NextLeaderboardEventMessage = "Next Leaderboard Event Starts In :";
    }
}

namespace Tag.NutSort.Editor
{
    public static class ResourcesConstants
    {
        public const string LEVELS_PATH = "Assets/Data/LevelData/";
        public const string LEVEL_SO_NAME_FORMAT = "Level {0}";
        public const string SPECIAL_LEVEL_SO_NAME_FORMAT = "Specail Level {0}";
        public const string ARRANGEMENT_SO_NAME_FORMAT = "Arrangement_{0}";
    }

    public static class EditorConstant
    {
        public const string MAPPING_IDS_PATH = "Assets/Data/IdMappings";
        public const string IAP_Manager_Prefab_Path = "Assets/Prefabs/Managers/IAPManager.prefab";
        public const string GameMainDataSO_Path = "Assets/Data/Managers/GameMainDataSO.asset";
    }
}
