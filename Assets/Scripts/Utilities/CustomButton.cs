using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Tag.NutSort
{
    [AddComponentMenu("UI/CustomButton", 30)]
    public class CustomButton : Button
    {
        #region static veriables
        private static bool isButtonClick;


        #endregion

        #region overrided methods

        //protected override void OnValidate()
        //{
        //    if (!gameObject.GetComponent<ButtonClickAnimation>())
        //        gameObject.AddComponent<ButtonClickAnimation>();

        //    if (!gameObject.GetComponent<ButtonClickSound>())
        //        gameObject.AddComponent<ButtonClickSound>();

        //    if (!gameObject.GetComponent<ButtonClickHaptic>())
        //        gameObject.AddComponent<ButtonClickHaptic>();
        //}

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (isButtonClick)
                return;
            isButtonClick = true;
            base.OnPointerClick(eventData);

            CoroutineRunner.Instance.Wait(0.2f, () =>
            {
                isButtonClick = false;
            });
        }

        #endregion

        #region coroutine

        #endregion
    }
}