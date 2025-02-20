using System.Collections;
using TMPro;
using UnityEngine;

namespace Tag.NutSort
{
    public class CurrencyTopbarComponent : BaseTopbarComponent
    {
        #region PRIVATE_VARS
        [SerializeField, CurrencyId] protected int currencyId;
        [SerializeField] protected TMP_Text currencyText;
        [SerializeField] protected Transform endTransform;
        [SerializeField] protected CurrencyAnimation currencyAnimation;
        [SerializeField] protected int currencyVaue;
        #endregion

        #region PROPERTIES
        public int CurrencyId => currencyId;
        public Transform EndPos => endTransform;
        public CurrencyAnimation CurrencyAnimation => currencyAnimation;
        #endregion

        #region UNITY_CALLBACKS

        public override void Start()
        {
            base.Start();
            Init();
        }


        public override void OnEnable()
        {
            base.OnEnable();
            Init();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            DeRegisterCurrencyEvent();
        }

        public virtual void SetCurrencyValue(int value)
        {
            currencyVaue = value;
            currencyText.text = value.ToString();
        }

        public void SetCurrencyValue()
        {
            currencyText.text = DataManager.Instance.GetCurrency(currencyId).Value.ToString();
        }

        #endregion

        #region PUBLIC_FUNCTIONS

        private void Init()
        {
            RegisterCurrencyEvent();
            currencyVaue = DataManager.Instance.GetCurrency(currencyId).Value;
            SetCurrencyValue(currencyVaue);
        }

        public virtual void RegisterCurrencyEvent()
        {
            DataManager.Instance.GetCurrency(currencyId).RegisterOnCurrencySpendEvent(SetDeductedCurrencyValue);
            currencyAnimation?.RegisterObjectAnimationComplete(SetCurrencyAmount);
        }

        public virtual void DeRegisterCurrencyEvent()
        {
            DataManager.Instance.GetCurrency(currencyId).RemoveOnCurrencySpendEvent(SetDeductedCurrencyValue);
            currencyAnimation?.DeregisterObjectAnimationComplete(SetCurrencyAmount);
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
            currencyVaue = Mathf.Clamp(currencyVaue, 0, DataManager.Instance.GetCurrency(currencyId).Value);
            currencyText.text = currencyVaue.ToString();
            if (isLastObject)
            {
                currencyVaue = DataManager.Instance.GetCurrency(currencyId).Value;
                currencyText.text = currencyVaue.ToString();
            }
        }

        #endregion

        #region CO-ROUTINES

        private IEnumerator DoAnimateTopBarValueChange(float time, int startValue, int targetValue, TMP_Text textTopBarComponent)
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
        }

        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region UI_CALLBACKS       

        public virtual void OnTryToBuyThisCurrency()
        {
            //BottombarView bottombarView = MainSceneUIManager.Instance.GetView<BottombarView>();
            //if (bottombarView.IsActive)
            //{
            //    bottombarView.OnTapButton(BottomBarButtonsType.Shop);
            //    bottombarView.OnShopButtonClick();
            //}
            //else
            //{
            //    MainSceneUIManager.Instance.GetView<ShopView>().Show();
            //}
        }

        #endregion
    }
}
