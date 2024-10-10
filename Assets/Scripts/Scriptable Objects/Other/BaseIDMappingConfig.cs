using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tag.NutSort
{
    [CreateAssetMenu(menuName = Constant.GAME_NAME + "/Editor/IdMappingConfig")]
    public class BaseIDMappingConfig : SerializedScriptableObject
    {
        public Dictionary<int, string> idMapping = new Dictionary<int, string>();

        public string GetNameFromId(int itemId)
        {
            if (idMapping.ContainsKey(itemId))
            {
                return idMapping[itemId];
            }

            return "";
        }

        public int GetIdFromName(string name)
        {
            foreach (var id in idMapping)
            {
                if (id.Value.Equals(name))
                    return id.Key;
            }

            return -1;
        }

#if UNITY_EDITOR
        public void SetData(List<Dictionary<string, object>> data)
        {
            idMapping.Clear();
            int itemId;
            string itemName;
            for (int i = 0; i < data.Count; i++)
            {
                itemId = int.Parse(data[i]["itemId"].ToString());
                itemName = data[i]["name"].ToString();
                if (!idMapping.ContainsKey(itemId))
                    idMapping.Add(itemId, itemName);
                else
                    idMapping[itemId] = itemName;
            }
        }

        public List<string> GetListOfString()
        {
            List<string> ids = new List<string>();
            foreach (var id in idMapping)
            {
                ids.Add(id.Value);
            }

            return ids;
        }
#endif
    }
}
