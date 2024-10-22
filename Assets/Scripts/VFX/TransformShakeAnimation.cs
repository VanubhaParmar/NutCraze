using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class TransformShakeAnimation : MonoBehaviour
    {
        #region PUBLIC_VARIABLES
        public float duration = 0.2f;
        public float strength = 0.1f;
        public int vibrato = 40;
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        [Button]
        public void DoShake()
        {
            transform.DOShakePosition(duration, strength, vibrato);
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