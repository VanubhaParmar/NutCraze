using DG.Tweening;
using System;
using System.Collections.Generic;

namespace Tag.NutSort
{
    public class NutTransferHelper : Manager<NutTransferHelper>
    {
        #region Events
        private List<Action<BaseScrew, BaseScrew, int>> onNutTransferComplete = new List<Action<BaseScrew, BaseScrew, int>>();
        private List<INutTransferRule> nutTransferRules = new List<INutTransferRule>();
        #endregion

        #region Private Variables
        #endregion

        #region Properties
        #endregion

        #region OVERRIDED_METHODS
        public override void Awake()
        {
            base.Awake();
            nutTransferRules = new List<INutTransferRule>()
            {
                new NotNullRule(),
                new DifferentScrewRule(),
                new HasSpaceInTargetRule(),
                new NutColorMatchRule(),
            };
        }
        #endregion

        #region Public Methods
        public void RegisterOnNutTransferComplete(Action<BaseScrew, BaseScrew, int> action)
        {
            if (!onNutTransferComplete.Contains(action))
                onNutTransferComplete.Add(action);
        }

        public void DeRegisterOnNutTransferComplete(Action<BaseScrew, BaseScrew, int> action)
        {
            if (onNutTransferComplete.Contains(action))
                onNutTransferComplete.Remove(action);
        }

        public bool CanTransferNuts(BaseScrew fromScrew, BaseScrew toScrew)
        {
            foreach (var rule in nutTransferRules)
            {
                if (!rule.CanTransfer(fromScrew, toScrew))
                    return false;
            }
            return true;
        }

        public void TransferNuts(BaseScrew fromScrew, BaseScrew toScrew)
        {
            int totalNutsTransferred = ExecuteTransfer(fromScrew, toScrew);

            var move = new MoveData(fromScrew.GridCellId, toScrew.GridCellId, totalNutsTransferred);
            LevelProgressManager.Instance.OnPlayerMoveConfirmed(move);

            fromScrew.CheckForSurpriseNutColorReveal();
            toScrew.CheckForScrewSortCompletion();

            InvokeOnNutTransferComplete(fromScrew, toScrew, totalNutsTransferred);
        }

        public void ResetToLastMovedScrew()
        {
            ScrewSelectionHelper.Instance.ResetToLastMovedScrew(out var lastMoveState);

            BaseScrew currentSelectedScrew = ScrewSelectionHelper.Instance.CurrentSelectedScrew;

            if (currentSelectedScrew.IsSorted())
            {
                DOTween.Kill(lastMoveState.toScrew);
                currentSelectedScrew.CapAnimation.gameObject.SetActive(false);
                currentSelectedScrew.SetScrewInteractableState(ScrewState.Interactable);
                currentSelectedScrew.StopStackFullIdlePS();

                int firstNutColorId = currentSelectedScrew.PeekNut().GetNutColorType();
                GameplayManager.Instance.GameplayStateData.levelNutsUniqueColorsSortCompletionState[firstNutColorId] = false;
            }


            RetransferNutFromCurrentSelectedScrewTo(ScrewManager.Instance.GetScrew(lastMoveState.fromScrew), lastMoveState.transferedNuts);
            GameplayManager.Instance.GameplayStateData.CalculatePossibleNumberOfMoves();
            LevelFailManager.Instance.CheckForLevelFail();
        }


        #endregion

        #region Private Methods
        private int ExecuteTransfer(BaseScrew fromScrew, BaseScrew toScrew)
        {
            BaseNut firstNut = TransferFirstNut(fromScrew, toScrew);

            int additionalNuts = TransferMatchingNuts(firstNut, fromScrew, toScrew);
            return 1 + additionalNuts;
        }

        private BaseNut TransferFirstNut(BaseScrew fromScrew, BaseScrew toScrew)
        {
            BaseNut firstNut = fromScrew.PopNut();
            toScrew.AddNut(firstNut, false);
            VFXManager.Instance.TransferThisNutFromStartScrewTopToEndScrew(firstNut, fromScrew, toScrew);
            return firstNut;
        }

        private int TransferMatchingNuts(BaseNut firstNut, BaseScrew fromScrew, BaseScrew toScrew)
        {
            int nutsTransferred = 0;

            while (CanTransferMoreNuts(fromScrew, toScrew, firstNut))
            {
                BaseNut nextNut = fromScrew.PopNut();
                toScrew.AddNut(nextNut, false);
                VFXManager.Instance.TransferThisNutFromStartScrewToEndScrew(nextNut, nutsTransferred, fromScrew, toScrew);
                nutsTransferred++;
            }

            return nutsTransferred;
        }

        private bool CanTransferMoreNuts(BaseScrew fromScrew, BaseScrew toScrew, BaseNut referenceNut)
        {
            if (!toScrew.CanAddNut || fromScrew.CurrentNutCount <= 0)
                return false;

            return fromScrew.PeekNut().GetNutColorType() == referenceNut.GetNutColorType();
        }

        private void RetransferNutFromCurrentSelectedScrewTo(BaseScrew toScrew, int nutsCountToTransfer)
        {
            BaseScrew fromScrew = ScrewSelectionHelper.Instance.CurrentSelectedScrew;

            BaseNut lastNut = fromScrew.PopNut();
            toScrew.AddNut(lastNut, false);

            VFXManager.Instance.TransferThisNutFromStartScrewTopToEndScrew(lastNut, fromScrew, toScrew);

            int extraNutIndex = 0;
            nutsCountToTransfer--;

            while (nutsCountToTransfer > 0)
            {
                BaseNut extraNut = fromScrew.PopNut();
                toScrew.AddNut(extraNut, false);

                VFXManager.Instance.TransferThisNutFromStartScrewToEndScrew(extraNut, extraNutIndex, fromScrew, toScrew); // Transfer all other nuts
                extraNutIndex++;
                nutsCountToTransfer--;
            }

            ScrewSelectionHelper.Instance.ClearSelection();

        }
        private void InvokeOnNutTransferComplete(BaseScrew fromScrew, BaseScrew toScrew, int nutsTransferred)
        {
            foreach (var handler in onNutTransferComplete)
                handler?.Invoke(fromScrew, toScrew, nutsTransferred);
        }
        #endregion
    }
}