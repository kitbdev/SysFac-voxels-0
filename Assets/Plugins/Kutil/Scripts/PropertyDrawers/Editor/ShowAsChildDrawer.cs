using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
// using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Kutil {
    [CustomPropertyDrawer(typeof(ShowAsChildAttribute))]
    public class ShowAsChildDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            ShowAsChildAttribute sacAttribute = (ShowAsChildAttribute)attribute;
            SerializedProperty childProp = property.FindPropertyRelative(sacAttribute.childSourceField);
            EditorGUI.PropertyField(position, childProp, label, false);
        }
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            VisualElement root = new VisualElement();
            // Label label = new Label(property.displayName);
            ShowAsChildAttribute sacAttribute = (ShowAsChildAttribute)attribute;
            SerializedProperty childProp = property.FindPropertyRelative(sacAttribute.childSourceField);
            PropertyField chieldPropField = new PropertyField(childProp, property.displayName);
            chieldPropField.BindProperty(childProp);
            root.Add(chieldPropField);
            return root;
        }
    }
}