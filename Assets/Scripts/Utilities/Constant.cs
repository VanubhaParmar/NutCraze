namespace Tag.NutSort
{
    public static class Constant
    {
        public const string GAME_NAME = "Nut Sort";
        public const string PLAYER_DATA = "PlayerData";

        public static bool IsAdOn = true;
    }

    public static class ResourcesConstants
    {
        public const string MAIN_RESOURCE_PATH = "Assets/Resources/";
        public const string MAIN_RESOURCE_PATH_FROM_PERSISTANT_PATH = "/Resources/";

        public const string LEVELS_PATH = "Levels/";
        public const string SPECIAL_LEVELS_PATH = "Special Levels/";

        public const string LEVEL_SO_NAME_FORMAT = "Level {0}";
    }

    public static class GamePlayConstant
    {
    }

    public static class EditorConstant
    {
        public const string MAPPING_IDS_PATH = "Assets/Data/IdMappings";
        public const string IAP_Manager_Prefab_Path = "Assets/Prefabs/Managers/IAPManager.prefab";
    }

    public static class CurrencyConstant
    {
        public const int COINS = 1;
        public const int GEMS = 2;
    }

    public class UserPromptMessageConstants
    {
        public const string All_Levels_Completed_Header = "Levels Completed";
        public const string All_Levels_Completed_Mesasge = "Well done! You have completed all the Levels. We will be adding more levels to the game soon. Stay tuned for future updates.";

        public const string PurchaseFailedMessage = "Purchase failed !\nPlease try again later.";
        public const string ConnectingToStoreMessage = "Connecting to Store !";

        public const string NotEnoughCoins = "Not enough coins !";

        public const string RewardedAdLoadingMessage = "The video ad will play soon";
        public const string RewardedNotAvailableMessage = "No video ads available\nPlease try again later.";
        public const string NoInternetConnection = "No internet connection!\nPlease check you internet and try again.";
    }
}
