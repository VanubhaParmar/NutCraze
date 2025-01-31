using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace com.tag.editor {
    public abstract class IdAttributesDrawer<T> : OdinAttributeDrawer<T, string> where T : Attribute
    {
        private List<string> idList;
        private string searchName = "";

        protected override void Initialize()
        {
            base.Initialize();
            idList = GetIdList();
        }
        protected abstract List<string> GetIdList();
        protected override void DrawPropertyLayout(GUIContent label)
        {
            EditorGUILayout.BeginHorizontal();

            if (idList == null)
                Initialize();

            if (idList.Count > 0)
                this.ValueEntry.SmartValue = SirenixEditorFields.Dropdown(label, this.ValueEntry.SmartValue, idList.ToArray(), idList.ToArray());
            else
            {
                SirenixEditorGUI.ErrorMessageBox("Search Not Found : " + searchName);
                this.ValueEntry.SmartValue = SirenixEditorFields.TextField(label, this.ValueEntry.SmartValue);
            }

            //searchName = SirenixEditorFields.TextField("", searchName, new GUILayoutOption[] { GUILayout.Width(150) });
            EditorGUILayout.EndHorizontal();
        }
    }
}
