using Sirenix.OdinInspector;
using UnityEngine;

namespace Tag.NutSort
{
    public abstract class BaseGamePlayHandler : SerializedMonoBehaviour
    {
        #region PRIVATE_VARS
        [SerializeField] private GamePlayType gamePlayType;
        protected GameplayManager gameplayManager;
        protected GameplayStateData gameplayStateData;
        #endregion

        #region PUBLIC_VARS
        #endregion

        #region PROPERTIES
        public GameplayStateData GameplayStateData { get => gameplayStateData;  }
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_FUNCTIONS
        public virtual void Init(GameplayManager gameplayManager)
        {
            this.gameplayManager = gameplayManager;
        }

        public virtual void StartLevel(LevelDataSO levelDataSO)
        {

        }

        public virtual void StartNextOrCurrentLevel()
        {
        }

        public virtual void OnLevelRestart()
        {
        }

        public virtual void OnLevelComplete()
        {

        }
        

        public virtual void OnLevelFail()
        {

        }

        public virtual void OnLevelFailRetry()
        {

        }

        public virtual void OnExit()
        {

        }

        public virtual void OnRetry()
        {

        }

        public virtual void OnNutTransferComplete(BaseScrew fromScrew, BaseScrew toScrew, int nutsTransferred)
        {
        }

        public virtual void OnScrewSortComplete(BaseScrew baseScrew)
        { 
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        #endregion

        #region CO-ROUTINES
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region UI_CALLBACKS
        #endregion

        #region EDITOR
#if UNITY_EDITOR
#endif
        #endregion
    }
    public enum GamePlayType
    {
        None,
        Main,
        Daily
    }
}
