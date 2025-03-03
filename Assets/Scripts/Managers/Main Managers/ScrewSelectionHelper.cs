using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

namespace Tag.NutSort
{
    public class ScrewSelectionHelper : Manager<ScrewSelectionHelper>
    {
        #region Events
        private List<Action<BaseScrew>> onScrewSelected = new List<Action<BaseScrew>>();
        private List<Action<BaseScrew>> onScrewDeselected = new List<Action<BaseScrew>>();
        private ScrewSelectionRules screwSelectionRules;
        #endregion

        #region Private Variables
        [ShowInInspector, ReadOnly] private BaseScrew currentSelectedScrew;
        #endregion

        #region Properties
        public BaseScrew CurrentSelectedScrew => currentSelectedScrew;
        public bool HasSelectedScrew => currentSelectedScrew != null;
        #endregion

        #region Unity Callbacks
        public override void Awake()
        {
            base.Awake();
            screwSelectionRules = new ScrewSelectionRules(OnScrewSelected);
        }

        #endregion

        #region Public Methods
        public void OnScrewClicked(BaseScrew baseScrew)
        {
            Vibrator.LightFeedback();
            if (HasSelectedScrew)
            {
                if (NutTransferHelper.Instance.CanTransferNuts(CurrentSelectedScrew, baseScrew))
                {
                    NutTransferHelper.Instance.TransferNuts(CurrentSelectedScrew, baseScrew);
                    ClearSelection();
                }
                else
                    HandleScrewSelection(baseScrew);
            }
            else
                HandleScrewSelection(baseScrew);
        }

        public void RegisterOnScrewSelected(Action<BaseScrew> action)
        {
            if (!onScrewSelected.Contains(action))
                onScrewSelected.Add(action);
        }

        public void DeRegisterOnScrewSelected(Action<BaseScrew> action)
        {
            if (onScrewSelected.Contains(action))
                onScrewSelected.Remove(action);
        }

        public void RegisterOnScrewDeselected(Action<BaseScrew> action)
        {
            if (!onScrewDeselected.Contains(action))
                onScrewDeselected.Add(action);
        }

        public void DeRegisterOnScrewDeselected(Action<BaseScrew> action)
        {
            if (onScrewDeselected.Contains(action))
                onScrewDeselected.Remove(action);
        }

        public void HandleScrewSelection(BaseScrew screw)
        {
            if (!HasSelectedScrew)
                screwSelectionRules.CheckRule(screw);
            else if (currentSelectedScrew == screw)
                DeselectCurrentScrew();
            else
            {
                // If selecting a different screw while one is already selected
                DeselectCurrentScrew();
                //screwSelectionRules.CheckRule(screw);
            }
        }

        public void ClearSelection()
        {
            currentSelectedScrew = null;
        }

        //public bool CanSelectScrew(BaseScrew screw)
        //{
        //    if (screw == null)
        //        return false;
        //    if (screw.ScrewInteractibilityState == ScrewInteractibilityState.Locked)
        //        return false;

        //    var nutsHolder = screw.GetScrewBehaviour<NutsHolderScrewBehaviour>();
        //    return nutsHolder != null && !nutsHolder.IsEmpty;
        //}

        public void ResetToLastMovedScrew(out GameplayMoveInfo lastMoveState)
        {
            lastMoveState = GameplayManager.Instance.GameplayStateData.GetLastGameplayMove();
            if (HasSelectedScrew)
                ClearSelection();
            currentSelectedScrew = LevelManager.Instance.GetScrewOfGridCell(lastMoveState.moveToScrew);

        }
        #endregion

        #region Private Methods
        private void InvokeOnScrewSelected(BaseScrew screw)
        {
            for (int i = 0; i < onScrewSelected.Count; i++)
                onScrewSelected[i]?.Invoke(screw);
        }

        private void InvokeOnScrewDeselected(BaseScrew screw)
        {
            for (int i = 0; i < onScrewDeselected.Count; i++)
                onScrewDeselected[i]?.Invoke(screw);
        }

        private void OnScrewSelected(BaseScrew screw)
        {
            currentSelectedScrew = screw;
            screw.LiftTheFirstNutAnimation();
            InvokeOnScrewSelected(screw);
        }

        private void DeselectCurrentScrew()
        {
            if (HasSelectedScrew)
            {
                BaseScrew screwToDeselect = currentSelectedScrew;
                screwToDeselect.PutBackFirstSelectedNutAnimation();
                ClearSelection();
                InvokeOnScrewDeselected(screwToDeselect);
            }
        }
        #endregion

        #region Unity Editor Methods
        #endregion
    }
}