using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Tag.NutSort
{
    public class CurrencyTimeBaseEnergyTopbarComponent : CurrencyTopbarComponent
    {
        #region PUBLIC_VARS

        #endregion

        #region PRIVATE_VARS

        [SerializeField] private GameObject infiniteLifeGO;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text timerInfiniteEnergyText;
        [SerializeField] private GameObject timerObject;
        private Coroutine coroutineEnergyAnimation;
        private int defaultValue;
        private CurrencyTimeBase currency;

        #endregion

        #region UNITY_CALLBACKS

        public override void OnEnable()
        {
            currency = DataManager.Instance.GetCurrency(currencyId).GetType<CurrencyTimeBase>();
            defaultValue = currency.defaultValue;

            base.OnEnable();
        }

        public override void Start()
        {
            base.Start();
        }

        #endregion

        #region PUBLIC_FUNCTIONS

        public override void RegisterCurrencyEvent()
        {
            base.RegisterCurrencyEvent();
            if (currency != null)
            {
                currency.RegisterOnCurrencyEarnedEvent(SetEarnEnergyValue);
                currency.RegisterTimerTick(OnEnergyTimeUpdate);
                currency.RegisterOnCurrencyUpdateByTimer(OnEnergyUpdateByTimer);
                currency.RegisterTimerStartOrStop(OnEnergyTimerStart);
                currency.RegisterOnInfiniteTimerStartOrStop(OnInfiniteEnergyTimerStart);
                currency.RegisterOnInfiniteTimerTick(OnInfiniteEnergyTimeUpdate);
            }
            currencyAnimation?.RegisterObjectAnimationComplete(SetEnergyAmount);
        }

        public override void DeRegisterCurrencyEvent()
        {
            base.DeRegisterCurrencyEvent();
            if (currency != null)
            {
                currency.RemoveOnCurrencyEarnedEvent(SetEarnEnergyValue);
                currency.RemoveTimerTick(OnEnergyTimeUpdate);
                currency.RemoveOnCurrencyUpdateByTimer(OnEnergyUpdateByTimer);
                currency.RemoveTimerStartOrStop(OnEnergyTimerStart);
                currency.RemoveOnInfiniteTimerStartOrStop(OnInfiniteEnergyTimerStart);
                currency.RemoveOnInfiniteTimerTick(OnInfiniteEnergyTimeUpdate);
            }
            currencyAnimation?.DeregisterObjectAnimationComplete(SetEnergyAmount);
        }

        public override void SetCurrencyValue(int value)
        {
            if (value >= defaultValue)
            {
                //currencyText.text = "<color=green>" + value + "</color>/" + defaultValue;
                currencyText.text = value.ToString();
                timerText.text = "Full";
                return;
            }
            currencyText.text = value.ToString();
        }

        public override void SetDeductedCurrencyValue(int value, Vector3 position)
        {
            if (coroutineEnergyAnimation != null)
                StopCoroutine(coroutineEnergyAnimation);
            coroutineEnergyAnimation = StartCoroutine(DoAnimateTopBarEnergyValueChange(0.65f, currencyVaue, currencyVaue - value));
            currencyVaue -= value;
        }

        public override void OnTryToBuyThisCurrency()
        {
            if (!currency.IsFull())
                MainSceneUIManager.Instance.GetView<NotEnoughLifeView>().ShowView(null, null);
            //else
            //GlobalUIManager.Instance.GetView<ToastMessageView>().ShowMessage($"Max {DataManager.Instance.GetCurrency(CurrencyConstant.ENERGY).currencyName}");
        }

        #endregion

        #region PRIVATE_FUNCTIONS

        private void SetEnergyAmount(int energyValue, bool isLastObject)
        {
            currencyVaue += energyValue;

            if (isLastObject)
            {
                SetCurrencyValue(currency.Value);
                return;
            }
            SetCurrencyValue(currency.Value);
        }

        private void SetEarnEnergyValue(int value, Vector3 position)
        {
            if (coroutineEnergyAnimation != null)
                StopCoroutine(coroutineEnergyAnimation);
            coroutineEnergyAnimation = StartCoroutine(DoAnimateTopBarEnergyValueChange(0.65f, currencyVaue, currencyVaue + value));
            currencyVaue += value;
        }

        private void OnEnergyTimeUpdate(TimeSpan timeSpan)
        {
            if (!timerObject.activeSelf)
                timerObject.SetActive(true);
            timerText.text = timeSpan.FormateTimeSpanForDaysInTwoDigit();
        }

        private void OnInfiniteEnergyTimeUpdate(TimeSpan timeSpan)
        {
            if (!infiniteLifeGO.activeSelf)
                infiniteLifeGO.SetActive(true);
            timerText.text = timeSpan.FormateTimeSpanForDaysInTwoDigit();
        }

        private void OnEnergyUpdateByTimer(int value)
        {
            currencyVaue = currency.Value;
            SetCurrencyValue(currencyVaue);
        }

        private void OnEnergyTimerStart(bool isStart, bool needToUpdate)
        {
            timerObject.SetActive(isStart);
            if (!isStart)
            {
                SetCurrencyValue(currencyVaue);
            }
        }

        private void OnInfiniteEnergyTimerStart(bool isStart)
        {
            infiniteLifeGO.SetActive(isStart);
            if (!isStart)
            {
                currencyVaue = currency.Value;
                SetCurrencyValue(currencyVaue);
            }
        }

        #endregion

        #region CO-ROUTINES

        private IEnumerator DoAnimateTopBarEnergyValueChange(float time, int startValue, int targetValue)
        {
            float i = 0;
            float rate = 1 / time;
            int tempValue = 0;
            while (i < 1)
            {
                i += Time.deltaTime * rate;
                tempValue = (int)Mathf.Lerp(startValue, targetValue, i);
                SetCurrencyValue(tempValue);
                yield return null;
            }
            SetCurrencyValue(targetValue);
        }

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       

        #endregion
    }
}
