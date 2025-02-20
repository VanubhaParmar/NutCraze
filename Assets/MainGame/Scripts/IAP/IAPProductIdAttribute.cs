using System.Diagnostics;

namespace Tag.NutSort {
    [Conditional("UNITY_EDITOR")]
    public class IAPProductIdAttribute : BaseIdAttribute
    {
        public IAPProductIdAttribute()
        {
        }
    }
}