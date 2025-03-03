using Sirenix.OdinInspector;
using System.Collections;
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
            ObjectPool.Instance.Recycle(gameObject);
        }
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}