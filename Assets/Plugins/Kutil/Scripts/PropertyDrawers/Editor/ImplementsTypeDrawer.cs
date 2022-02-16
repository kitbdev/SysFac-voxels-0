using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
// using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Kutil {
    [CustomPropertyDrawer(typeof(ImplementsType<>))]
    public class ImplementsTypeDrawer : PropertyDrawer {
        // public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

        // }
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            VisualElement root = new VisualElement();
            // Label label = new Label(property.displayName);
            SerializedProperty selNameProp = property.FindPropertyRelative(nameof(ImplementsType<int>._selectedName));
            PropertyField choicesField = new PropertyField(
                selNameProp, property.displayName);
            choicesField.BindProperty(selNameProp);
            root.Add(choicesField);
            // label.Add(choicesField);
            // root.Add(label);
            return root;
        }
    }
}