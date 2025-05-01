using Sirenix.OdinInspector;
using UnityEngine;

namespace Tag.NutSort
{
    public abstract class BaseGameplayHelper : MonoBehaviour
    {
        #region PROTECTED_VARIABLES
        protected GameplayManager gameplayManager;
        [ShowInInspector] protected GameplayStateData gameplayStateData;
        #endregion

        #region PUBLIC_VARIABLES
        #endregion

        #region PROPERTIES
        public GameplayStateData GameplayStateData => gameplayStateData;
        public bool IsPlayingLevel => GameplayStateData.gameplayStateType == GameplayStateType.PLAYING_LEVEL;
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region VIRTUAL_METHODS
        public virtual void Init(GameplayManager manager)
        {
            gameplayManager = manager;
        }
        public virtual void StartGameplay() { }
        public virtual void PauseGameplay() { }
        public virtual void ResumeGameplay() { }
        public virtual void RestartGameplay() { }
        public virtual void EndGameplay() { }
        public virtual void LoadLevel(LevelDataSO levelData) { }
        public virtual void LoadNormalLevel() { }
        public virtual void LoadSpecialLevel(int specialLevelNumber) { }
        public virtual void LoadSavedLevel() { }
        public virtual void OnScrewSortComplete(BaseScrew screw) { }
        public virtual void OnNutTransferComplete(BaseScrew fromScrew, BaseScrew toScrew, int nutsTransferred) { }
        public virtual void Cleanup() { }
        #endregion
    }
}