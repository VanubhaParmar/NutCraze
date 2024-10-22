using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class GameManager : SerializedManager<GameManager>
    {
        #region PUBLIC_VARIABLES
        public CameraSizeHandler MainCameraSizeHandler => mainCameraSizeHandler;
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private CameraSizeHandler mainCameraSizeHandler;
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