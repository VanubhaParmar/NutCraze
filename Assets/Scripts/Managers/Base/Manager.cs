using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

namespace Tag.NutSort
{
    public class Manager<T> : MonoBehaviour where T : Manager<T>
    {
        public static T Instance;
        private ManagerInstanceLoader managerInstanceLoader;

        public bool IsLoaded
        {
            get
            {
                if (managerInstanceLoader == null)
                    return true;
                return managerInstanceLoader.Loaded;
            }
        }

        public virtual void Awake()
        {
            managerInstanceLoader = GetComponent<ManagerInstanceLoader>();
            OnLoadingStart();
            if (!Instance)
            {
                Instance = this as T;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public virtual void OnDestroy()
        {
            if (Instance)
            {
                Destroy(Instance);
            }
        }

        public void OnLoadingStart()
        {
            if (managerInstanceLoader != null)
                managerInstanceLoader.OnLoadingStart();
        }

        public void OnLoadingDone()
        {
            if (managerInstanceLoader != null)
                managerInstanceLoader.OnLoadingDone();
        }
    }

    public class SerializedManager<T> : SerializedMonoBehaviour where T : SerializedManager<T>
    {
        public static T Instance;
        private ManagerInstanceLoader managerInstanceLoader;

        public bool IsLoaded
        {
            get
            {
                if (managerInstanceLoader == null)
                    return true;
                return managerInstanceLoader.Loaded;
            }
        }

        public virtual void Awake()
        {
            managerInstanceLoader = GetComponent<ManagerInstanceLoader>();
            OnLoadingStart();
            if (!Instance)
            {
                Instance = this as T;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public virtual void OnDestroy()
        {
            if (Instance)
            {
                Destroy(Instance);
            }
        }

        public void OnLoadingStart()
        {
            if (managerInstanceLoader != null)
                managerInstanceLoader.OnLoadingStart();
        }

        public void OnLoadingDone()
        {
            if (managerInstanceLoader != null)
                managerInstanceLoader.OnLoadingDone();
        }
    }

    public class UIManager<T> : Manager<T> where T : UIManager<T>
    {
        #region public veriables

        [SerializeField] public List<BaseView> views = new List<BaseView>();

        #endregion

        #region private veriables

        private Dictionary<Type, BaseView> mapViews = new Dictionary<Type, BaseView>();

        #endregion

        #region unity callback

        public override void Awake()
        {
            base.Awake();
            MapViews();
        }
        #endregion

        #region public methods

        public TBaseView GetView<TBaseView>() where TBaseView : BaseView
        {
            Type type = typeof(TBaseView);
            if (mapViews.ContainsKey(type))
                return (TBaseView)mapViews[type];
            return null;
        }

        public void ShowView<TBaseView>() where TBaseView : BaseView
        {
            Type type = typeof(TBaseView);
            if (mapViews.ContainsKey(type))
                mapViews[type].Show();
        }

        public void HideView<TBaseView>() where TBaseView : BaseView
        {
            Type type = typeof(TBaseView);
            if (mapViews.ContainsKey(type) && mapViews[type].IsActive)
                mapViews[type].Hide();
        }

        public void ShowView(Type type)
        {
            if (mapViews.ContainsKey(type))
                mapViews[type].Show();
        }

        public void HideView(Type type)
        {
            if (mapViews.ContainsKey(type))
                mapViews[type].Hide();
        }

        public void MapView(BaseView baseView)
        {
            Type type = baseView.GetType();
            if (!mapViews.ContainsKey(type))
            {
                baseView.Init();
                views.Add(baseView);
                mapViews.Add(type, baseView);
            }
        }

        public void RemoveView(BaseView baseView)
        {
            Type type = baseView.GetType();
            if (mapViews.ContainsKey(type))
                mapViews.Remove(type);
        }
        #endregion

        #region private methods

        private void MapViews()
        {
            for (int i = 0; i < views.Count; i++)
            {
                BaseView baseView = views[i];
                Type type = baseView.GetType();
                if (!mapViews.ContainsKey(type))
                {
                    baseView.Init();
                    mapViews.Add(type, baseView);
                }
            }
        }
        #endregion
    }

    public enum LoadingType
    {
        auto,
        Manual,
    }
}