using System;

namespace Tag.NutSort
{
    [Serializable]
    public abstract class NutTransferRule
    {
        public abstract bool CanTransfer(BaseScrew fromScrew, BaseScrew toScrew);
    }

    [Serializable]
    public class NotNullRule : NutTransferRule
    {
        public override bool CanTransfer(BaseScrew fromScrew, BaseScrew toScrew)
        {
            return fromScrew != null && toScrew != null;
        }
    }

    [Serializable]
    public class DifferentScrewRule : NutTransferRule
    {
        public override bool CanTransfer(BaseScrew fromScrew, BaseScrew toScrew)
        {
            return fromScrew != toScrew;
        }
    }

    [Serializable]
    public class HasSpaceInTargetRule : NutTransferRule
    {
        public override bool CanTransfer(BaseScrew fromScrew, BaseScrew toScrew)
        {
            return toScrew.TryGetScrewBehaviour(out NutsHolderScrewBehaviour toHolder) && toHolder.CanAddNut;
        }
    }

    [Serializable]
    public class NutColorMatchRule : NutTransferRule
    {
        public override bool CanTransfer(BaseScrew fromScrew, BaseScrew toScrew)
        {
            if (toScrew.TryGetScrewBehaviour(out NutsHolderScrewBehaviour toHolder) && toHolder.IsEmpty)
                return true; // If empty, allow any color.

            NutsHolderScrewBehaviour fromHolder = fromScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();
            return fromHolder.PeekNut().GetNutColorType() == toHolder.PeekNut().GetNutColorType();
        }
    }
}
