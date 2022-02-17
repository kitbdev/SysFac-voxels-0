using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
// using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Kutil {
    // [CustomPropertyDrawer(typeof(TypeSelector<>))]
    public class TypeSelectorDrawer : PropertyDrawer {
        string lastTypeName = null;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // SerializedProperty selNameProp = property.FindPropertyRelative(nameof(ImplementsType<int>._selectedName));
            using (var scope = new EditorGUI.PropertyScope(position, label, property)) {
                SerializedProperty typeprop = property.FindPropertyRelative(nameof(TypeSelector<int>._type));
                SerializedProperty objprop = property.FindPropertyRelative(nameof(TypeSelector<int>._obj));
                EditorGUI.BeginChangeCheck();
                Rect typepos = position;
                typepos.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(typepos, typeprop, new GUIContent(typeprop.displayName), typeprop.isExpanded);
                if (lastTypeName == null || EditorGUI.EndChangeCheck()) {
                    if (ReflectionHelper.TryGetValue<System.Object>(property.serializedObject.targetObject, objprop.propertyPath, out var typename)) {
                        // Debug.Log("got " + typeprop.type);
                        lastTypeName = typename?.GetType()?.Name ?? "unknown";
                    }
                    // ((UnityEngine.Object)property.serializedObject.targetObject)

                    // .SendMessage("OnValidate", null, SendMessageOptions.DontRequireReceiver);
                }

                position.y += EditorGUIUtility.singleLineHeight;
                GUIContent objContent = new GUIContent(lastTypeName ?? "unknown");
                EditorGUI.PropertyField(position, objprop, objContent, true);
                if (EditorGUI.EndChangeCheck()) {
                    // if (ReflectionHelper.TryGetValue<TypeSelector<int>>(property.serializedObject.targetObject, property.propertyPath, out var typeSelector)) {

                    // }
                }
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            SerializedProperty objprop = property.FindPropertyRelative(nameof(TypeSelector<int>.obj));
            return EditorGUIUtility.singleLineHeight
                + EditorGUI.GetPropertyHeight(objprop, objprop.isExpanded);
            // + base.GetPropertyHeight(objprop, label); // obj height
        }
        // public override VisualElement CreatePropertyGUI(SerializedProperty property) {
        //     VisualElement root = new VisualElement();
        //     // // Label label = new Label(property.displayName);
        //     // SerializedProperty selNameProp = property.FindPropertyRelative(nameof(ImplementsType<int>._selectedName));
        //     // PropertyField choicesField = new PropertyField(
        //     //     selNameProp, property.displayName);
        //     // choicesField.BindProperty(selNameProp);
        //     // root.Add(choicesField);
        //     // // label.Add(choicesField);
        //     // // root.Add(label);
        //     return root;
        // }
    }
}