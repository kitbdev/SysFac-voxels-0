using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
// using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Kutil {
    /// <summary>
    /// Inherit this for an easy way to show as child
    /// </summary>
    public class ShowAsChildPropertyDrawer : PropertyDrawer {
        public virtual string childName => "";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            SerializedProperty selNameProp = property.FindPropertyRelative(childName);
            if (selNameProp == null) {
                base.OnGUI(position, property, label);
                return;
            }
            EditorGUI.PropertyField(position, selNameProp, label, false);
        }
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            VisualElement root = new VisualElement();
            // Label label = new Label(property.displayName);
            SerializedProperty selNameProp = property.FindPropertyRelative(childName);
            if (selNameProp == null) {
                return base.CreatePropertyGUI(property);
            }
            PropertyField childField = new PropertyField(
                selNameProp, property.displayName);
            childField.BindProperty(selNameProp);
            root.Add(childField);
            return root;
        }
    }
}