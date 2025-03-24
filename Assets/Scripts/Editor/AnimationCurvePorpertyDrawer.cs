using Newtonsoft.Json;
using System;
using UnityEditor;
using UnityEngine;

namespace Tag.NutSort.Editor
{
    // <summary>
    // Custom property drawer for AnimationCurve that adds copy and paste functionality.
    // This drawer adds two buttons next to each AnimationCurve field in the Inspector:
    // - 'C' button: Copies the AnimationCurve data as JSON to the clipboard.
    // - 'P' button: Pastes AnimationCurve data from the clipboard JSON to the field.
    // 
    // This script allows copying and pasting AnimationCurves across different Unity Editor instances,
    // making it easy to transfer curves between projects or scenes.
    // </summary>

    [CustomPropertyDrawer(typeof(AnimationCurve))]
    public class AnimationCurvePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position.width -= 60;
            EditorGUI.PropertyField(position, property, label);

            position.x += position.width + 5;
            position.width = 25;
            if (GUI.Button(position, "C"))
            {
                string json = JsonConvert.SerializeObject(property.animationCurveValue);
                EditorGUIUtility.systemCopyBuffer = json;
            }

            position.x += 30;
            if (GUI.Button(position, "P"))
            {
                string json = EditorGUIUtility.systemCopyBuffer;
                try
                {
                    AnimationCurve curve = JsonConvert.DeserializeObject<AnimationCurve>(json);
                    property.animationCurveValue = curve;
                    property.serializedObject.ApplyModifiedProperties();
                }
                catch (Exception e)
                {
                    Debug.LogError("Invalid AnimationCurve JSON in clipboard " + e);
                }
            }
            EditorGUI.EndProperty();
        }
    }

}
