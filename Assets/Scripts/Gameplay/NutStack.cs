using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace Tag.NutSort
{
    public class NutStack
    {
        [ShowInInspector] public List<BaseNut> nutsHolder = new List<BaseNut>();
       
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
