using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Tag.TowerDefence
{
    public class AnimateObject : MonoBehaviour
    {
        #region PRIVATE_VARS

        private int currencyItemPoints = 0;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Image image;
        [SerializeField] private Canvas canvas;

        #endregion

        #region propertices

        public int CurrencyItemPoints
        {
            get { return currencyItemPoints; }
            set { currencyItemPoints = value; }
        }

        public RectTransform RectTransform => rectTransform;

        public Image Image => image;

        #endregion

        #region UNITY_CALLBACKS

        #endregion

        #region PUBLIC_FUNCTIONS

        public void SetDetails(int value)
        {
            CurrencyItemPoints = value;
        }

        public void SetSortingOrder(string layerName, int sortingOrder)
        {
            if (canvas == null)
                return;

            canvas.sortingLayerName = layerName;
            canvas.sortingOrder = sortingOrder;
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        #endregion

        #region CO-ROUTINES

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS

        #endregion
    }
}