using UnityEditor;
using UnityEngine;

namespace Tag.NutSort.Editor
{
    [CustomPropertyDrawer(typeof(CustomVector3))]
    public class CustomVector3PropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            label = EditorGUI.BeginProperty(pos, label, prop);

            var contentRect = EditorGUI.PrefixLabel(pos, GUIUtility.GetControlID(FocusType.Passive), label);

            var labels = new[] { new GUIContent("X"), new GUIContent("Y"), new GUIContent("Z") };
            var properties = new[] { prop.FindPropertyRelative("x"), prop.FindPropertyRelative("y"), prop.FindPropertyRelative("z") };

            PropertyDrawerHelper.DrawMultiplePropertyFields(contentRect, labels, properties);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }
    }
}