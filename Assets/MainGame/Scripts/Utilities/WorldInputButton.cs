using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Tag.NutSort {
    public class WorldInputButton : MonoBehaviour,IPointerClickHandler
    {
        #region PUBLIC_VARS

        #endregion

        #region PRIVATE_VARS
        [SerializeField] UnityEvent onClickEvent;

        public void OnPointerClick(PointerEventData eventData)
        {
            onClickEvent?.Invoke();
        }

        #endregion

        #region UNITY_CALLBACKS
       
        #endregion

        #region PUBLIC_FUNCTIONS

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
