using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Tag.NutSort
{
    public class NutTransferHelper : SerializedMonoBehaviour
    {
        #region Events
        public event Action<BaseScrew, BaseScrew, int> OnNutTransferComplete;
        public event Action<BaseScrew> OnNutTransferStart;
        #endregion

        #region Private Variables
        [SerializeField] private MainGameplayAnimator gameplayAnimator;
        private bool isTransferInProgress;
        #endregion

        #region Properties
        public bool IsTransferInProgress => isTransferInProgress;
        #endregion

        #region Public Methods
        public void Initialize(MainGameplayAnimator animator)
        {
            gameplayAnimator = animator;
        }

        public bool CanTransferNuts(BaseScrew fromScrew, BaseScrew toScrew)
        {
            if (isTransferInProgress || fromScrew == null || toScrew == null || fromScrew == toScrew)
                return false;

            var fromHolder = fromScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();
            var toHolder = toScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();

            if (fromHolder == null || toHolder == null || fromHolder.IsEmpty || !toHolder.CanAddNut)
                return false;

            if (fromScrew.ScrewInteractibilityState == ScrewInteractibilityState.Locked ||
                toScrew.ScrewInteractibilityState == ScrewInteractibilityState.Locked)
                return false;

            // If target screw is empty, transfer is valid
            if (toHolder.IsEmpty)
                return true;

            // Check if colors match
            return fromHolder.PeekNut().GetNutColorType() == toHolder.PeekNut().GetNutColorType();
        }

        public void TransferNuts(BaseScrew fromScrew, BaseScrew toScrew)
        {
            if (!CanTransferNuts(fromScrew, toScrew))
                return;

            isTransferInProgress = true;
            OnNutTransferStart?.Invoke(fromScrew);

            var fromHolder = fromScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();
            var toHolder = toScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();

            // Transfer first nut with animation
            BaseNut firstNut = fromHolder.PopNut();
            toHolder.AddNut(firstNut, false);
            PlayFirstNutTransferAnimation(firstNut, fromScrew, toScrew);

            // Transfer additional matching nuts
            int additionalNuts = TransferMatchingNuts(fromHolder, toHolder, firstNut, fromScrew, toScrew);

            // Complete transfer
            isTransferInProgress = false;
            OnNutTransferComplete?.Invoke(fromScrew, toScrew, 1 + additionalNuts);
        }

        public void PlayNutSelectionAnimation(BaseScrew screw)
        {
            if (gameplayAnimator != null && !isTransferInProgress)
            {
                gameplayAnimator.LiftTheFirstSelectionNut(screw);
            }
        }

        public void ResetNutSelectionAnimation(BaseScrew screw)
        {
            if (gameplayAnimator != null)
            {
                gameplayAnimator.ResetTheFirstSelectionNut(screw);
            }
        }
        #endregion

        #region Private Methods
        private int TransferMatchingNuts(NutsHolderScrewBehaviour fromHolder, NutsHolderScrewBehaviour toHolder, 
            BaseNut firstNut, BaseScrew fromScrew, BaseScrew toScrew)
        {
            int nutsTransferred = 0;

            while (CanTransferMoreNuts(fromHolder, toHolder, firstNut))
            {
                BaseNut nextNut = fromHolder.PopNut();
                toHolder.AddNut(nextNut, false);
                PlayAdditionalNutTransferAnimation(nextNut, nutsTransferred, fromScrew, toScrew);
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

        private void PlayFirstNutTransferAnimation(BaseNut nut, BaseScrew fromScrew, BaseScrew toScrew)
        {
            if (gameplayAnimator != null)
            {
                gameplayAnimator.TransferThisNutFromStartScrewTopToEndScrew(nut, fromScrew, toScrew);
            }
        }

        private void PlayAdditionalNutTransferAnimation(BaseNut nut, int nutIndex, BaseScrew fromScrew, BaseScrew toScrew)
        {
            if (gameplayAnimator != null)
            {
                gameplayAnimator.TransferThisNutFromStartScrewToEndScrew(nut, nutIndex, fromScrew, toScrew);
            }
        }
        #endregion

        #region Unity Editor Methods
        [Button]
        private void FindGameplayAnimator()
        {
            if (gameplayAnimator == null)
            {
                gameplayAnimator = FindObjectOfType<MainGameplayAnimator>();
            }
        }
        #endregion
    }
}