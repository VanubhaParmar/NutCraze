using System;
using UnityEngine;

namespace Tag.NutSort
{
    public class ScrewInputBehaviour : BaseScrewBehaviour
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private Transform inputTransform;
        private Action clickAction;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public void RegisterClickAction(Action clickAction)
        {
            this.clickAction = clickAction;
        }

        public void UnregisterClickAction()
        {
            this.clickAction = null;
        }

        public override void InitScrewBehaviour(BaseScrew myScrew)
        {
            base.InitScrewBehaviour(myScrew);
            SetScrewInputSize();
        }

        public void UpdateScrewInputSize()
        {
            SetScrewInputSize();
        }
        #endregion

        #region PRIVATE_METHODS
        private void SetScrewInputSize()
        {
            inputTransform.position = myScrew.GetBasePosition() + myScrew.ScrewDimensions.baseHeight * Vector3.down;
            inputTransform.localScale = new Vector3(1f, myScrew.GetTotalScrewApproxHeight(), 1f);
        }
        #endregion

        #region EVENT_HANDLERS
        public void OnScrewClick()
        {
            //Debug.Log("MysCrew Click : " +  myScrew.GridCellId.rowNumber + " - " + myScrew.GridCellId.colNumber);
            if (GameplayManager.Instance.GameplayStateData.gameplayStateType == GameplayStateType.PLAYING_LEVEL && myScrew.ScrewInteractibilityState == ScrewInteractibilityState.Interactable)
            {
                Vibrator.LightFeedback();
                GameplayManager.Instance.OnScrewClicked(myScrew);
                clickAction?.Invoke();
            }
        }
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}