using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    public class BaseView : SerializedMonoBehaviour
    {
        #region private veriabels
        [SerializeField] private bool blockRayCast;
        [SerializeField] private bool manageQueue;
        [SerializeField] private bool enableBackButton = true;
        [SerializeField] private bool hasSubview;
        [SerializeField] public BaseUiAnimation viewAnimation;

        private Coroutine coroutine;
        private bool canShow;
        private bool canHide;
        private bool isAnimationInProgress;

        #endregion

        #region public static veriables

        [ShowInInspector, ReadOnly] public static List<BaseView> backPressableViews = new List<BaseView>();
        [ShowInInspector, ReadOnly] public static List<BaseView> openView = new List<BaseView>();
        [ShowInInspector, ReadOnly] public static List<BaseView> blockView = new List<BaseView>();
        [ShowInInspector, ReadOnly] public static List<QueueViewConfig> queueView = new List<QueueViewConfig>();

        #endregion

        #region propertices

        public bool IsActive
        {
            get { return gameObject.activeInHierarchy; }
        }

        public static bool IsViewInQueue
        {
            get { return queueView.Count > 0; }
        }

        public static List<Action<BaseView>> onViewHide = new List<Action<BaseView>>();

        public bool ManageQueue => manageQueue;
        #endregion

        #region virtual methods

        public virtual void Init()
        {
            canHide = IsActive;
            canShow = !IsActive;
        }

        public virtual void Awake()
        {
        }

        public virtual void OnDestroy()
        {
            RemoveFromBlockView();
        }

        public virtual void Show(Action action = null, bool isForceShow = false)
        {
            if (AddToQueue(action, isForceShow))
            {
                if (!canShow || isAnimationInProgress)
                    return;
                action?.Invoke();
                EventSystemHelper.Instance?.SetIntractable(false);
                openView.Add(this);
                AddToBackButtonPressableViews();
                if (blockRayCast)
                    blockView.Add(this);
                gameObject.SetActive(true);
                HideButtons();
                OnShowStart();
                if (viewAnimation != null)
                {
                    isAnimationInProgress = true;
                    viewAnimation.ShowAnimation(OnShowComplete);
                    return;
                }
                OnShowComplete();
            }
        }

        public virtual void Hide()
        {
            if (!canHide || isAnimationInProgress)
                return;
            openView.Remove(this);
            RemoveFromBackButtonPressableViews();
            EventSystemHelper.Instance?.SetIntractable(false);
            if (viewAnimation != null)
            {
                isAnimationInProgress = true;
                OnHideStart();
                viewAnimation.HideAnimation(OnHideComplete);
                return;
            }

            OnHideStart();
            OnHideComplete();
        }

        public virtual void OnBackgroundClick()
        {
            Hide();
        }

        public virtual void OnViewShowDone()
        {
        }

        public virtual void OnViewHideDone()
        {
        }

        public virtual void OnShowStart()
        {
        }

        public virtual void OnHideStart()
        {
        }

        public virtual void OnBackButtonPressed()
        {
            Hide();
        }

        public virtual bool CanPressBackButton()
        {
            return enableBackButton && canHide;
        }

        #endregion

        #region public static methods


        public static void ResetViews()
        {
            blockView = new List<BaseView>();
            openView = new List<BaseView>();
            queueView = new List<QueueViewConfig>();
            onViewHide = new List<Action<BaseView>>();
            backPressableViews = new List<BaseView>();
        }

        #endregion

        #region public methods

        public void ShowView()
        {
            if (!canShow || isAnimationInProgress)
                return;
            openView.Add(this);
            AddToBackButtonPressableViews();
            if (blockRayCast)
                blockView.Add(this);
            gameObject.SetActive(true);
            HideButtons();
            OnShowStart();
            if (viewAnimation != null)
            {
                isAnimationInProgress = true;
                viewAnimation.ShowAnimation(OnShowComplete);
                return;
            }
            OnShowComplete();
        }


        public bool HideCalled()
        {
            return !canHide || isAnimationInProgress;
        }

        public bool ShowCalled()
        {
            return !canShow || isAnimationInProgress;
        }
        #endregion

        #region private methods
        protected void AddToBackButtonPressableViews()
        {
            if (enableBackButton && !backPressableViews.Contains(this))
                backPressableViews.Add(this);
        }

        protected void RemoveFromBackButtonPressableViews()
        {
            if (backPressableViews.Contains(this))
                backPressableViews.Remove(this);
        }

        private void RemoveFromBlockView()
        {
            openView.RemoveAll(view => view == this);
            blockView.RemoveAll(view => view == this);
            queueView.RemoveAll(config => config.view == this);
            backPressableViews.RemoveAll(view => view == this);
        }

        private void ShowPendingView()
        {
            if (queueView.Count <= 0 || openView.Count > 0)
                return;
            if (queueView.Count > 0 && manageQueue)
                queueView.RemoveAt(0);
            if (queueView.Count > 0)
            {
                SetQueueView();
                queueView[0].action?.Invoke();
                queueView[0].view.ShowView();
            }
        }

        private void SetQueueView()
        {

        }

        private void OnShowComplete()
        {
            isAnimationInProgress = false;
            canHide = true;
            canShow = false;
            OnViewShowDone();
            EventSystemHelper.Instance?.SetIntractable(true);
        }

        public virtual void OnHideComplete()
        {
            canHide = false;
            canShow = true;
            isAnimationInProgress = false;
            if (blockRayCast)
                blockView.Remove(this);
            gameObject.SetActive(false);
            EventSystemHelper.Instance?.SetIntractable(true);
            OnViewHideDone();
            ShowPendingView();
            ShowButtons();
            OnViewHide(this);
        }

        public void OnForceHideOnly()
        {
            canHide = false;
            canShow = true;
            isAnimationInProgress = false;
            if (blockRayCast)
                blockView.Remove(this);
            gameObject.SetActive(false);
            RemoveFromBackButtonPressableViews();
            EventSystemHelper.Instance?.SetIntractable(true);
        }

        private bool AddToQueue(Action action, bool isForceShow)
        {
            if (!manageQueue || isForceShow)
                return true;
            queueView.Add(new QueueViewConfig()
            {
                view = this,
                action = action
            });
            return (queueView.Count <= 1 && openView.Count <= 0);
        }

        public static void ShowPendingViewOnInterActionStart()
        {
            if (queueView.Count <= 0 || openView.Count > 0)
                return;
            if (queueView.Count > 0)
            {
                queueView[0].action?.Invoke();
                queueView[0].view.ShowView();
            }
        }

        private void ShowButtons()
        {
            
        }
        private void HideButtons()
        {
        }

        #endregion

        #region public static methods

        public static void RegisterOnViewHide(Action<BaseView> action)
        {
            if (!onViewHide.Contains(action))
                onViewHide.Add(action);
        }

        public static void DeregisterOnViewHide(Action<BaseView> action)
        {
            if (onViewHide.Contains(action))
                onViewHide.Remove(action);
        }

        public static void OnViewHide(BaseView baseView)
        {
            for (int i = 0; i < onViewHide.Count; i++)
            {
                onViewHide[i].Invoke(baseView);
            }
        }

        #endregion

        public class QueueViewConfig
        {
            public BaseView view;
            public Action action;
        }
    }
}