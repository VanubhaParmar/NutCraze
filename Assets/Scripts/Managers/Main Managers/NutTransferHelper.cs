using BuildReportTool.Window.Screen;
using System;
using System.Collections.Generic;

namespace Tag.NutSort
{
    public class NutTransferHelper : Manager<NutTransferHelper>
    {
        #region Events
        private List<Action<BaseScrew, BaseScrew, int>> onNutTransferComplete = new List<Action<BaseScrew, BaseScrew, int>>();
        #endregion

        #region Private Variables
        #endregion

        #region Properties
        #endregion

        #region OVERRIDED_METHODS
        public override void Awake()
        {
            base.Awake();
            RegisterEvents();
        }

        public override void OnDestroy()
        {
            DeRegisterEvents();
            base.OnDestroy();
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
            if (!ValidateScrewsForTransfer(fromScrew, toScrew))
                return false;

            if (toScrew.TryGetScrewBehaviour(out NutsHolderScrewBehaviour toHolder) && toHolder.CanAddNut)
            {
                if (toHolder.IsEmpty)
                    return true;
                else
                {
                    NutsHolderScrewBehaviour fromHolder = fromScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();
                    return fromHolder.PeekNut().GetNutColorType() == toHolder.PeekNut().GetNutColorType();
                }

            }
            return false;
        }

        public void TransferNuts(BaseScrew fromScrew, BaseScrew toScrew)
        {
            if (!CanTransferNuts(fromScrew, toScrew))
                return;
            var fromHolder = fromScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();
            var toHolder = toScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();

            int totalNutsTransferred = ExecuteTransfer(fromHolder, toHolder, fromScrew, toScrew);
            InvokeOnNutTransferComplete(fromScrew, toScrew, totalNutsTransferred);
        }
        #endregion

        #region Private Methods
        private void RegisterEvents()
        {
            ScrewSelectionHelper.Instance.RegisterOnScrewSelected(PlayNutSelectionAnimation);
            ScrewSelectionHelper.Instance.RegisterOnScrewDeselected(ResetNutSelectionAnimation);
        }

        private void DeRegisterEvents()
        {
            ScrewSelectionHelper.Instance.DeRegisterOnScrewSelected(PlayNutSelectionAnimation);
            ScrewSelectionHelper.Instance.DeRegisterOnScrewDeselected(ResetNutSelectionAnimation);
        }

        private void PlayNutSelectionAnimation(BaseScrew screw)
        {
            VFXManager.Instance.LiftTheFirstSelectionNut(screw);
        }

        private void ResetNutSelectionAnimation(BaseScrew screw)
        {
            VFXManager.Instance.ResetTheFirstSelectionNut(screw);
        }

        private bool ValidateScrewsForTransfer(BaseScrew fromScrew, BaseScrew toScrew)
        {
            if (fromScrew == null || toScrew == null || fromScrew == toScrew)
            {
                return false;
            }
            return true;
        }

        private int ExecuteTransfer(NutsHolderScrewBehaviour fromHolder, NutsHolderScrewBehaviour toHolder,
            BaseScrew fromScrew, BaseScrew toScrew)
        {
            BaseNut firstNut = TransferFirstNut(fromHolder, toHolder, fromScrew, toScrew);

            int additionalNuts = TransferMatchingNuts(fromHolder, toHolder, firstNut, fromScrew, toScrew);
            return 1 + additionalNuts;
        }

        private BaseNut TransferFirstNut(NutsHolderScrewBehaviour fromHolder, NutsHolderScrewBehaviour toHolder,
            BaseScrew fromScrew, BaseScrew toScrew)
        {
            BaseNut firstNut = fromHolder.PopNut();
            toHolder.AddNut(firstNut, false);
            VFXManager.Instance.TransferThisNutFromStartScrewTopToEndScrew(firstNut, fromScrew, toScrew);
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

        private void InvokeOnNutTransferComplete(BaseScrew fromScrew, BaseScrew toScrew, int nutsTransferred)
        {
            foreach (var handler in onNutTransferComplete)
                handler?.Invoke(fromScrew, toScrew, nutsTransferred);
        }
        #endregion
    }
}