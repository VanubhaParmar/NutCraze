using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort
{
    public class CurrencyTopbarComponents : BaseTopbarComponents
    {
        #region PUBLIC_VARS
        public int CurrencyId { get => currencyId; }
        public Image CurrencyImage => currencyImage;
        //public CurrencyAnimation CurrencyAnimation { get => currencyAnimation; }

        #endregion

        #region PRIVATE_VARS

        [SerializeField, CurrencyId] protected int currencyId;
        [SerializeField] protected Text currencyText;
        [SerializeField] protected Image currencyImage;
        [SerializeField] protected Transform endTransform;
        //[SerializeField] protected CurrencyAnimation currencyAnimation;
        protected int currencyVaue;

        protected Coroutine currencySetCoroutine;
        #endregion

        #region UNITY_CALLBACKS

        public override void Start()
        {
            base.Start();
        }

        public override void OnEnable()
        {
            base.OnEnable();
            RegisterCurrencyEvent();
            currencyVaue = DataManager.Instance.GetCurrency(currencyId).Value;
            SetCurrencyValue(currencyVaue);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            DeRegisterCurrencyEvent();

            if (currencySetCoroutine != null)
            {
                StopCoroutine(currencySetCoroutine);
                currencySetCoroutine = null;
            }
        }

        public virtual void SetCurrencyValue(int value)
        {
            currencyVaue = value;
            currencyText.text = value.ToString();
        }

		public void SetCurrencyValue(bool animate = false, float animTime = 0.65f, int target = -1)
		{
            if (target < 0)
                target = DataManager.Instance.GetCurrency(currencyId).Value;

            if (animate)
            {
                if (currencySetCoroutine == null)
                {
                    currencySetCoroutine = StartCoroutine(DoAnimateTopBarValueChange(animTime, currencyVaue, target, currencyText));
                    currencyVaue = target;
                }
            }
            else
                SetCurrencyValue(target);
        }
        #endregion

        #region PUBLIC_FUNCTIONS

        public virtual void RegisterCurrencyEvent()
        {
            DataManager.Instance.GetCurrency(currencyId).RegisterOnCurrencySpendEvent(SetDeductedCurrencyValue);
            //currencyAnimation?.RegisterObjectAnimationComplete(SetCurrencyAmount);
        }

        public virtual void DeRegisterCurrencyEvent()
        {
            DataManager.Instance.GetCurrency(currencyId).RemoveOnCurrencySpendEvent(SetDeductedCurrencyValue);
            //currencyAnimation?.DeregisterObjectAnimationComplete(SetCurrencyAmount);
        }

        public virtual void SetDeductedCurrencyValue(int value, Vector3 position)
        {
            StartCoroutine(DoAnimateTopBarValueChange(0.65f, currencyVaue, DataManager.Instance.GetCurrency(currencyId).Value, currencyText));
            currencyVaue -= value;
        }


        #endregion

        #region PRIVATE_FUNCTIONS


        private void SetCurrencyAmount(int value, bool isLastObject)
        {
            currencyVaue += value;
            currencyText.text = currencyVaue.ToString();
            if (isLastObject)
            {
                currencyVaue = DataManager.Instance.GetCurrency(currencyId).Value;
                currencyText.text = currencyVaue.ToString();
            }
        }

        #endregion

        #region CO-ROUTINES

        private IEnumerator DoAnimateTopBarValueChange(float time, int startValue, int targetValue, Text textTopBarComponent)
        {
            float i = 0;
            float rate = 1 / time;

            while (i < 1)
            {
                i += Time.deltaTime * rate;

                textTopBarComponent.text = "" + (int)Mathf.Lerp(startValue, targetValue, i);
                yield return null;
            }
            textTopBarComponent.text = "" + targetValue;

            currencySetCoroutine = null;
        }

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       

        //[Button]
        //public void OnCoinButtonClick(Transform transform, bool isReverseAnimation)
        //{
        //    DataManager.Instance.GetCurrency(currencyId).Add(10);
        //    currencyAnimation?.UIStartAnimation(transform.position, endTransform.position, 10, isReverseAnimation);
        //}

        //public virtual void OnTryToBuyThisCurrency()
        //{
        //    DataManager.Instance.TryToGetThisCurrency(currencyId);
        //}
        #endregion
    }
}
