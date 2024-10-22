using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tag.NutSort
{
    [RequireComponent(typeof(RectTransform))]
    public class RectTransformSizeFollower : MonoBehaviour
    {
        #region PUBLIC_VARIABLES
        public Vector2 sizeOffset;

        [Space]
        public bool hasMinSizeConstraint;
        public Vector2 minSize;
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private RectTransform targetFollowerTransform;
        private RectTransform myTransform;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        private void Awake()
        {
            AssignRect();
        }

        private void OnEnable()
        {
            UIUtilityEvents.onRefreshUIRects += UIUtilityEvents_onRefreshUIRects;

            UIUtilityEvents_onRefreshUIRects();
        }

        private void OnDisable()
        {
            UIUtilityEvents.onRefreshUIRects -= UIUtilityEvents_onRefreshUIRects;
        }
        #endregion

        #region PUBLIC_METHODS
        [Button]
        public void RefreshRectSize()
        {
            Vector2 targetSizeDelta = targetFollowerTransform.rect.size;
            myTransform.sizeDelta = targetSizeDelta + sizeOffset;

            if (hasMinSizeConstraint)
                myTransform.sizeDelta = new Vector2(Mathf.Max(minSize.x, myTransform.sizeDelta.x), Mathf.Max(minSize.y, myTransform.sizeDelta.y));
        }

        [Button]
        public void AssignRect()
        {
            if (myTransform == null)
                myTransform = gameObject.GetComponent<RectTransform>();
        }
        #endregion

        #region PRIVATE_METHODS
        #endregion

        #region EVENT_HANDLERS
        private void UIUtilityEvents_onRefreshUIRects()
        {
            RefreshRectSize();
        }
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }

    public static class UIUtilityEvents
    {
        public delegate void UIRectRefreshEvent();
        public static event UIRectRefreshEvent onRefreshUIRects;

        public static void RaiseOnRefreshUIRects()
        {
            if (onRefreshUIRects != null)
                onRefreshUIRects();
        }
    }
}