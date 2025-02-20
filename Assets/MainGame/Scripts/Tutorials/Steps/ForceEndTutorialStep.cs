using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Tag.NutSort {
    public class ForceEndTutorialStep : MonoBehaviour
    {
        [SerializeField] private GameObject _button;
        [SerializeField] private float _checkInterval = 1f;
        [SerializeField] private UnityEvent _onForceEndTutorial;

        private Coroutine _timeOutCoroutine;

        public void StartIntervalCo()
        {
            StopIntervalCo();
            _timeOutCoroutine = StartCoroutine(TimeOutCoroutine());
        }

        public void StopIntervalCo()
        {
            if (_timeOutCoroutine != null)
            {
                StopCoroutine(_timeOutCoroutine);
                _timeOutCoroutine = null;
            }
        }

        public void SetObject(Transform transform)
        {
            _button = transform.gameObject;
        }

        private IEnumerator TimeOutCoroutine()
        {
            WaitForSeconds wait = new WaitForSeconds(_checkInterval);
            while (_button.activeInHierarchy)
                yield return wait;
            _onForceEndTutorial?.Invoke();
        }
    }
}