using System.Collections.Generic;

namespace Tag.NutSort
{
    public class NutStack
    {
        public int Count => nutsHolder.Count;
        public List<BaseNut> nutsHolder = new List<BaseNut>(); // First nut in the list will always be the bottom most nut of the screw
        public int stackCapacity;
        public NutStack() { }
        public NutStack(int stackCapacity)
        {
            this.stackCapacity = stackCapacity;
        }

        public void Push(BaseNut baseNut)
        {
            nutsHolder.Add(baseNut);
        }

        public BaseNut Pop()
        {
            return nutsHolder.PopAt(nutsHolder.Count - 1);
        }

        public BaseNut Peek(int count)
        {
            int selectionIndex = nutsHolder.Count - 1 - count;
            if (selectionIndex >= 0)
                return nutsHolder[selectionIndex];
            return null;
        }

        public int GetNutIndex(BaseNut baseNut)
        {
            return nutsHolder.IndexOf(baseNut);
        }

        public void ResetStack()
        {
            nutsHolder.Clear();
        }
    }
}
