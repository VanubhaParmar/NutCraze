using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class BoosterAnimationView : SerializedMonoBehaviour
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private BoosterAnimationObject boosterAnimationObjecyPrefab;

        private List<BoosterAnimationObject> boosterAnimationObjectPools = new List<BoosterAnimationObject>();
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void PlayBoosterClaimAnimation(BoosterType boosterType, int boosterCount, Vector3 startPosition, Action actionToCallOnOver = null)
        {
            var toast = boosterAnimationObjectPools.Find(x => !x.gameObject.activeInHierarchy);
            if (toast == null)
                toast = CreateBoosterObject();
            toast.PlayAnimation(boosterType, boosterCount, startPosition, actionToCallOnOver);
        }
        #endregion

        #region PRIVATE_METHODS
        private BoosterAnimationObject CreateBoosterObject()
        {
            BoosterAnimationObject boosterObj = Instantiate(boosterAnimationObjecyPrefab, transform);
            boosterAnimationObjectPools.Add(boosterObj);

            return boosterObj;
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