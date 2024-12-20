using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Sirenix.OdinInspector;

namespace Tag.NutSort
{
    public class ReusableVerticalScrollView : SerializedMonoBehaviour
    {
        #region PUBLIC_VARIABLES
        public float ExtraBufferItems = 1f; // Buffer items above and below visible area
        [Tooltip("Horizontal offset from the left edge")]
        public float LeftOffset = 0f;
        [Tooltip("Vertical spacing between items")]
        public float ItemSpacing = 0f;
        [Tooltip("Additional padding at the top of the scroll view")]
        public float TopOffset = 0f;
        [Tooltip("Additional padding at the bottom of the scroll view")]
        public float BottomOffset = 0f;
        #endregion

        #region PRIVATE_VARIABLES
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform contentPanel => scrollRect.content;
        [SerializeField] private RectTransform itemPrefab;

        private float itemHeight;
        private List<RectTransform> pooledItems = new List<RectTransform>();
        private int totalDataCount;
        private float lastScrollPos;
        private int startIndex;
        [ShowInInspector, ReadOnly] private Dictionary<int, RectTransform> visibleItems = new Dictionary<int, RectTransform>();
        private Action<GameObject, int> onPopulateItem;
        #endregion

        #region UNITY_CALLBACKS
        private void Awake()
        {
            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        }
        #endregion

        #region PUBLIC_METHODS
        public void Initialize(int itemCount, Action<GameObject, int> populateCallback)
        {
            totalDataCount = itemCount;
            onPopulateItem = populateCallback;

            // Get height from prefab
            itemHeight = itemPrefab.rect.height + ItemSpacing;

            // Set content height based on total items plus offsets
            float totalHeight = (itemHeight * totalDataCount) + TopOffset + BottomOffset - ItemSpacing; // Subtract one spacing since we don't need space after the last item
            contentPanel.sizeDelta = new Vector2(contentPanel.sizeDelta.x, totalHeight);

            // Calculate how many items we need based on viewport height
            int visibleCount = Mathf.CeilToInt(scrollRect.viewport.rect.height / itemHeight);
            int totalNeededItems = visibleCount + (Mathf.CeilToInt(ExtraBufferItems) * 2);

            // Create pool of items
            CreatePool(totalNeededItems);

            // Initial population
            RefreshVisibleItems();
        }

        /// <summary>
        /// Gets the anchored position of an item at the specified index relative to the content panel
        /// </summary>
        /// <param name="index">Index of the item</param>
        /// <returns>Vector2 representing the anchored position</returns>
        public Vector2 GetItemPosition(int index)
        {
            if (index < 0 || index >= totalDataCount)
                return Vector2.zero;
            
            return new Vector2(LeftOffset, -index * itemHeight - TopOffset);
        }

        public Vector3 GetItemWorldPosition(int index)
        {
            // Get the anchored position first
            Vector2 anchoredPos = GetItemPosition(index);

            // Convert to world position using the content panel's RectTransform
            Vector3 localPos = new Vector3(anchoredPos.x, anchoredPos.y, 0f);
            return contentPanel.TransformPoint(localPos);
        }

        public RectTransform GetItemIfVisible(int index)
        {
            if (visibleItems.ContainsKey(index))
                return visibleItems[index];

            return null;
        }

        public void RefreshVisibility()
        {
            RefreshVisibleItems();
        }
        #endregion

        #region PRIVATE_METHODS
        private void CreatePool(int count)
        {
            // Clear existing pool
            foreach (var item in pooledItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            pooledItems.Clear();
            visibleItems.Clear();

            // Create new pool
            for (int i = 0; i < count; i++)
            {
                GameObject item = Instantiate(itemPrefab.gameObject, contentPanel);
                RectTransform rect = item.GetComponent<RectTransform>();
                rect.gameObject.SetActive(false);
                pooledItems.Add(rect);
            }
        }

        private void OnScrollValueChanged(Vector2 normalizedPos)
        {
            RefreshVisibleItems();
        }

        private void RefreshVisibleItems()
        {
            float scrollPos = contentPanel.anchoredPosition.y;
            
            // Calculate visible range
            int newStartIndex = Mathf.FloorToInt((scrollPos - TopOffset) / itemHeight);
            newStartIndex = Mathf.Max(0, newStartIndex - Mathf.CeilToInt(ExtraBufferItems));
            
            int visibleCount = Mathf.CeilToInt(scrollRect.viewport.rect.height / itemHeight);
            int endIndex = Mathf.Min(totalDataCount, newStartIndex + visibleCount + Mathf.CeilToInt(ExtraBufferItems * 2));

            // Remove items that are no longer visible
            List<int> itemsToRemove = new List<int>();
            foreach (var kvp in visibleItems)
            {
                if (kvp.Key < newStartIndex || kvp.Key >= endIndex)
                {
                    kvp.Value.gameObject.SetActive(false);
                    itemsToRemove.Add(kvp.Key);
                }
            }
            foreach (int key in itemsToRemove)
            {
                visibleItems.Remove(key);
            }

            // Add new visible items
            for (int i = newStartIndex; i < endIndex; i++)
            {
                if (!visibleItems.ContainsKey(i) && i < totalDataCount)
                {
                    RectTransform item = GetPooledItem();
                    if (item != null)
                    {
                        item.gameObject.SetActive(true);
                        item.anchoredPosition = new Vector2(LeftOffset, -i * itemHeight - TopOffset);
                        visibleItems[i] = item;
                        onPopulateItem?.Invoke(item.gameObject, i);
                    }
                }
            }

            startIndex = newStartIndex;
        }

        private RectTransform GetPooledItem()
        {
            return pooledItems.Find(x => !x.gameObject.activeInHierarchy);
        }
        #endregion
    }
}