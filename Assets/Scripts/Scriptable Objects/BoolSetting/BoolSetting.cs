//using Sirenix.OdinInspector;
//using System.Collections.Generic;
//using System;
//using UnityEngine;

//namespace Tag.NutSort
//{
//    [System.Serializable]
//    public abstract class BoolSetting : SerializedScriptableObject
//    {
//        #region public veriables

//        [ReadOnly] public string key;
//        public bool defaultValue;

//        public string settingName;

//        #endregion

//        #region private veriables

//        private List<Action<bool>> onSettingChange = new List<Action<bool>>();
//        private List<Action<bool> onSettingEnabled = new List<Action<bool>>();
//        private List<Action<bool> onSettingDisabled = new List<Action<bool, Vector3>>();
//        private List<Action> onUnAuthenticatModificationDetected = new List<Action>();

//        #endregion

//        #region propertice

//        [ShowInInspector]
//        public bool Value
//        {
//            get { return (PlayerPrefbsHelper.GetInt(key, defaultValue ? 1 : 0) == 1); }
//            protected set
//            {
//                if (IsValidSetting())
//                {
//                    PlayerPrefbsHelper.SetInt(key, value ? 1 : 0);
//                }
//                else
//                {
//                    Debug.LogError("Currency Is Not Valid");
//                    OnUnauthenticatedModification();
//                }
//            }
//        }

//        public int SavedValue => PlayerPrefbsHelper.GetSavedInt(key, defaultValue);

//        #endregion

//        #region virtual methods

//        public virtual void Init()
//        {
//            PlayerPrefbsHelper.RegisterEvent(key, OnCurrencyValueChange);
//        }

//        public virtual void OnDestroy()
//        {
//            PlayerPrefbsHelper.DeregisterEvent(key, OnCurrencyValueChange);
//        }
//        [Button]
//        public virtual void Toggle()
//        {
//            SetValue(!Value);
//        }

//        public virtual void Enable()
//        {
//            if (!Value)
//            {
//                Value = true;
//                OnSettingEnabled(true);
//            }
//        }

//        public virtual void Disable()
//        {
//            if (Value)
//            {
//                Value = false;
//                OnSettingDisabled(false);
//            }
//        }

//        public virtual void Reset()
//        {
//            Value = defaultValue;
//        }

//        public virtual void SetValue(bool value)
//        {
//            bool oldValue = Value;
//            Value = value;

//            if (oldValue != value)
//            {
//                if (value)
//                    OnSettingEnabled(value);
//                else
//                    OnSettingDisabled(value);
//            }

//        }
//        [Button]
//        public virtual void Add(int value, Action successAction = null, Vector3 position = default(Vector3))
//        {
//            Value = Mathf.Max(Value + value, 0);
//            if (value > 0)
//                OnCurrencyEarned(value, position);
//            else
//                OnCurrencySpend(Mathf.Abs(value), position);
//        }

//        public virtual void Delete()
//        {
//            Value = defaultValue;
//        }

//        public virtual void SetValue(bool value)
//        {
//            Value = value;
//        }
       
//        public virtual void RemoveAllCallback()
//        {
//            onSettingChange.Clear();
//            onSettingEnabled.Clear();
//            onSettingDisabled.Clear();
//            onUnAuthenticatModificationDetected.Clear();
//        }

//        public virtual T GetType<T>() where T : BoolSetting
//        {
//            return (T)this;
//        }

//        #endregion

//        #region private methods

//        private bool IsValidSetting()
//        {
//            return true;
//        }

//        #endregion

//        #region public methods

//        public void RegisterOnCurrencyChangeEvent(Action<bool> action)
//        {
//            if (!onSettingChange.Contains(action))
//            {
//                onSettingChange.Add(action);
//            }
//        }

//        public void RemoveOnCurrencyChangeEvent(Action<bool> action)
//        {
//            if (onSettingChange.Contains(action))
//            {
//                onSettingChange.Remove(action);
//            }
//        }

//        public void RegisterOnSettingEnabled(Action<bool, Vector3> action)
//        {
//            if (!onSettingEnabled.Contains(action))
//            {
//                onSettingEnabled.Add(action);
//            }
//        }

//        public void RemoveOnSettingEnabled(Action<bool, Vector3> action)
//        {
//            if (onSettingEnabled.Contains(action))
//            {
//                onSettingEnabled.Remove(action);
//            }
//        }

//        public void RegisterOnSettingDisabled(Action<bool, Vector3> action)
//        {
//            if (!onSettingDisabled.Contains(action))
//            {
//                onSettingDisabled.Add(action);
//            }
//        }

//        public void RemoveOnSettingDisabled(Action<bool, Vector3> action)
//        {
//            if (onSettingDisabled.Contains(action))
//            {
//                onSettingDisabled.Remove(action);
//            }
//        }

//        public void RegisterOnUnauthenticatedModificationDetected(Action action)
//        {
//            if (!onUnAuthenticatModificationDetected.Contains(action))
//                onUnAuthenticatModificationDetected.Add(action);
//        }

//        public void RemoveOnUnauthenticatedModificationDetected(Action action)
//        {
//            if (onUnAuthenticatModificationDetected.Contains(action))
//                onUnAuthenticatModificationDetected.Remove(action);
//        }
//        #endregion


//        #region protected veriables

//        protected void OnCurrencyValueChange()
//        {
//            for (int i = 0; i < onSettingChange.Count; i++)
//            {
//                onSettingChange[i]?.Invoke(Value);
//            }
//        }

//        protected void OnSettingEnabled(bool value)
//        {
//            for (int i = 0; i < onSettingEnabled.Count; i++)
//            {
//                onSettingEnabled[i]?.Invoke(value);
//            }
//        }

//        protected void OnCurrencyEarned(bool value, Vector3 position)
//        {
//            for (int i = 0; i < onSettingDisabled.Count; i++)
//            {
//                onSettingDisabled[i]?.Invoke(value, position);
//            }
//        }

//        protected void OnUnauthenticatedModification()
//        {
//            for (int i = 0; i < onUnAuthenticatModificationDetected.Count; i++)
//            {
//                onUnAuthenticatModificationDetected[i]?.Invoke();
//            }
//        }

//        #endregion
//#if UNITY_EDITOR
//        [Button]
//        public void SetKey(string key)
//        {
//            this.key = key;
//        }
//#endif
//    }
//}
