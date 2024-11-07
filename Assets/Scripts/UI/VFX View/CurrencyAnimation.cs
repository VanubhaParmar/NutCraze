using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Tag.NutSort;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;
using Random = UnityEngine.Random;

namespace Tag.TowerDefence
{
    public class CurrencyAnimation : MonoBehaviour
    {
        #region PUBLIC_VARIABLES

        [Header("References")] public AnimateObject objectToAnimate;
        [SerializeField] private RectTransform StartRect;
        //public Transform midPos;
        //public Transform midPos2;

        public Transform endPos;
        [SerializeField] private ParticleSystem feedBackParticles;
        //public TMP_Text textTopBarComponent;

        [Space(2)][Header("Values")] public float speed;
        public float interval;
        [Space(2)][Header("Curves")] public AnimationCurve moveCurve;
        public AnimationCurve scaleCurve;
        public Gradient colorOverLifeTime;
        [Space(2)][Header("Vectors")] public Vector3 startSize;
        public Vector3 endSize;

        [Space(2)]
        [Header("On Place Animation")]
        public bool useOnPlaceAnimation;

        public float speedOnPlace;
        public Vector3 startSizeOnPlace;
        public Vector3 endSizeOnPlace;
        public AnimationCurve scaleCurveOnPlace;

        public AnimationCurve easeOutCurve;

        public bool isLeftDirection = true;
        [Space(2)] public bool isInitOnEnable = true;

        //[SerializeField] private bool canAnimateText = false;
        [SerializeField] private bool canPlaySound = false;

        //[SerializeField] SoundType soundToPlayOnPlace;
        #endregion

        #region propertice

        public bool IsAnimationInProgress => animatedObjectList.Count > 0;

        #endregion

        #region PRIVATE_VARIABLES

        private Vector3 scale;
        private Vector3 p1;
        private Vector3 p2;
        private Vector3 p3;
        private Vector3 t1;
        private Vector3 t2;
        private Vector3 tempScale;
        private Vector3 viewportPoint;
        private Image imageItems;
        private int spawnedCurrencyObjectValue = 0;
        private float totalCurrency = 0;
        private float decrementScale;
        private bool isRandomStart = false;
        private List<Action<int, bool>> onObjectAnimationComeplete = new List<Action<int, bool>>();
        [ShowInInspector] private Dictionary<string, List<Action<int, bool>>> onObjectAnimationComepleted = new Dictionary<string, List<Action<int, bool>>>();
        private List<AnimateObject> animatedObjectList = new List<AnimateObject>();


        #endregion

        #region UNITY_CALLBACKS

        private void Awake()
        {
            Init();
            onObjectAnimationComeplete = new List<Action<int, bool>>();
            animatedObjectList = new List<AnimateObject>();
        }

        public void Start()
        {
            tempScale = startSize;
        }

        #endregion

        #region PUBLIC_METHODS


        public void StartAnimation(Vector3 startWorldPos, int amount, Sprite rewardSprite, bool isReverseAnimation = false)
        {
            CameraCache.TryFetchCamera(CameraCacheType.GLOBAL_UI_CAMERA, out Camera cam);
            viewportPoint = cam.WorldToViewportPoint(startWorldPos);
            imageItems.sprite = rewardSprite;
            StartRect.anchoredPosition = new Vector3(0, 0, 0);
            StartRect.anchorMax = viewportPoint;
            StartRect.anchorMin = viewportPoint;
            isRandomStart = false;
            Animate(amount, isReverseAnimatation: isReverseAnimation);
        }

        public void StartAnimation(Vector3 startWorldPos, int amount)
        {
            if (CameraCache.TryFetchCamera(CameraCacheType.GLOBAL_UI_CAMERA, out Camera cam))
                viewportPoint = cam.WorldToViewportPoint(startWorldPos);
            StartRect.anchoredPosition = new Vector3(0, 0, 0);
            StartRect.anchorMax = viewportPoint;
            StartRect.anchorMin = viewportPoint;
            isRandomStart = false;
            Animate(amount);
        }

        public void StartAnimation(Vector3 startWorldPos, int amount, Transform endPosition)
        {
            if (CameraCache.TryFetchCamera(CameraCacheType.GLOBAL_UI_CAMERA, out Camera cam))
                viewportPoint = cam.WorldToViewportPoint(startWorldPos);
            StartRect.anchoredPosition = new Vector3(0, 0, 0);
            StartRect.anchorMax = viewportPoint;
            StartRect.anchorMin = viewportPoint;
            endPos = endPosition;
            isRandomStart = false;
            Animate(amount);
        }

        public void StartAnimation(string key, Vector3 startWorldPos, int amount, Transform endPosition)
        {
            if (CameraCache.TryFetchCamera(CameraCacheType.GLOBAL_UI_CAMERA, out Camera cam))
                viewportPoint = cam.WorldToViewportPoint(startWorldPos);
            StartRect.anchoredPosition = new Vector3(0, 0, 0);
            StartRect.anchorMax = viewportPoint;
            StartRect.anchorMin = viewportPoint;
            endPos = endPosition;
            isRandomStart = false;
            Animate(amount, key: key);
        }

        public void UIStartAnimation(Vector3 anchorPosition, int objects = 2, bool isReverseAnimation = false, string layer = "UI", int sortingOrder = 0)
        {
            StartRect.position = anchorPosition;
            isRandomStart = true;
            Animate(objects, isReverseAnimation, layer, sortingOrder);
        }

        public void UIStartAnimation(Vector3 anchorPosition, int objects = 2, Sprite rewardSprite = null, bool isReverseAnimation = false, string layer = "UI", int sortingOrder = 0)
        {
            imageItems.sprite = rewardSprite;
            StartRect.position = anchorPosition;
            isRandomStart = true;
            Animate(objects, isReverseAnimation, layer, sortingOrder);
        }

        public void UIStartAnimation(Vector3 anchorPosition, Vector3 endPosition, int objects = 2, bool isReverseAnimation = false, string layer = "UI", int sortingOrder = 0)
        {
            StartRect.position = anchorPosition;
            isRandomStart = true;
            endPos.position = endPosition;
            Animate(objects, isReverseAnimation, layer, sortingOrder);
        }

        public void Animate(int tempAmount, bool isReverseAnimatation = false, string layer = "UI", int sortingOrder = 0, string key = "")
        {
            int amount = Mathf.Clamp(tempAmount, 3, 7);
            int[] valueArray = AssignCurrencyToSpawnedObjects(tempAmount, amount);
            Vector3[] temp = new Vector3[amount];
            Vector3[] offsetTemp = new Vector3[amount];
            AnimateObject[] animatedObjects = new AnimateObject[amount];

            for (int i = 0; i < amount; i++)
            {
                animatedObjects[i] = ObjectPool.Instance.Spawn(objectToAnimate, transform, StartRect.position);
                animatedObjects[i].SetSortingOrder(layer, sortingOrder);
                animatedObjects[i].gameObject.SetActive(false);
                animatedObjects[i].SetDetails(valueArray[i]);
                offsetTemp[i] = isLeftDirection ? (StartRect.position + new Vector3(Random.Range(-0, -0.5f), Random.Range(-0, 0.5f), 0f)) : (StartRect.position + new Vector3(Random.Range(0.5f, -1f), -Random.Range(0.0f, 1f), 0));
            }
            if (!isReverseAnimatation)
                StartCoroutine(AnimateThroughLoop(animatedObjects, amount, temp, offsetTemp, isReverseAnimatation, key));
            else
                StartCoroutine(AnimateThroughLoopReverse(animatedObjects, amount, temp, StartRect.position, isReverseAnimatation, key));
        }

        public void SetScaleWithCamera()
        {
            decrementScale = ((Camera.main.orthographicSize - 2.7f) * 0.4f) / 2.6f;
            startSize.x = tempScale.x - decrementScale;
            startSize.y = tempScale.y - decrementScale;
            startSize.z = tempScale.z - decrementScale;
        }

        public int[] AssignCurrencyToSpawnedObjects(int value, int count)
        {
            int[] temp = new int[count];
            int counter = 0;
            spawnedCurrencyObjectValue = value / count;
            for (int i = 0; i < temp.Length - 1; i++)
            {
                temp[i] = spawnedCurrencyObjectValue;
                counter += spawnedCurrencyObjectValue;
            }

            temp[temp.Length - 1] = value - counter;
            return temp;
        }


        [ContextMenu("Mid 2 As Self")]
        public void AssignMidTwoAsSelf()
        {
            //midPos2 = midPos;
        }

        #endregion

        #region PRIVATE_METHODS
        public void RegisterObjectAnimationComplete(Action<int, bool> action)
        {
            if (!onObjectAnimationComeplete.Contains(action))
                onObjectAnimationComeplete.Add(action);
        }

        public void DeregisterObjectAnimationComplete(Action<int, bool> action)
        {
            if (onObjectAnimationComeplete.Contains(action))
                onObjectAnimationComeplete.Remove(action);
        }

        private void OnObjectAnimationComplete(int size, bool isLastObject)
        {
            for (int i = 0; i < onObjectAnimationComeplete.Count; i++)
            {
                onObjectAnimationComeplete[i]?.Invoke(size, isLastObject);
            }
        }
        public void RegisterObjectAnimationComplete(string key, Action<int, bool> action)
        {
            if (!onObjectAnimationComepleted.ContainsKey(key))
            {
                onObjectAnimationComepleted.Add(key, new List<Action<int, bool>>());
                onObjectAnimationComepleted[key].Add(action);
            }
            else
            {
                if (!onObjectAnimationComepleted[key].Contains(action))
                {
                    onObjectAnimationComepleted[key].Add(action);
                }
            }
        }

        public void DeregisterObjectAnimationComplete(string key, Action<int, bool> action)
        {
            if (onObjectAnimationComepleted.ContainsKey(key) && onObjectAnimationComepleted[key].Contains(action))
                onObjectAnimationComepleted[key].Remove(action);
        }

        private void OnObjectAnimationComepleted(string key, int size, bool isLastObject)
        {
            if (onObjectAnimationComepleted.ContainsKey(key))
            {
                List<Action<int, bool>> tempList = onObjectAnimationComepleted[key];
                for (int i = 0; i < tempList.Count; i++)
                {
                    tempList[i]?.Invoke(size, isLastObject);
                }
            }
        }

        private void Init()
        {
            //if (midPos2 == null)
            //{
            //    midPos2 = midPos;
            //}
            imageItems = objectToAnimate.Image;
        }

        //private void PlayPlaceSoundClip()
        //{
        //    if (soundToPlayOnPlace != SoundType.None)
        //        SoundHandler.Instance.PlaySound(soundToPlayOnPlace);
        //}
        #endregion

        #region EVENT_HANDLERS

        #endregion

        #region CO-ROUTINES

        public IEnumerator AnimateThroughLoop(AnimateObject[] animateObjects, int amount, Vector3[] temp, Vector3[] offset, bool isReverseAnimatation = false, string key = "")
        {
            animatedObjectList.AddRange(animateObjects);
            for (int i = 0; i < amount; i++)
            {
                temp[i] = new Vector3(Random.Range(0, 0), Random.Range(0, 0), 0);
                animateObjects[i].transform.position = StartRect.position;
                animateObjects[i].gameObject.SetActive(true);
                StartCoroutine(PunchPositionLerp(animateObjects[i], i, (i == amount - 1), offset, StartRect.position, isReverseAnimatation, key));

                yield return new WaitForSeconds(0.08f);
            }
            //yield return new WaitForSeconds(0.15f);
            StartCoroutine(AnimateTemp(animateObjects, amount, temp, offset, isReverseAnimatation, key));
        }

        public IEnumerator AnimateTemp(AnimateObject[] animateObjects, int amount, Vector3[] temp, Vector3[] offset, bool isReverseAnimatation = false, string key = "")
        {
            animatedObjectList.AddRange(animateObjects);
            for (int i = 0; i < amount; i++)
            {
                StartCoroutine(AnimateLerp(animateObjects[i], i == 0, (i == amount - 1), temp, offset[i], isReverseAnimatation, key));
                yield return new WaitForSeconds(interval);
            }
        }

        public IEnumerator AnimateThroughLoopReverse(AnimateObject[] animateObjects, int amount, Vector3[] temp, Vector3 startPosition, bool isReverseAnimatation = false, string key = "")
        {
            animatedObjectList.AddRange(animateObjects);
            for (int i = 0; i < amount; i++)
            {
                temp[i] = new Vector3(Random.Range(0, 0), Random.Range(0, 0), 0);
                StartCoroutine(AnimateLerp(animateObjects[i], i == 0, (i == amount - 1), temp, startPosition, isReverseAnimatation, key));

                yield return new WaitForSeconds(interval);
            }
        }

        private IEnumerator PunchPositionLerp(AnimateObject animateObject, int count, bool isLastObject, Vector3[] offset, Vector3 startTransformPos, bool isReverseAnimation = false, string key = "")
        {
            AnimationCurve easeOutCurve = this.easeOutCurve;
            float i = 0;

            while (i < 1)
            {
                i += (Time.deltaTime * (1 / 0.35f));
                animateObject.transform.position = Vector3.LerpUnclamped(startTransformPos, offset[count], easeOutCurve.Evaluate(isReverseAnimation ? (1 - i) : i));
                //animateObject.transform.localScale = Vector3.LerpUnclamped(startSize, endSize * 0.6f, easeOutCurve.Evaluate(isReverseAnimation ? (1 - i) : i));
                yield return 0;
            }
        }

        private IEnumerator AnimateLerp(AnimateObject animateObject, bool isFirstObject, bool isLastObject, Vector3[] temp, Vector3 offset, bool isReverseAnimatation = false, string key = "")
        {
            int tempIndex = Random.Range(0, temp.Length);
            float i = 0;
            float rate = (Vector3.Distance(offset, endPos.position) /*+ Vector3.Distance(midPos.position, midPos2.position) + Vector3.Distance(midPos2.position, endPos.position)*/) / (speed * 1);

            // offset = isLeftDirection ? (startTransformPos + new Vector3(-1f, -1f, 0f)) : (startTransformPos + new Vector3(0.5f, -0.5f, 0));
            Vector3 startScale = animateObject.transform.localScale;
            i = 0;
            while (i < 1)
            {
                i += (Time.deltaTime / rate);
                float lerp = isReverseAnimatation ? (1 - i) : i;
                animateObject.transform.position = Vector3.LerpUnclamped(offset, endPos.position, moveCurve.Evaluate(lerp));
                //p1 = Vector3.LerpUnclamped(offset, midPos.position + temp[tempIndex], moveCurve.Evaluate(lerp));
                //p2 = Vector3.LerpUnclamped(midPos.position + temp[tempIndex], midPos2.position + temp[tempIndex], moveCurve.Evaluate(lerp));
                //p3 = Vector3.LerpUnclamped(midPos2.position + temp[tempIndex], endPos.position, moveCurve.Evaluate(lerp));

                //t1 = Vector3.LerpUnclamped(p1, p2, moveCurve.Evaluate(lerp));
                //t2 = Vector3.LerpUnclamped(p2, p3, moveCurve.Evaluate(lerp));

                //animateObject.transform.position = Vector3.LerpUnclamped(t1, t2, moveCurve.Evaluate(lerp));
                animateObject.transform.localScale = Vector3.LerpUnclamped(startScale, endSize, scaleCurve.Evaluate(lerp));

                yield return 0;
            }

            if (useOnPlaceAnimation && isLastObject)
                StartCoroutine(AnimateOnPlaceScale());
            animatedObjectList.Remove(animateObject);
            if (key != "")
                OnObjectAnimationComepleted(key, animateObject.CurrencyItemPoints, isLastObject);
            OnObjectAnimationComplete(animateObject.CurrencyItemPoints, isLastObject);
            ObjectPool.Instance.Recycle(animateObject);

            //PlayPlaceSoundClip();
        }

        private IEnumerator AnimateOnPlaceScale()
        {
            if (feedBackParticles != null)
            {
                feedBackParticles.Stop();
                feedBackParticles.Play();
            }

            float i = 0;
            float rate = 1 / speedOnPlace;
            while (i < 1)
            {
                i += rate * Time.deltaTime;
                endPos.transform.localScale = Vector3.LerpUnclamped(startSizeOnPlace, endSizeOnPlace, scaleCurveOnPlace.Evaluate(i));
                yield return 0;
            }

            i = 1;
            endPos.transform.localScale = Vector3.LerpUnclamped(startSizeOnPlace, endSizeOnPlace, scaleCurveOnPlace.Evaluate(i));
        }

        #endregion
    }
}