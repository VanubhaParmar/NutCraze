using System;
using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(fileName = "CurrencyTimeBaseLife", menuName = Constant.GAME_NAME + "/Currency/CurrencyTimeBaseLife")]
    public class CurrencyTimeBaseLife : CurrencyTimeBase
    {
        #region Private Variables
        #endregion

        #region Properties
        #endregion

        #region Override Methods

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

            if (IsInfiniteCurrencyActive)
            {
                successAction?.Invoke();
                return;
            }
            
            value = Mathf.Abs(value);
            if (Value >= value)
            {
                base.Add(-value, successAction, position);
                successAction?.Invoke();
            }
        }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods

        #endregion

        #region COROUTINES
        #endregion
    }
}