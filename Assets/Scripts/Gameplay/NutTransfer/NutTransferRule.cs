namespace Tag.NutSort
{
    public interface INutTransferRule
    {
        public bool CanTransfer(BaseScrew fromScrew, BaseScrew toScrew);
    }

    public class NotNullRule : INutTransferRule
    {
        public bool CanTransfer(BaseScrew fromScrew, BaseScrew toScrew)
        {
            return fromScrew != null && toScrew != null;
        }
    }

    public class DifferentScrewRule : INutTransferRule
    {
        public bool CanTransfer(BaseScrew fromScrew, BaseScrew toScrew)
        {
            return fromScrew != toScrew;
        }
    }

    public class HasSpaceInTargetRule : INutTransferRule
    {
        public bool CanTransfer(BaseScrew fromScrew, BaseScrew toScrew)
        {
            return toScrew.TryGetScrewBehaviour(out NutsHolderScrewBehaviour toHolder) && toHolder.CanAddNut;
        }
    }

    public class NutColorMatchRule : INutTransferRule
    {
        public bool CanTransfer(BaseScrew fromScrew, BaseScrew toScrew)
        {
            if (toScrew.TryGetScrewBehaviour(out NutsHolderScrewBehaviour toHolder) && toHolder.IsEmpty)
                return true; // If empty, allow any color.

            NutsHolderScrewBehaviour fromHolder = fromScrew.GetScrewBehaviour<NutsHolderScrewBehaviour>();
            return fromHolder.PeekNut().GetNutColorType() == toHolder.PeekNut().GetNutColorType();
        }
    }
}
