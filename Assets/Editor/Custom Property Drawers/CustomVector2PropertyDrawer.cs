using UnityEditor;
using UnityEngine;

namespace Tag.NutSort.Editor
{
    [CustomPropertyDrawer(typeof(CustomVector2))]
    public class CustomVector2PropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            label = EditorGUI.BeginProperty(pos, label, prop);

            var contentRect = EditorGUI.PrefixLabel(pos, GUIUtility.GetControlID(FocusType.Passive), label);

            var labels = new[] { new GUIContent("X"), new GUIContent("Y") };
            var properties = new[] { prop.FindPropertyRelative("x"), prop.FindPropertyRelative("y") };

            PropertyDrawerHelper.DrawMultiplePropertyFields(contentRect, labels, properties);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }
    }
}