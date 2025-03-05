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
            if (!CanTransferNuts(fromScrew, toScrew))
                return;

            int totalNutsTransferred = ExecuteTransfer(fromScrew, toScrew);

            InvokeOnNutTransferComplete(fromScrew, toScrew, totalNutsTransferred);

            fromScrew.CheckForSurpriseNutColorReveal();
            toScrew.CheckForScrewSortCompletion();
        }

        public void ResetToLastMovedScrew()
        {
            ScrewSelectionHelper.Instance.ResetToLastMovedScrew(out var lastMoveState);

            BaseScrew currentSelectedScrew = ScrewSelectionHelper.Instance.CurrentSelectedScrew;

            if (currentSelectedScrew.IsSorted())
            {
                DOTween.Kill(lastMoveState.moveToScrew);
                currentSelectedScrew.CapAnimation.gameObject.SetActive(false);
                currentSelectedScrew.SetScrewInteractableState(ScrewInteractibilityState.Interactable);
                currentSelectedScrew.StopStackFullIdlePS();

                NutsHolderScrewBehaviour currentSelectedScrewNutsHolder = currentSelectedScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();
                int firstNutColorId = currentSelectedScrewNutsHolder.PeekNut().GetNutColorType();
                GameplayManager.Instance.GameplayStateData.levelNutsUniqueColorsSortCompletionState[firstNutColorId] = false;
            }


            RetransferNutFromCurrentSelectedScrewTo(LevelManager.Instance.GetScrewOfGridCell(lastMoveState.moveFromScrew), lastMoveState.transferredNumberOfNuts);
            GameplayManager.Instance.GameplayStateData.CalculatePossibleNumberOfMoves();
        }


        #endregion

        #region Private Methods
        private int ExecuteTransfer(BaseScrew fromScrew, BaseScrew toScrew)
        {
            var fromHolder = fromScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();
            var toHolder = toScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();

            BaseNut firstNut = TransferFirstNut(fromHolder, toHolder, fromScrew, toScrew);

            int additionalNuts = TransferMatchingNuts(fromHolder, toHolder, firstNut, fromScrew, toScrew);
            return 1 + additionalNuts;
        }

        private BaseNut TransferFirstNut(NutsHolderScrewBehaviour fromHolder, NutsHolderScrewBehaviour toHolder,
            BaseScrew fromScrew, BaseScrew toScrew)
        {
            BaseNut firstNut = fromHolder.PopNut();
            toHolder.AddNut(firstNut, false);
            VFXManager.Instance.TransferThisNutFromStartScrewTopToEndScrew(firstNut, fromScrew, toScrew, toHolder);
            return firstNut;
        }

        private int TransferMatchingNuts(NutsHolderScrewBehaviour fromHolder, NutsHolderScrewBehaviour toHolder,
            BaseNut firstNut, BaseScrew fromScrew, BaseScrew toScrew)
        {
            int nutsTransferred = 0;

            while (CanTransferMoreNuts(fromHolder, toHolder, firstNut))
            {
                BaseNut nextNut = fromHolder.PopNut();
                toHolder.AddNut(nextNut, false);
                VFXManager.Instance.TransferThisNutFromStartScrewToEndScrew(nextNut, nutsTransferred, fromScrew, toScrew);
                nutsTransferred++;
            }

            return nutsTransferred;
        }

        private bool CanTransferMoreNuts(NutsHolderScrewBehaviour fromHolder, NutsHolderScrewBehaviour toHolder, BaseNut referenceNut)
        {
            if (!toHolder.CanAddNut || fromHolder.CurrentNutCount <= 0)
                return false;

            return fromHolder.PeekNut().GetNutColorType() == referenceNut.GetNutColorType();
        }

        private void RetransferNutFromCurrentSelectedScrewTo(BaseScrew baseScrew, int nutsCountToTransfer)
        {
            BaseScrew currentSelectedScrew = ScrewSelectionHelper.Instance.CurrentSelectedScrew;
            NutsHolderScrewBehaviour currentSelectedScrewNutsHolder = currentSelectedScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();
            NutsHolderScrewBehaviour targetScrewNutsHolder = baseScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();

            BaseNut lastNut = currentSelectedScrewNutsHolder.PopNut();
            targetScrewNutsHolder.AddNut(lastNut, false);

            VFXManager.Instance.TransferThisNutFromStartScrewTopToEndScrew(lastNut, currentSelectedScrew, baseScrew, targetScrewNutsHolder);

            int extraNutIndex = 0;
            nutsCountToTransfer--;

            while (nutsCountToTransfer > 0)
            {
                BaseNut extraNut = currentSelectedScrewNutsHolder.PopNut();
                targetScrewNutsHolder.AddNut(extraNut, false);

                VFXManager.Instance.TransferThisNutFromStartScrewToEndScrew(extraNut, extraNutIndex, currentSelectedScrew, baseScrew); // Transfer all other nuts
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