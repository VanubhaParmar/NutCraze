using Sirenix.OdinInspector;
using UnityEngine;

namespace Tag.NutSort
{
    public class CameraSizeHandler : MonoBehaviour
    {
        #region PUBLIC_VARIABLES
        public CameraCacheType changeCameraType;

        [Header("Bounds Settings")]
        public SpriteRenderer requiredGameplayBounds;
        public SpriteRenderer maximumGameplayBounds;

        #endregion

        #region UNITY_CALLBACKS
        private void Start()
        {
            InitializeSize();
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        [Button]
        public void InitializeSize()
        {
            CameraCache.TryFetchCamera(changeCameraType, out Camera myCam);

            myCam.transform.position = requiredGameplayBounds.transform.position;

            Vector2 requiredWidthLimits = new Vector2(requiredGameplayBounds.transform.position.x - requiredGameplayBounds.transform.localScale.x / 2, requiredGameplayBounds.transform.position.x + requiredGameplayBounds.transform.localScale.x / 2);
            Vector2 requiredHeightLimits = new Vector2(requiredGameplayBounds.transform.position.y - requiredGameplayBounds.transform.localScale.y / 2, requiredGameplayBounds.transform.position.y + requiredGameplayBounds.transform.localScale.y / 2);

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

            Vector2 maxWidthLimits = new Vector2(maximumGameplayBounds.transform.position.x - maximumGameplayBounds.transform.localScale.x / 2, maximumGameplayBounds.transform.position.x + maximumGameplayBounds.transform.localScale.x / 2);
            Vector2 maxHeightLimits = new Vector2(maximumGameplayBounds.transform.position.y - maximumGameplayBounds.transform.localScale.y / 2, maximumGameplayBounds.transform.position.y + maximumGameplayBounds.transform.localScale.y / 2);

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
            myCam.transform.position = new Vector3(myCam.transform.position.x, myCam.transform.position.y, -10);
        }

        #endregion
    }
}