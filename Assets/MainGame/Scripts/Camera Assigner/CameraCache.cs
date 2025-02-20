using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    [RequireComponent(typeof(Camera))]
    public class CameraCache : MonoBehaviour
    {
        #region PUBLIC_VARIABLES
        public CameraCacheType cameraCacheType;
        public static List<CameraCacheData> cameraCaches = new List<CameraCacheData>();
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        private void Awake()
        {
            Camera cameraRef = this.gameObject.GetComponent<Camera>();
            var cache = cameraCaches.Find(x => x.cameraCacheType == cameraCacheType);

            if (cache == null)
            {
                cache = new CameraCacheData(cameraCacheType);
                cameraCaches.Add(cache);
            }
            cache.targetCamera = cameraRef;
        }
        #endregion

        #region PUBLIC_METHODS
        public static bool TryFetchCamera(CameraCacheType cameraCacheType, out Camera camera)
        {
            camera = null;
            var cache = cameraCaches.Find(x => x.cameraCacheType == cameraCacheType);
            if (cache == null || cache.targetCamera == null)
                return false;

            camera = cache.targetCamera;
            return true;
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

    [System.Serializable]
    public class CameraCacheData
    {
        public Camera targetCamera;
        public CameraCacheType cameraCacheType;

        public CameraCacheData() { }
        public CameraCacheData(CameraCacheType cameraCacheType)
        {
            this.cameraCacheType = cameraCacheType;
        }
    }

    public enum CameraCacheType
    {
        // Dont change the enum values of cameras... BEWARE

        MAIN_SCENE_CAMERA = 0,
        GLOBAL_UI_CAMERA = 1,
    }
}