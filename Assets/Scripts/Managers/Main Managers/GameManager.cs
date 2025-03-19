using UnityEngine;

namespace Tag.NutSort
{
    public class GameManager : SerializedManager<GameManager>
    {
        #region PUBLIC_VARIABLES
        public GameMainDataSO GameMainDataSO => _gameMainDataSO;
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private GameMainDataSO _gameMainDataSO;
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
        #endregion

        #region PRIVATE_METHODS
        #endregion

        #region EVENT_HANDLERS
        public delegate void OnGameVoidEvents();

        public static event OnGameVoidEvents onBoosterPurchaseSuccess;
        public static void RaiseOnBoosterPurchaseSuccess()
        {
            onBoosterPurchaseSuccess?.Invoke();
        }

        public static event OnGameVoidEvents onRewardsClaimedUIRefresh;
        public static void RaiseOnRewardsClaimedUIRefresh()
        {
            onRewardsClaimedUIRefresh?.Invoke();
        }
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}