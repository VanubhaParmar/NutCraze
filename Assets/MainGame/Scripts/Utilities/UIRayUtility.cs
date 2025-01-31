using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.tag.nut_sort {
    public static class UIRayUtility
    {
        public static bool IsPointerOverUIElement(string name)
        {
            return IsPointerOverUIElement(GetEventSystemRaycastResults(), name);
        }

        public static bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults, string name)
        {
            for (int i = 0; i < eventSystemRaysastResults.Count; i++)
            {
                RaycastResult curRaysastResult = eventSystemRaysastResults[i];
                if (curRaysastResult.gameObject.name == name)
                    return true;
            }
            return false;
        }

        public static List<RaycastResult> GetEventSystemRaycastResults()
        {
            PointerEventData eventData = new PointerEventData(EventSystemHelper.EventSystem);
            eventData.position = Input.mousePosition;
            List<RaycastResult> raysastResults = new List<RaycastResult>();
            EventSystemHelper.EventSystem.RaycastAll(eventData, raysastResults);
            return raysastResults;
        }
    }
}
