using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

namespace com.tag.nut_sort
{

    public class SurpriseNutAnimation : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private float animationTime;
        [SerializeField] private string variableName;
        [SerializeField] private float startValue;
        [SerializeField] private float endValue;

        [Button]
        public void DoSurpriceNutRevealAnimation()
        {
            StartCoroutine(DoAnimation());
        }

        IEnumerator DoAnimation()
        {
            float i = 0;
            float rate = 1 / animationTime;

            MaterialPropertyBlock mp = new MaterialPropertyBlock();
            mp.SetFloat(variableName, startValue);

            while (i < 1)
            {
                i += Time.deltaTime * rate;
                mp.SetFloat(variableName, Mathf.Lerp(startValue, endValue, i));
                _renderer.SetPropertyBlock(mp);
                yield return null;
            }

            ObjectPool.Instance.Recycle(gameObject);
        }
    }
}