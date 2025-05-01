using UnityEditor;
using UnityEngine;

namespace Tag.NutSort.Editor
{
    [CustomPropertyDrawer(typeof(TimeDuration))]
    public class TimeDurationPropertyDrawer : PropertyDrawer
    {
        #region PUBLIC_VARIABLES
        #endregion

        #region PRIVATE_VARIABLES
        private const float BottomSpacing = 2;
        #endregion

        #region PROPERTIES
        #endregion

        #region UNITY_CALLBACKS
        #endregion

        #region PUBLIC_METHODS
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            pos.height -= BottomSpacing;
            label = EditorGUI.BeginProperty(pos, label, prop);
            var contentRect = EditorGUI.PrefixLabel(pos, GUIUtility.GetControlID(FocusType.Passive), label);
            var labels = new[] { new GUIContent("H:", "Hours"), new GUIContent("M:", "Minutes"), new GUIContent("S:", "Seconds") };
            var properties = new[] { prop.FindPropertyRelative("hours"), prop.FindPropertyRelative("minutes"), prop.FindPropertyRelative("seconds") };
            PropertyDrawerHelper.DrawMultiplePropertyFields(contentRect, labels, properties);
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + BottomSpacing;
        }
        #endregion

        #region PRIVATE_METHODS
        #endregion

        #region EVENT_HANDLERS
        #endregion

        #region COROUTINES
        #endregion

        #region UI_CALLBACKS
        #endregion
    }
}