using System;
using System.Collections.Generic;
using System.Globalization;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Tag.NutSort
{
    public static class Utility
    {
        public static List<string> GetRows(string json)
        {
            json = json.Replace("[", "").Replace("]", "");
            List<string> rowsList = new List<string>();
            string[] rows = json.Split("},{");
            for (int i = 0; i < rows.Length; i++)
            {
                string newRow = rows[i];
                if (i == 0)
                {
                    newRow = newRow + "}";
                }
                else if (i == rows.Length - 1)
                {
                    newRow = "{" + newRow;
                }
                else
                {
                    newRow = "{" + newRow + "}";
                }
                rowsList.Add(newRow);
            }
            return rowsList;
        }

        public static List<T> GetSubList<T>(this List<T> listOfT, int startIndex, int endIndex)
        {
            List<T> subList = new List<T>();
            for (int i = startIndex; i < endIndex; i++)
                subList.Add(listOfT[i]);
            return subList;
        }

        public static DateTime GetUpcomingTime(int hour, int minute)
        {
            DateTime now = CustomTime.GetCurrentTime();
            DateTime upcomingTime = new(now.Year, now.Month, now.Day, hour, minute, 0);
            if (now.TimeOfDay.TotalSeconds > new TimeSpan(hour, minute, 0).TotalSeconds)
                upcomingTime = upcomingTime.AddDays(1);
            return upcomingTime;
        }

        public static T GetRandomItemFromList<T>(this List<T> listOfT)
        {
            if (listOfT.Count > 0)
                return listOfT[UnityEngine.Random.Range(0, listOfT.Count)];
            return default(T);
        }

        public static T GetLastItemFromList<T>(this List<T> listOfT)
        {
            if (listOfT.Count > 0)
                return listOfT[listOfT.Count - 1];
            return default(T);
        }
        public static T PopAt<T>(this List<T> list, int index)
        {
            if (list == null || list.Count == 0 || index < 0 || index >= list.Count)
            {
                return default(T); // Return default value if list is null, empty, or index is out of range
            }

            T item = list[index];
            list.RemoveAt(index); // Remove the item at the specified index
            return item;  // Return the removed item
        }

        public static T Pop<T>(this List<T> list, Predicate<T> match)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            if (match == null) return default(T);
            if (list == null || list.Count == 0)
            {
                return default(T); // Return default value if list is null, empty, or index is out of range
            }

            // Find the first item that matches the predicate
            T item = list.Find(match);

            // If an item is found, remove it and return true
            if (item != null)
                list.Remove(item);

            return item;
        }

        public static bool Remove<T>(this List<T> list, Predicate<T> match)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            if (match == null) return false;

            // Find the first item that matches the predicate
            T item = list.Find(match);

            // If an item is found, remove it and return true
            if (item != null)
                return list.Remove(item);

            // Return false if no item matches the predicate
            return false;
        }
        public static string PrintList<T>(this List<T> listOfT)
        {
            string debugList = "List Log : \n";
            foreach(var data in listOfT)
            {
                debugList += data.ToString() + "\n";
            }
            return debugList;
        }

        public static int CalculateCost(int totalCost, int totalTime, int remainingTime)
        {
            if (remainingTime > totalTime)
                remainingTime = totalTime;
            int cost = Mathf.CeilToInt(remainingTime * totalCost / (float)totalTime);
            return Mathf.Clamp(cost, 1, totalCost);
        }

        public static string Remove(this string inputString, string stringToRemove)
        {
            return inputString.Replace(stringToRemove, "");
        }

        public static string ParseDateString(this DateTime dateTimeToCovert)
        {
            return dateTimeToCovert.ToString("dd-MM-yyyy hh:mm tt", CultureInfo.InvariantCulture);
        }

        public static DeviceType GetDeviceType()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return DeviceType.Android_Device;
#elif UNITY_IOS && !UNITY_EDITOR
            return DeviceType.IOS_Device;
#else
            return DeviceType.Unity_Editor;
#endif
        }

        public static string ParseTimeSpan(this TimeSpan timeSpanToCovert, string format)
        {
            return string.Format(format, timeSpanToCovert);
        }

        public static string ParseTimeSpan(this TimeSpan timeSpanToCovert, int maxParams = 3)
        {
            string currentFormat = "{0:hh}h {0:mm}m {0:ss}s";
            if (maxParams == 2)
            {
                if (timeSpanToCovert.TotalDays >= 1)
                    currentFormat = "{0:%d}d {0:hh}h";
                else if (timeSpanToCovert.Hours >= 1)
                    currentFormat = "{0:hh}h {0:mm}m";
                else
                    currentFormat = "{0:mm}m {0:ss}s";
            }
            else if (maxParams == 3 && timeSpanToCovert.TotalDays >= 1)
            {
                currentFormat = "{0:%d}d {0:hh}h {0:mm}m";
            }

            return string.Format(currentFormat, timeSpanToCovert);
        }

        public static bool IsInsideRange(this Vector2Int range, int value)
        {
            return value >= range.x && value <= range.y;
        }

        public static string GetDebugString(this Vector3 vector)
        {
            return $"({vector.x.ToString("0.00")}, {vector.y.ToString("0.00")}, {vector.z.ToString("0.00")})";
        }

        public static string ToCultureInvariantString(this DateTime dateTimeToSave)
        {
            return dateTimeToSave.ToString(CultureInfo.InvariantCulture);
        }

        public static DateTime AddTimeDuration(this DateTime dateTime, TimeDuration timeDuration)
        {
            return dateTime.AddHours(timeDuration.hours).AddMinutes(timeDuration.minutes).AddSeconds(timeDuration.seconds);
        }

        public static void ScrollToRect(this ScrollRect scrollRect, RectTransform viewTransform, bool playAnim = false)
        {
            int direction = scrollRect.horizontal ? -1 : 1;
            int widthMultiPlier = scrollRect.horizontal ? 1 : 0;
            int heightMultiPlier = scrollRect.vertical ? 1 : 0;

            RectTransform contentRect = scrollRect.content;
            RectTransform viewPortRect = scrollRect.viewport;

            // Calculate the position of the view transform relative to the content
            Vector2 viewPosition = contentRect.InverseTransformPoint(viewTransform.position);

            // Calculate the center of the viewport
            Vector2 viewportCenter = new Vector2(viewPortRect.rect.width * 0.5f * widthMultiPlier, viewPortRect.rect.height * 0.5f * heightMultiPlier);

            // Calculate the target position of the content
            Vector2 targetPosition = -(viewPosition + viewportCenter * direction);

            // Clamp the target position to ensure the content doesn't scroll beyond its bounds
            float maxPos = (contentRect.rect.width - viewPortRect.rect.width) * widthMultiPlier + (contentRect.rect.height - viewPortRect.rect.height) * heightMultiPlier;
            float minVal = direction < 0f ? -maxPos : 0f;
            float maxVal = direction < 0f ? 0f : maxPos;

            targetPosition.x = Mathf.Clamp(targetPosition.x, minVal, maxVal) * widthMultiPlier;
            targetPosition.y = Mathf.Clamp(targetPosition.y, minVal, maxVal) * heightMultiPlier;

            // Animate the content to the target position
            if (playAnim)
                contentRect.DOAnchorPos(targetPosition, 0.3f).SetEase(Ease.OutQuad);
            else
                contentRect.anchoredPosition = targetPosition;
        }

        public static T LoadResourceAsset<T>(string path) where T : UnityEngine.Object
        {
            return Resources.Load<T>(path);
        }

        public static void LoadResourceAssetAsync<T>(string path, Action<T> onLoaded) where T : UnityEngine.Object
        {
            // Start the async loading
            ResourceRequest request = Resources.LoadAsync<T>(path);

            // Assign a callback to handle when the asset is loaded
            request.completed += (asyncOperation) =>
            {
                // Check if the asset was successfully loaded
                if (request.asset != null)
                    onLoaded?.Invoke(request.asset as T);
                else
                    Debug.LogError("Failed to load asset at: " + path);
            };
        }

        #region ANIMTION_HELPERS
        public static float GetAnimatorClipLength(this Animator animator, string animationName)
        {
            RuntimeAnimatorController cont = animator.runtimeAnimatorController;
            for (int i = 0; i < cont.animationClips.Length; i++)
            {
                if (cont.animationClips[i].name == animationName)
                    return cont.animationClips[i].length;
            }

            return 0;
        }
        #endregion

        #region DOTWEEN_ANIMATION_HELPERS
        /// <summary>
        /// Moves transform with overshoot in global cordinates.
        /// </summary>
        /// <param name="moveTransform"></param>
        /// <param name="targetPosition"></param>
        /// <param name="animationTime"></param>
        /// <param name="overshootValue">Should be between 0 and 1.</param>
        /// <returns></returns>
        public static Sequence DoMoveWithOvershoot(this Transform moveTransform, Vector3 targetPosition, float animationTime, float overshootValue, float overshootTime = -1f)
        {
            Vector3 startPos = moveTransform.position;
            Vector3 overshootPos = Vector3.LerpUnclamped(startPos, targetPosition, 1f + overshootValue);

            if (overshootTime < 0f)
                overshootTime = overshootValue * animationTime;

            Sequence moveSeq = DOTween.Sequence();
            moveSeq.Append(moveTransform.DOMove(overshootPos, animationTime - overshootTime));
            moveSeq.Append(moveTransform.DOMove(targetPosition, overshootTime));
            return moveSeq;
        }

        /// <summary>
        /// Moves transform with overshoot in global cordinates.
        /// </summary>
        /// <param name="moveTransform"></param>
        /// <param name="targetPosition"></param>
        /// <param name="animationTime"></param>
        /// <param name="overshootValue">Should be between 0 and 1.</param>
        /// <returns></returns>
        public static Sequence DoMoveWithReverseOvershoot(this Transform moveTransform, Vector3 targetPosition, float animationTime, float overshootValue, float overshootTime = -1f)
        {
            Vector3 startPos = moveTransform.position;
            Vector3 overshootOnePos = Vector3.LerpUnclamped(startPos, targetPosition, -overshootValue);

            if (overshootTime < 0f)
                overshootTime = overshootValue * animationTime;

            Sequence moveSeq = DOTween.Sequence();
            moveSeq.Append(moveTransform.DOMove(overshootOnePos, overshootTime));
            moveSeq.Append(moveTransform.DOMove(targetPosition, animationTime - overshootTime));
            return moveSeq;
        }

        /// <summary>
        /// Moves transform with overshoot in global cordinates.
        /// </summary>
        /// <param name="moveTransform"></param>
        /// <param name="targetPosition"></param>
        /// <param name="animationTime"></param>
        /// <param name="overshootValue">Should be between 0 and 1.</param>
        /// <returns></returns>
        public static Sequence DoMoveWithBothOvershoot(this Transform moveTransform, Vector3 targetPosition, float animationTime, float overshootValue, float overshootTime = -1f)
        {
            Vector3 startPos = moveTransform.position;
            Vector3 overshootTwoPos = Vector3.LerpUnclamped(startPos, targetPosition, 1f + overshootValue);
            Vector3 overshootOnePos = Vector3.LerpUnclamped(startPos, targetPosition, -overshootValue);

            if (overshootTime < 0f)
                overshootTime = overshootValue * animationTime;

            Sequence moveSeq = DOTween.Sequence();
            moveSeq.Append(moveTransform.DOMove(overshootOnePos, overshootTime));
            moveSeq.Append(moveTransform.DOMove(overshootTwoPos, animationTime - (overshootTime * 2f)));
            moveSeq.Append(moveTransform.DOMove(targetPosition, overshootTime));
            return moveSeq;
        }

        /// <summary>
        /// Moves transform with overshoot in global cordinates.
        /// </summary>
        /// <param name="moveTransform"></param>
        /// <param name="targetPosition"></param>
        /// <param name="animationTime"></param>
        /// <param name="overshootValue">Should be between 0 and 1.</param>
        /// <returns></returns>
        public static Sequence DoAnchorPosWithOvershoot(this RectTransform moveTransform, Vector3 targetPosition, float animationTime, float overshootValue, float overshootTime = -1f)
        {
            Vector3 startPos = moveTransform.anchoredPosition;
            Vector3 overshootPos = Vector3.LerpUnclamped(startPos, targetPosition, 1f + overshootValue);

            if (overshootTime < 0f)
                overshootTime = overshootValue * animationTime;

            Sequence moveSeq = DOTween.Sequence();
            moveSeq.Append(moveTransform.DOAnchorPos(overshootPos, animationTime - overshootTime));
            moveSeq.Append(moveTransform.DOAnchorPos(targetPosition, overshootTime));
            return moveSeq;
        }

        /// <summary>
        /// Moves transform with overshoot in global cordinates.
        /// </summary>
        /// <param name="moveTransform"></param>
        /// <param name="targetPosition"></param>
        /// <param name="animationTime"></param>
        /// <param name="overshootValue">Should be between 0 and 1.</param>
        /// <returns></returns>
        public static Sequence DoAnchorPosWithReverseOvershoot(this RectTransform moveTransform, Vector3 targetPosition, float animationTime, float overshootValue, float overshootTime = -1f)
        {
            Vector3 startPos = moveTransform.anchoredPosition;
            Vector3 overshootPos = Vector3.LerpUnclamped(startPos, targetPosition, -overshootValue);

            if (overshootTime < 0f)
                overshootTime = overshootValue * animationTime;

            Sequence moveSeq = DOTween.Sequence();
            moveSeq.Append(moveTransform.DOAnchorPos(overshootPos, overshootTime));
            moveSeq.Append(moveTransform.DOAnchorPos(targetPosition, animationTime - overshootTime));
            return moveSeq;
        }

        /// <summary>
        /// Moves transform with overshoot in local cordinates.
        /// </summary>
        /// <param name="moveTransform"></param>
        /// <param name="targetPosition"></param>
        /// <param name="animationTime"></param>
        /// <param name="overshootValue">Should be between 0 and 1.</param>
        /// <returns></returns>
        public static Sequence DoLocalMoveWithOvershoot(this Transform moveTransform, Vector3 targetPosition, float animationTime, float overshootValue, float overshootTime = -1f)
        {
            Vector3 startPos = moveTransform.localPosition;
            Vector3 overshootPos = Vector3.LerpUnclamped(startPos, targetPosition, 1f + overshootValue);

            if (overshootTime < 0f)
                overshootTime = overshootValue * animationTime;

            Sequence moveSeq = DOTween.Sequence();
            moveSeq.Append(moveTransform.DOLocalMove(overshootPos, animationTime - overshootTime));
            moveSeq.Append(moveTransform.DOLocalMove(targetPosition, overshootTime));
            return moveSeq;
        }

        /// <summary>
        /// Scales transform with overshoot.
        /// </summary>
        /// <param name="scaleTransform"></param>
        /// <param name="targetScale"></param>
        /// <param name="animationTime"></param>
        /// <param name="overshootValue">Should be between 0 and 1.</param>
        /// <returns></returns>
        public static Sequence DoScaleWithOvershoot(this Transform scaleTransform, Vector3 targetScale, float animationTime, float overshootValue, float overshootTime = -1f)
        {
            Vector3 startScale = scaleTransform.localScale;
            Vector3 overshootScale = Vector3.LerpUnclamped(startScale, targetScale, 1f + overshootValue);

            if (overshootTime < 0f)
                overshootTime = overshootValue * animationTime;

            Sequence moveSeq = DOTween.Sequence();
            moveSeq.Append(scaleTransform.DOScale(overshootScale, animationTime - overshootTime));
            moveSeq.Append(scaleTransform.DOScale(targetScale, overshootTime));
            return moveSeq;
        }

        /// <summary>
        /// Scales transform with reverse overshoot.
        /// </summary>
        /// <param name="scaleTransform"></param>
        /// <param name="targetScale"></param>
        /// <param name="animationTime"></param>
        /// <param name="overshootValue">Should be between 0 and 1.</param>
        /// <returns></returns>
        public static Sequence DoScaleWithReverseOvershoot(this Transform scaleTransform, Vector3 targetScale, float animationTime, float overshootValue, float overshootTime = -1f)
        {
            Vector3 startScale = scaleTransform.localScale;
            Vector3 overshootScale = Vector3.LerpUnclamped(startScale, targetScale, -overshootValue);

            if (overshootTime < 0f)
                overshootTime = overshootValue * animationTime;

            Sequence moveSeq = DOTween.Sequence();
            moveSeq.Append(scaleTransform.DOScale(overshootScale, overshootTime));
            moveSeq.Append(scaleTransform.DOScale(targetScale, animationTime - overshootTime));
            return moveSeq;
        }
        #endregion
    }

    public static class IListExtensions
    {
        /// <summary>
        /// Shuffles the element order of the specified list.
        /// </summary>
        public static void Shuffle<T>(this IList<T> ts)
        {
            var count = ts.Count;
            var last = count - 1;
            for (var i = 0; i < last; ++i)
            {
                var r = UnityEngine.Random.Range(i, count);
                var tmp = ts[i];
                ts[i] = ts[r];
                ts[r] = tmp;
            }
        }

        /// <summary>
        /// Deepcopy an Object list
        /// </summary>
        public static List<T> DeepCopy<T>(this List<T> ts) where T : class, IDeepCopyable
        {
            List<T> newCopy = new List<T>();
            for (var i = 0; i < ts.Count; i++)
            {
                newCopy.Add(ts[i].DeepCopy() as T);
            }
            return newCopy;
        }

        public interface IDeepCopyable
        {
            public object DeepCopy();
        }
    }

    public enum DeviceType
    {
        Unity_Editor,
        Android_Device,
        IOS_Device
    }
}