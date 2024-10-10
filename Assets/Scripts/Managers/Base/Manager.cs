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
                return managerInstanceLoader.loaded;
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
                return managerInstanceLoader.loaded;
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
            if (mapViews.ContainsKey(typeof(TBaseView)))
                return (TBaseView)mapViews[typeof(TBaseView)];
            return null;
        }

        public void ShowView<TBaseView>() where TBaseView : BaseView
        {
            if (mapViews.ContainsKey(typeof(TBaseView)))
                mapViews[typeof(TBaseView)].Show();
        }

        public void HideView<TBaseView>() where TBaseView : BaseView
        {
            if (mapViews.ContainsKey(typeof(TBaseView)) && mapViews[typeof(TBaseView)].IsActive)
                mapViews[typeof(TBaseView)].Hide();
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
            if (!mapViews.ContainsKey(baseView.GetType()))
            {
                baseView.Init();

                views.Add(baseView);
                mapViews.Add(baseView.GetType(), baseView);
            }
        }

        public void RemoveView(BaseView baseView)
        {
            if (mapViews.ContainsKey(baseView.GetType()))
                mapViews.Remove(baseView.GetType());
        }
        #endregion

        #region private methods

        private void MapViews()
        {
            for (int i = 0; i < views.Count; i++)
            {
                if (!mapViews.ContainsKey(views[i].GetType()))
                {
                    views[i].Init();
                    mapViews.Add(views[i].GetType(), views[i]);
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