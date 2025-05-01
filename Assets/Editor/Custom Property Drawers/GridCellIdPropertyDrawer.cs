using UnityEditor;
using UnityEngine;

namespace Tag.NutSort.Editor
{
    [CustomPropertyDrawer(typeof(GridCellId))]
    public class GridCellIdPropertyDrawer : PropertyDrawer
    {
        #region PRIVATE_VARIABLES
        private const float BottomSpacing = 2;
        #endregion

        #region PUBLIC_METHODS
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            pos.height -= BottomSpacing; 
            label = EditorGUI.BeginProperty(pos, label, prop);

            var contentRect = EditorGUI.PrefixLabel(pos, GUIUtility.GetControlID(FocusType.Passive), label);

            var labels = new[] { new GUIContent("R ", "Row"), new GUIContent("C ", "Colomn") };
            var properties = new[] { prop.FindPropertyRelative("rowNumber"), prop.FindPropertyRelative("colNumber") };

            PropertyDrawerHelper.DrawMultiplePropertyFields(contentRect, labels, properties);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + BottomSpacing;
        }
        #endregion
    }

    public static class PropertyDrawerHelper
    {
        private const float SubLabelSpacing = 4;
        public static void DrawMultiplePropertyFields(Rect pos, GUIContent[] subLabels, SerializedProperty[] props)
        {
            var indent = EditorGUI.indentLevel;
            var labelWidth = EditorGUIUtility.labelWidth;

            var propsCount = props.Length;
            if (propsCount == 0) return;

            var fieldAreaWidth = (pos.width - (propsCount - 1) * SubLabelSpacing) / propsCount;

            var currentX = pos.x; 

            EditorGUI.indentLevel = 0;

            for (var i = 0; i < propsCount; i++)
            {
                if (props[i] == null)
                {
                    Debug.LogError($"Property field index {i} is null.");
                    var errorRect = new Rect(currentX, pos.y, fieldAreaWidth, pos.height);
                    EditorGUI.LabelField(errorRect, "Error");
                }
                else
                {
                    var subLabelSize = EditorStyles.label.CalcSize(subLabels[i]);
                    var subLabelWidth = subLabelSize.x;

                    var fieldRect = new Rect(currentX, pos.y, fieldAreaWidth, pos.height);

                    EditorGUIUtility.labelWidth = subLabelWidth;

                    EditorGUI.PropertyField(fieldRect, props[i], subLabels[i]);
                }
                currentX += fieldAreaWidth + SubLabelSpacing;
            }
            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUI.indentLevel = indent;
        }
    }
}