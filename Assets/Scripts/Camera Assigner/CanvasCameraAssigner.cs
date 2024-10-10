using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    [RequireComponent(typeof(Canvas))]
    public class CanvasCameraAssigner : MonoBehaviour
    {
        #region PUBLIC_VARIABLES
        public CameraCacheType cameraCacheTypeToAssign;
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        private void Start()
        {
            Canvas canvas = gameObject.GetComponent<Canvas>();
            if (CameraCache.TryFetchCamera(cameraCacheTypeToAssign, out Camera camera))
                canvas.worldCamera = camera;
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