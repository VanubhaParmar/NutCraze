using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

namespace Tag.NutSort
{
    [System.Serializable]
    public class BoolSetting : SerializedScriptableObject
    {
        #region public veriables

        [ReadOnly] public string key;
        public bool defaultValue;

        public string settingName;

        #endregion

        #region private veriables

        private List<Action<bool>> onSettingChange = new List<Action<bool>>();
        private List<Action> onSettingEnabled = new List<Action>();
        private List<Action> onSettingDisabled = new List<Action>();

        #endregion

        #region propertice

        [ShowInInspector]
        public bool Value
        {
            get
            {
                return (PlayerPrefbsHelper.GetInt(key, defaultValue ? 1 : 0) == 1);
            }
            protected set
            {
                PlayerPrefbsHelper.SetInt(key, value ? 1 : 0);
            }
        }

        public bool SavedValue => PlayerPrefbsHelper.GetSavedInt(key, defaultValue ? 1 : 0) == 1;

        #endregion

        #region virtual methods

        public virtual void Init()
        {
            PlayerPrefbsHelper.RegisterEvent(key, OnSettingChanged);
        }

        public virtual void OnDestroy()
        {
            PlayerPrefbsHelper.DeregisterEvent(key, OnSettingChanged);
        }

        [Button]
        public virtual void Toggle()
        {
            SetValue(!Value);
        }

        public virtual void Enable()
        {
            if (!Value)
            {
                Value = true;
                OnSettingEnabled();
            }
        }

        public virtual void Disable()
        {
            if (Value)
            {
                Value = false;
                OnSettingDisabled();
            }
        }

        public virtual void Reset()
        {
            Value = defaultValue;
        }

        public virtual void SetValue(bool value)
        {
            bool oldValue = Value;
            Value = value;

            if (oldValue != value)
            {
                if (value)
                    OnSettingEnabled();
                else
                    OnSettingDisabled();
            }
        }

        public virtual void Delete()
        {
            Value = defaultValue;
        }

        public virtual void RemoveAllCallback()
        {
            onSettingChange.Clear();
            onSettingEnabled.Clear();
            onSettingDisabled.Clear();
        }

        public virtual T GetType<T>() where T : BoolSetting
        {
            return (T)this;
        }

        #endregion

        #region private methods
        #endregion

        #region public methods

        public void RegisterOnSettingChange(Action<bool> action)
        {
            if (!onSettingChange.Contains(action))
                onSettingChange.Add(action);
        }

        public void RemoveOnSettingChange(Action<bool> action)
        {
            if (onSettingChange.Contains(action))
                onSettingChange.Remove(action);
        }

        public void RegisterOnSettingEnabled(Action action)
        {
            if (!onSettingEnabled.Contains(action))
                onSettingEnabled.Add(action);
        }

        public void RemoveOnSettingEnabled(Action action)
        {
            if (onSettingEnabled.Contains(action))
                onSettingEnabled.Remove(action);
        }

        public void RegisterOnSettingDisabled(Action action)
        {
            if (!onSettingDisabled.Contains(action))
                onSettingDisabled.Add(action);
        }

        public void RemoveOnSettingDisabled(Action action)
        {
            if (onSettingDisabled.Contains(action))
                onSettingDisabled.Remove(action);
        }
        #endregion


        #region protected veriables

        protected void OnSettingChanged()
        {
            for (int i = 0; i < onSettingChange.Count; i++)
                onSettingChange[i]?.Invoke(Value);
        }

        protected void OnSettingEnabled()
        {
            for (int i = 0; i < onSettingEnabled.Count; i++)
                onSettingEnabled[i]?.Invoke();
        }

        protected void OnSettingDisabled()
        {
            for (int i = 0; i < onSettingDisabled.Count; i++)
                onSettingDisabled[i]?.Invoke();
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
