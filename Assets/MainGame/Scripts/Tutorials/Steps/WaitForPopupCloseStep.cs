using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Tag.NutSort {
    public class WaitForPopupCloseStep : BaseTutorialStep
    {
        [SerializeField] private UnityEvent _addPopUps;
        private List<GameObject> _popUps = new();

        public override void OnStartStep1()
        {
            base.OnStartStep1();
            _addPopUps.Invoke();
            StartCoroutine(WaitForPopupClose());
        }

        private IEnumerator WaitForPopupClose()
        {
            WaitForSeconds wait = new(0.1f);
            while (_popUps.Find(x => x.activeInHierarchy) != null)
            {
                TutorialElementHandler.Instance.SetActivateBackGround(false, 0);
                yield return wait;
            }
            TutorialElementHandler.Instance.SetActivateBackGround(true, 0);
            EndStep();
        }

        #region ADD_POPUP_CALLBACKS
        #endregion
    }
}