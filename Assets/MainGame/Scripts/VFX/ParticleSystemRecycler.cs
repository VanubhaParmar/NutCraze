using Sirenix.OdinInspector;
using UnityEngine;

namespace Tag.NutSort
{
    public class ParticleSystemRecycler : SerializedMonoBehaviour
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private float recycleAfterTime = 0.5f;
        [SerializeField] private ParticleSystem targetParticleSystem;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        private void OnEnable()
        {
            LevelManager.Instance.RegisterOnLevelUnlod(OnLevelRecycle);
        }

        private void OnDisable()
        {
            LevelManager.Instance.DeRegisterOnLevelUnload(OnLevelRecycle);
        }
        #endregion

        #region PUBLIC_METHODS
        #endregion

        #region PRIVATE_METHODS
        #endregion

        #region EVENT_HANDLERS
        private void OnLevelRecycle()
        {
            StopAllCoroutines();
            ObjectPool.Instance.Recycle(gameObject);
        }
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}