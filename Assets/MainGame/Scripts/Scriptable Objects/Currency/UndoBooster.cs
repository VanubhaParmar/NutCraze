using System;
using UnityEngine;

namespace Tag.NutSort {
    [CreateAssetMenu(fileName = "UndoBooster", menuName = Constant.GAME_NAME + "/Currency/UndoBooster")]
    public class UndoBooster : Currency
    {
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
    }
}
