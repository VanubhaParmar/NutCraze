using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;

namespace Tag.NutSort
{
    public class CameraSizeHandler : Manager<CameraSizeHandler>
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
        public void Start()
        {
            StartCoroutine(WaitForManagerToLoad(() =>
            {
                LevelManager.Instance.RegisterOnLevelLoad(InitializeSize);
            }));
        }
        public override void OnDestroy()
        {
            LevelManager.Instance.DeRegisterOnLevelLoad(InitializeSize);
        }
        #endregion

        #region PUBLIC_FUNCTIONS
        [Button]
        public void InitializeSize()
        {
            CameraCache.TryFetchCamera(changeCameraType, out Camera myCam);

            LevelArrangementConfigDataSO levelArrangementConfigDataSO = LevelManager.Instance.GetCurrentLevelArrangementConfig();
            myCam.transform.position = GetGridCentrePositionOnCameraCollider(levelArrangementConfigDataSO.GetCentrePosition());

            Vector2 finalGridSize = GetGridRequiredSizeOnCameraCollider(levelArrangementConfigDataSO) + levelSizeOffset;
            finalGridSize.x = Mathf.Max(minimumGameplayBounds.size.x, finalGridSize.x);
            finalGridSize.y = Mathf.Max(minimumGameplayBounds.size.y, finalGridSize.y);

            requiredGameplayBounds.size = finalGridSize;

            Vector2 requiredWidthLimits = new Vector2(requiredGameplayBounds.transform.position.x - requiredGameplayBounds.size.x / 2, requiredGameplayBounds.transform.position.x + requiredGameplayBounds.size.x / 2);
            Vector2 requiredHeightLimits = new Vector2(requiredGameplayBounds.transform.position.y - requiredGameplayBounds.size.y / 2, requiredGameplayBounds.transform.position.y + requiredGameplayBounds.size.y / 2);

            float requiredWidth = requiredWidthLimits.y - requiredWidthLimits.x;
            float requiredHeight = requiredHeightLimits.y - requiredHeightLimits.x;

            myCam.orthographicSize = (requiredWidth / myCam.aspect) / 2;

            float camHeightInWorldScale = myCam.orthographicSize * 2;
            if (camHeightInWorldScale < requiredHeight)
            {
                myCam.orthographicSize = requiredHeight / 2;
            }

            Vector2 maxWidthLimits = new Vector2(maximumGameplayBounds.transform.position.x - maximumGameplayBounds.size.x / 2, maximumGameplayBounds.transform.position.x + maximumGameplayBounds.size.x / 2);
            Vector2 maxHeightLimits = new Vector2(maximumGameplayBounds.transform.position.y - maximumGameplayBounds.size.y / 2, maximumGameplayBounds.transform.position.y + maximumGameplayBounds.size.y / 2);

            float maxWidth = maxWidthLimits.y - maxWidthLimits.x;
            float maxHeight = maxHeightLimits.y - maxHeightLimits.x;

            if (myCam.orthographicSize * myCam.aspect * 2 > maxWidth || myCam.orthographicSize * 2 > maxHeight)
            {
                float maxOrthographicSize = Mathf.Min(maxWidth / (myCam.aspect * 2), maxHeight / 2);
                myCam.orthographicSize = maxOrthographicSize;
            }

            Vector3 camPos = myCam.transform.position;
            camPos.x = Mathf.Clamp(camPos.x, maxWidthLimits.x, maxWidthLimits.y);
            camPos.y = Mathf.Clamp(camPos.y, maxHeightLimits.x, maxHeightLimits.y);
            myCam.transform.position = camPos;
        }

        public void DoCameraShake()
        {
            transform.DOShakePosition(0.2f, 0.1f, 40);
        }
        #endregion

        #region PRIVATE_FUNCTIONS
        private Vector3 GetGridCentrePositionOnCameraCollider(Vector3 levelCentrePosition)
        {
            CameraCache.TryFetchCamera(changeCameraType, out Camera myCam);

            Vector3 centreCamPosition = transform.position;

            Ray ray = new Ray(levelCentrePosition, -myCam.transform.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100.0f, boxRaycastMask))
                centreCamPosition = hit.point;

            return centreCamPosition;
        }

        private Vector2 GetGridRequiredSizeOnCameraCollider(LevelArrangementConfigDataSO arrangementConfig)
        {
            Vector3 halfCellSize = new Vector3(arrangementConfig.arrangementCellSize.x / 2f, arrangementConfig.arrangementCellSize.y / 2f);

            Vector3 firstCellPos = arrangementConfig.GetCellPosition(GridCellId.Zero) - halfCellSize;

            GridCellId lastCellId;
            lastCellId.rowNumber = arrangementConfig.arrangementGridSize.x - 1;
            lastCellId.colNumber = arrangementConfig.arrangementGridSize.y - 1;

            Vector3 lastCellPos = arrangementConfig.GetCellPosition(lastCellId) + halfCellSize;

            float xDist = lastCellPos.x - firstCellPos.x;
            float yDist = Mathf.Sqrt(Vector3.SqrMagnitude(lastCellPos - firstCellPos) - Mathf.Pow(xDist, 2));

            return new Vector2(xDist, yDist);
        }
        #endregion

        #region COROUTINES
        private IEnumerator WaitForManagerToLoad(Action onLoad)
        {
            while (LevelManager.Instance == null)
                yield return 0;
            onLoad?.Invoke();
        }
        #endregion
    }
}