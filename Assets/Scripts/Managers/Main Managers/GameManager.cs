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
        }
        public void AddUndoBoosters()
        {
            var playerData = PlayerPersistantData.GetMainPlayerProgressData();
            playerData.undoBoostersCount = playerData.undoBoostersCount + _gameMainDataSO.undoBoostersCountToAddOnAdWatch;
            PlayerPersistantData.SetMainPlayerProgressData(playerData);
        }
        public void AddExtraScrewBoosters()
        {
            var playerData = PlayerPersistantData.GetMainPlayerProgressData();
            playerData.extraScrewBoostersCount = playerData.extraScrewBoostersCount + _gameMainDataSO.extraScrewBoostersCountToAddOnAdWatch;
            PlayerPersistantData.SetMainPlayerProgressData(playerData);
        }
        #endregion

        #region PRIVATE_METHODS
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}