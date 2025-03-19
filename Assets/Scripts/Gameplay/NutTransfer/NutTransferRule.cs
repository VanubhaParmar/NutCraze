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
            return toScrew.CanAddNut;
        }
    }

    public class NutColorMatchRule : INutTransferRule
    {
        public bool CanTransfer(BaseScrew fromScrew, BaseScrew toScrew)
        {
            if (toScrew.IsEmpty)
                return true;
            return fromScrew.PeekNut().GetNutColorType() == toScrew.PeekNut().GetNutColorType();
        }
    }
}
