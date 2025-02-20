using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort {
    public class TutorialHandSetter : MonoBehaviour
    {
        #region PUBLIC_VARIABLES
        public Transform handTargetTransform;
        public Vector3 offset;
        public Vector3 rotation;
        public bool canPlayTranslateAnimation;
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void SetHandAnimation()
        {
            TutorialElementHandler.Instance.SetActiveTapHand(true, handTargetTransform.position + offset, rotation, canPlayTranslateAnimation);
        }

        public void ResetHandAnimation()
        {
            TutorialElementHandler.Instance.tutorialHandAnimation.gameObject.SetActive(false);
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