using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class TransformRotater : MonoBehaviour
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private Vector3 rotateSpeed;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        private void Update()
        {
            transform.localEulerAngles += rotateSpeed * Time.unscaledDeltaTime;
        }
        #endregion

        #region PUBLIC_METHODS
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