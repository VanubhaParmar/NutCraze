using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening.Core.Easing;
using UnityEngine;

namespace Tag.NutSort
{
    [System.Serializable]
    public abstract class Currency : SerializedScriptableObject
    {
        #region public veriables

        [ReadOnly] public string key;
        public int defaultValue;

        public string currencyName;

        #endregion

        #region private veriables

        private List<Action<int>> onCurrencyChange = new List<Action<int>>();
        private List<Action<int, Vector3>> onCurrencySpend = new List<Action<int, Vector3>>();
        private List<Action<int, Vector3>> onCurrencyEarned = new List<Action<int, Vector3>>();
        private List<Action> onUnAuthenticatModificationDetected = new List<Action>();

        #endregion

        #region propertice

        [ShowInInspector]
        public int Value
        {
            get { return PlayerPrefbsHelper.GetInt(key, defaultValue); }
            protected set
            {
                if (IsValidCurrency())
                {
                    PlayerPrefbsHelper.SetInt(key, value);
                }
                else
                {
                    Debug.LogError("Currency Is Not Valid");
                    OnUnauthenticatedModification();
                }
            }
        }

        public int SavedValue => PlayerPrefbsHelper.GetSavedInt(key, defaultValue);

        #endregion

        #region virtual methods

        public virtual void Init()
        {
            PlayerPrefbsHelper.RegisterEvent(key, OnCurrencyValueChange);
        }

        public virtual void OnDestroy()
        {
            PlayerPrefbsHelper.DeregisterEvent(key, OnCurrencyValueChange);
        }

        [Button]
        public virtual void Add(int value, Action successAction = null, Vector3 position = default(Vector3))
        {
            Value = Mathf.Max(Value + value, 0);
            if (value > 0)
                OnCurrencyEarned(value, position);
            else
                OnCurrencySpend(Mathf.Abs(value), position);
        }

        public virtual void Delete()
        {
            Value = defaultValue;
        }

        public virtual void SetValue(int value)
        {
            Value = value;
        }

        public virtual bool HasEnoughValue(int requiredValue)
        {
            return Value >= requiredValue;
        }

        public virtual void RemoveAllCallback()
        {
            onCurrencyChange.Clear();
            onCurrencySpend.Clear();
            onCurrencyEarned.Clear();
            onUnAuthenticatModificationDetected.Clear();
        }

        public virtual T GetType<T>() where T : Currency
        {
            return (T)this;
        }

        #endregion

        #region private methods

        private bool IsValidCurrency()
        {
            return true;
        }

        #endregion

        #region public methods

        public void RegisterOnCurrencyChangeEvent(Action<int> action)
        {
            if (!onCurrencyChange.Contains(action))
            {
                onCurrencyChange.Add(action);
            }
        }

        public void RemoveOnCurrencyChangeEvent(Action<int> action)
        {
            if (onCurrencyChange.Contains(action))
            {
                onCurrencyChange.Remove(action);
            }
        }

        public void RegisterOnCurrencySpendEvent(Action<int, Vector3> action)
        {
            if (!onCurrencySpend.Contains(action))
            {
                onCurrencySpend.Add(action);
            }
        }

        public void RemoveOnCurrencySpendEvent(Action<int, Vector3> action)
        {
            if (onCurrencySpend.Contains(action))
            {
                onCurrencySpend.Remove(action);
            }
        }

        public void RegisterOnCurrencyEarnedEvent(Action<int, Vector3> action)
        {
            if (!onCurrencyEarned.Contains(action))
            {
                onCurrencyEarned.Add(action);
            }
        }

        public void RemoveOnCurrencyEarnedEvent(Action<int, Vector3> action)
        {
            if (onCurrencyEarned.Contains(action))
            {
                onCurrencyEarned.Remove(action);
            }
        }

        public void RegisterOnUnauthenticatedModificationDetected(Action action)
        {
            if (!onUnAuthenticatModificationDetected.Contains(action))
                onUnAuthenticatModificationDetected.Add(action);
        }

        public void RemoveOnUnauthenticatedModificationDetected(Action action)
        {
            if (onUnAuthenticatModificationDetected.Contains(action))
                onUnAuthenticatModificationDetected.Remove(action);
        }

        public virtual bool IsSufficentValue(int value)
        {
            if (Value >= value)
                return true;
            return false;
        }

        #endregion


        #region protected veriables

        protected void OnCurrencyValueChange()
        {
            for (int i = 0; i < onCurrencyChange.Count; i++)
            {
                onCurrencyChange[i]?.Invoke(Value);
            }
        }

        protected void OnCurrencySpend(int value, Vector3 position)
        {
            for (int i = 0; i < onCurrencySpend.Count; i++)
            {
                onCurrencySpend[i]?.Invoke(value, position);
            }
        }

        protected void OnCurrencyEarned(int value, Vector3 position)
        {
            for (int i = 0; i < onCurrencyEarned.Count; i++)
            {
                onCurrencyEarned[i]?.Invoke(value, position);
            }
        }

        protected void OnUnauthenticatedModification()
        {
            for (int i = 0; i < onUnAuthenticatModificationDetected.Count; i++)
            {
                onUnAuthenticatModificationDetected[i]?.Invoke();
            }
        }

        #endregion
#if UNITY_EDITOR
        [Button]
        public void SetKey(string key)
        {
            this.key = key;
        }
#endif
    }
}