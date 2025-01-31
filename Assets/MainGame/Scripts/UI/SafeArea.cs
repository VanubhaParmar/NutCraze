using UnityEngine;

namespace com.tag.nut_sort {
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        #region PUBLIC_VARIABLES

        #endregion

        #region PRIVATE_VARIABLES

        private RectTransform panelRect;

        #endregion

        #region UNITY_CALLBACKS

        private void Awake()
        {
            if (panelRect == null)
                panelRect = gameObject.GetComponent<RectTransform>();
            ApplySafeArea();
        }

        #endregion

        #region PUBLIC_FUNCTIONS

        #endregion

        #region PRIVATE_FUNCTIONS

        private void ApplySafeArea()
        {
            Rect safeArea = Screen.safeArea;
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            panelRect.anchorMin = anchorMin;
            panelRect.anchorMax = anchorMax;
        }

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region COROUTINES

        #endregion

        #region UI_CALLBACKS

        #endregion
    }
}