using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Tag.NutSort
{
    public class GameplayInputButton : MonoBehaviour
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        [SerializeField] UnityEvent onClickEvent;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (GameplayManager.Instance.IsPlayingLevel)
                onClickEvent?.Invoke();
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