using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System.Collections.Generic;
using Tag.NutSort;
using UnityEditor;
using UnityEngine;

namespace com.tag.editor
{
    public class BaseIdAttributesDrawer<T> : OdinAttributeDrawer<T, int> where T : BaseIdAttribute
    {
        public List<string> values = new List<string>();
        public List<string> names = new List<string>();
        public Dictionary<string, int> mapValues = new Dictionary<string, int>();
        public Dictionary<int, string> map = new Dictionary<int, string>();
        public static BaseIDMappingConfig itemList;
        public string searchName = "";
        public string oldSearchName = " ";

        protected override void DrawPropertyLayout(GUIContent label)
        {
            EditorGUILayout.BeginHorizontal();
            FilterIDs();

            if (values.Count > 0 && map.ContainsKey(this.ValueEntry.SmartValue))
            {
                string item = SirenixEditorFields.Dropdown(label, map[this.ValueEntry.SmartValue], values.ToArray(), names.ToArray());
                this.ValueEntry.SmartValue = mapValues[item];
            }
            else
            {
                SirenixEditorGUI.ErrorMessageBox("Search Not Found : " + searchName);
                this.ValueEntry.SmartValue = SirenixEditorFields.IntField(label, this.ValueEntry.SmartValue);
            }

            searchName = SirenixEditorFields.TextField("", searchName, new GUILayoutOption[] { GUILayout.Width(150) });
            EditorGUILayout.EndHorizontal();
        }

        private void FilterIDs()
        {
            if (oldSearchName.Equals(searchName))
                return;
            oldSearchName = searchName;
            if (itemList != null && itemList.idMapping != null && itemList.idMapping.Count > 0)
            {
                map = itemList.idMapping;
                names.Clear();
                values.Clear();
                foreach (var m in itemList.idMapping)
                {
                    if (m.Value.ToLower().Contains(searchName.ToLower()))
                    {
                        names.Add(m.Key + "-" + m.Value);
                        values.Add(m.Value);
                    }

                    if (!mapValues.ContainsKey(m.Value))
                        mapValues.Add(m.Value, m.Key);
                }
            }
        }
    }
}