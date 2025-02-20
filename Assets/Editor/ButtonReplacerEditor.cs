using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using Tag.NutSort;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace com.tag.editor
{
    public class ButtonReplacerEditor : OdinEditorWindow
    {
        [SerializeField] private List<Button> buttons = new List<Button>();

        [MenuItem("Tools/Button Replacer")]
        public static void ShowWindow()
        {
            GetWindow<ButtonReplacerEditor>("ButtonReplacerEditor");
        }

        [Button]
        private void ReplaceAllButton()
        {
            foreach (Button button in buttons)
            {
                if (button is not CustomButton)
                    ReplaceSelectedButton(button);
            }
            buttons.Clear();
        }

        private void ReplaceSelectedButton(Button button)
        {
            GameObject selectedGameObject = button.gameObject;

            Button selectedButton = selectedGameObject.GetComponent<Button>();
            if (selectedButton == null)
            {
                Debug.LogWarning("No Button component found on the selected GameObject.");
                return;
            }

            DestroyImmediate(selectedButton);

            CustomButton newButton = selectedGameObject.AddComponent<CustomButton>();

            newButton.onClick = selectedButton.onClick;
            newButton.transition = Selectable.Transition.None;
            SceneView.RepaintAll();
        }
    }
}
