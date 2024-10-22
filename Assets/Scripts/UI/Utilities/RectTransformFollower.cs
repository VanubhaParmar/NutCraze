using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Tag.NutSort
{
    [RequireComponent(typeof(RectTransform))]
    public class RectTransformFollower : MonoBehaviour
    {
        #region PUBLIC_VARIABLES
        public Vector2 followOffset;
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
        public void RefreshRectPos()
        {
            Vector2 targetPos = targetFollowerTransform.position;
            myTransform.position = targetPos + followOffset;
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
            RefreshRectPos();
        }
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}