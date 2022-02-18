using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
// using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Kutil {
    // [CustomPropertyDrawer(typeof(TypeChoice<>))]
    public class TypeChoiceDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            SerializedProperty selNameProp = property.FindPropertyRelative(nameof(TypeChoice<int>._selectedName));
            EditorGUI.PropertyField(position, selNameProp, label, false);
        }
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            VisualElement root = new VisualElement();
            // Label label = new Label(property.displayName);
            SerializedProperty selNameProp = property.FindPropertyRelative(nameof(TypeChoice<int>._selectedName));
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