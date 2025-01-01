using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class GameManager : SerializedManager<GameManager>
    {
        #region PUBLIC_VARIABLES
        public CameraSizeHandler MainCameraSizeHandler => mainCameraSizeHandler;
        public TransformShakeAnimation MainCameraShakeAnimation => mainCameraShakeAnimation;
        public GameMainDataSO GameMainDataSO => _gameMainDataSO;
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private GameMainDataSO _gameMainDataSO;

        [SerializeField] private CameraSizeHandler mainCameraSizeHandler;
        [SerializeField] private TransformShakeAnimation mainCameraShakeAnimation;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        public override void Awake()
        {
            base.Awake();
            InitGameManager();
            OnLoadingDone();
        }
        #endregion

        #region PUBLIC_METHODS
        public void InitGameManager()
        {
            _gameMainDataSO.InitializeDataSO();
        }

        public void AddWatchAdRewardUndoBoosters()
        {
            DataManager.Instance.AddBoosters(BoosterType.UNDO, _gameMainDataSO.undoBoostersCountToAddOnAdWatch);
        }
        public void AddWatchAdRewardExtraScrewBoosters()
        {
            DataManager.Instance.AddBoosters(BoosterType.EXTRA_BOLT, _gameMainDataSO.extraScrewBoostersCountToAddOnAdWatch);
        }
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