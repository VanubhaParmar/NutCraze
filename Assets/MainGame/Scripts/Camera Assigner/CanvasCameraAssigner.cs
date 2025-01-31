using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.tag.nut_sort {
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
            AssignCamera();
        }
        #endregion

        #region PUBLIC_METHODS
        public void AssignCamera()
        {
            Canvas canvas = gameObject.GetComponent<Canvas>();
            if (CameraCache.TryFetchCamera(cameraCacheTypeToAssign, out Camera camera))
                canvas.worldCamera = camera;
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