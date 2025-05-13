using UnityEngine;

namespace Tag.NutSort
{
    public class ParticleSystemRecycler : MonoBehaviour
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private ParticleSystem targetParticleSystem;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        private void OnEnable()
        {
            LevelManager.Instance.RegisterOnLevelUnlod(OnLevelUnload);
        }

        private void OnDisable()
        {
            LevelManager.Instance.DeRegisterOnLevelUnload(OnLevelUnload);
        }
        #endregion

        #region PUBLIC_METHODS
        #endregion

        #region PRIVATE_METHODS
        #endregion

        #region EVENT_HANDLERS
        private void OnLevelUnload()
        {
            StopAllCoroutines();
            Debug.Log($"<color=yellow>ParticleSystemRecycler: OnLevelUnload {this.gameObject.name}</color>");
            ObjectPool.Instance.Recycle(this);
        }
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}