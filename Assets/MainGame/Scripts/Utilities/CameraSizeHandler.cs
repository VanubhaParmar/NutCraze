using Sirenix.OdinInspector;
using UnityEngine;

namespace com.tag.nut_sort {
    public class CameraSizeHandler : MonoBehaviour
    {
        #region PUBLIC_VARIABLES
        public CameraCacheType changeCameraType;

        [Space]
        public LayerMask boxRaycastMask;
        public Vector2 levelSizeOffset;

        [Header("Bounds Settings")]
        public SpriteRenderer requiredGameplayBounds;
        public SpriteRenderer minimumGameplayBounds;
        public SpriteRenderer maximumGameplayBounds;

        #endregion

        #region UNITY_CALLBACKS
        private void OnEnable()
        {
            LevelManager.onLevelLoadOver += LevelManager_onLevelLoadOver;
        }

        private void OnDisable()
        {
            LevelManager.onLevelLoadOver -= LevelManager_onLevelLoadOver;
        }
        #endregion

        #region PUBLIC_FUNCTIONS
        [Button]
        public void InitializeSize()
        {
            CameraCache.TryFetchCamera(changeCameraType, out Camera myCam);

            myCam.transform.position = GetGridCentrePositionOnCameraCollider();

            Vector2 finalGridSize = GetGridRequiredSizeOnCameraCollider() + levelSizeOffset;
            finalGridSize.x = Mathf.Max(minimumGameplayBounds.size.x, finalGridSize.x);
            finalGridSize.y = Mathf.Max(minimumGameplayBounds.size.y, finalGridSize.y);

            requiredGameplayBounds.size = finalGridSize;

            Vector2 requiredWidthLimits = new Vector2(requiredGameplayBounds.transform.position.x - requiredGameplayBounds.size.x / 2, requiredGameplayBounds.transform.position.x + requiredGameplayBounds.size.x / 2);
            Vector2 requiredHeightLimits = new Vector2(requiredGameplayBounds.transform.position.y - requiredGameplayBounds.size.y / 2, requiredGameplayBounds.transform.position.y + requiredGameplayBounds.size.y / 2);

            float requiredWidth = requiredWidthLimits.y - requiredWidthLimits.x;
            float requiredHeight = requiredHeightLimits.y - requiredHeightLimits.x;

            // Set camera size based on required width
            myCam.orthographicSize = (requiredWidth / myCam.aspect) / 2;

            // Check if height is still less than required height
            float camHeightInWorldScale = myCam.orthographicSize * 2;
            if (camHeightInWorldScale < requiredHeight)
            {
                // Set camera size based on required height
                myCam.orthographicSize = requiredHeight / 2;
            }

            Vector2 maxWidthLimits = new Vector2(maximumGameplayBounds.transform.position.x - maximumGameplayBounds.size.x / 2, maximumGameplayBounds.transform.position.x + maximumGameplayBounds.size.x / 2);
            Vector2 maxHeightLimits = new Vector2(maximumGameplayBounds.transform.position.y - maximumGameplayBounds.size.y / 2, maximumGameplayBounds.transform.position.y + maximumGameplayBounds.size.y / 2);

            float maxWidth = maxWidthLimits.y - maxWidthLimits.x;
            float maxHeight = maxHeightLimits.y - maxHeightLimits.x;

            // Check if camera size exceeds maximum bounds
            if (myCam.orthographicSize * myCam.aspect * 2 > maxWidth || myCam.orthographicSize * 2 > maxHeight)
            {
                // Set camera size based on maximum bounds
                float maxOrthographicSize = Mathf.Min(maxWidth / (myCam.aspect * 2), maxHeight / 2);
                myCam.orthographicSize = maxOrthographicSize;
            }

            // Check if camera position is within maximum bounds
            Vector3 camPos = myCam.transform.position;
            camPos.x = Mathf.Clamp(camPos.x, maxWidthLimits.x, maxWidthLimits.y);
            camPos.y = Mathf.Clamp(camPos.y, maxHeightLimits.x, maxHeightLimits.y);
            myCam.transform.position = camPos;
            //myCam.transform.position = new Vector3(myCam.transform.position.x, myCam.transform.position.y, -10);
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        private Vector3 GetGridCentrePositionOnCameraCollider()
        {
            CameraCache.TryFetchCamera(changeCameraType, out Camera myCam);

            Vector3 levelCentrePosition = LevelManager.Instance.CurrentLevelDataSO.levelArrangementConfigDataSO.GetCentrePosition();
            Vector3 centreCamPosition = transform.position;

            Ray ray = new Ray(levelCentrePosition, -myCam.transform.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100.0f, boxRaycastMask))
                centreCamPosition = hit.point;

            return centreCamPosition;
        }

        private Vector2 GetGridRequiredSizeOnCameraCollider()
        {
            var arrangementConfig = LevelManager.Instance.CurrentLevelDataSO.levelArrangementConfigDataSO;
            Vector3 halfCellSize = new Vector3(arrangementConfig.arrangementCellSize.x / 2f, arrangementConfig.arrangementCellSize.y / 2f);

            Vector3 firstCellPos = arrangementConfig.GetCellPosition(new GridCellId(0, 0)) - halfCellSize;
            Vector3 lastCellPos = arrangementConfig.GetCellPosition(new GridCellId(arrangementConfig.arrangementGridSize.x - 1, arrangementConfig.arrangementGridSize.y - 1)) + halfCellSize;

            float xDist = lastCellPos.x - firstCellPos.x;
            float yDist = Mathf.Sqrt(Vector3.SqrMagnitude(lastCellPos - firstCellPos) - Mathf.Pow(xDist, 2));

            return new Vector2(xDist, yDist);
        }

        private void LevelManager_onLevelLoadOver()
        {
            InitializeSize();
        }
        #endregion
    }
}