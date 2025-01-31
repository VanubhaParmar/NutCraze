using System;
using UnityEngine;

namespace com.tag.nut_sort
{
    [CreateAssetMenu(menuName = "Merge Game/Currency TimeBase Energy")]
    public class CurrencyTimeBaseLife : CurrencyTimeBase
    {
        #region Ovreride_Methods
        public override int UnitTimeUpdate
        {
            get { return unitTimeUpdate; }
        }

        public override void Add(int value, Action successAction = null, Vector3 position = default(Vector3))
        {
            if (value >= 0)
            {
                base.Add(value, successAction, position);
                return;
            }

            value = Mathf.Abs(value);
            if (Value >= value)
            {
                base.Add(-value, successAction, position);
                successAction?.Invoke();
            }
        }

        public override bool IsSufficentValue(int value)
        {
            if (Value >= value)
            {
                return true;
            }
          //  GameplayUIManager.Instance.GetView<EnergyView>().Show(0);
            return false;
        }

        #endregion

        #region Public Function
        #endregion

        #region private Funcation
        #endregion
    }
}