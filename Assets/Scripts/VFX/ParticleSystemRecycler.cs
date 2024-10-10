using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
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
            StartCoroutine(WaitAndRecycle());
        }
        #endregion

        #region PUBLIC_METHODS
        #endregion

        #region PRIVATE_METHODS
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        IEnumerator WaitAndRecycle()
        {
            yield return new WaitForSeconds(targetParticleSystem.main.duration + recycleAfterTime);
            ObjectPool.Instance.Recycle(gameObject);
        }
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}