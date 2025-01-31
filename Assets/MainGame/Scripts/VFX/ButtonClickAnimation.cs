using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace com.tag.nut_sort {
    [RequireComponent(typeof(Button))]
    public class ButtonClickAnimation : MonoBehaviour
    {
        #region PUBLIC_VARS
        public bool overrideTransform;
        [ShowIf("overrideTransform")] public Transform targetTransform;
        #endregion

        #region PRIVATE_VARS

        Vector3 defaultScale = Vector3.one;

        #endregion

        #region UNITY_CALLBACKS

        void Awake()
        {
            GetComponent<Button>().onClick.AddListener(ClickAnimation);
        }

        #endregion

        #region PUBLIC_FUNCTIONS

        #endregion

        #region PRIVATE_FUNCTIONS

        private void ClickAnimation()
        {
            Transform target = targetTransform == null ? transform : targetTransform;

            target.localScale = defaultScale;
            target.DOPunchScale(-Vector3.one * 0.1f, 0.15f, 4);
        }

        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS

        #endregion
    }
}