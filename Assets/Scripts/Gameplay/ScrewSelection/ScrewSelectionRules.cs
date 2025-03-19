using System;
using System.Collections.Generic;

namespace Tag.NutSort
{
    public class ScrewSelectionRules
    {
        private List<IScrewSelectionRule> screwSelectionRules = new List<IScrewSelectionRule>();
        private Action<BaseScrew> onScrewSelected;
        public ScrewSelectionRules(Action<BaseScrew> onScrewSelected)
        {
            this.onScrewSelected = onScrewSelected;
            screwSelectionRules.Add(new IsNotNullRule());
            screwSelectionRules.Add(new IsNotLockedRule());
            screwSelectionRules.Add(new ScrewNotEmptyRule());
        }

        public void CheckRule(BaseScrew screw)
        {
            foreach (var rule in screwSelectionRules)
            {
                if (!rule.CanSelect(screw))
                    return;
            }
            onScrewSelected.Invoke(screw);
        }
    }

    public interface IScrewSelectionRule
    {
        public bool CanSelect(BaseScrew screw);
    }

    public class IsNotNullRule : IScrewSelectionRule
    {
        public bool CanSelect(BaseScrew screw)
        {
            return screw != null;
        }

    }
    public class IsNotLockedRule : IScrewSelectionRule
    {
        public bool CanSelect(BaseScrew screw)
        {
            return screw.ScrewState != ScrewState.Locked;
        }
    }

    public class ScrewNotEmptyRule : IScrewSelectionRule
    {
        public bool CanSelect(BaseScrew screw)
        {
            return !screw.IsEmpty;
        }
    }
}
