using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Kutil {
    [CustomPropertyDrawer(typeof(CustomDropDownAttribute))]
    public class CustomDropDownDrawer : PropertyDrawer {
        // public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

        // }
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            // return base.CreatePropertyGUI(property);
            VisualElement root = new VisualElement();


            CustomDropDownAttribute cddAttribute = (CustomDropDownAttribute)attribute;
            var choices = GetChoices(cddAttribute, property);
            choices ??= GetChoicesRef(cddAttribute, property);
            if (choices == null) {
                root.Add(new Label("Set choicesListSourceField to a string array!"));
                // todo backup string field?
                TextField textField = new TextField(property.displayName);
                textField.BindProperty(property);
                root.Add(textField);
                return root;
            }
            if (choices.Count == 0) {
                root.Add(new Label("No choices found"));
                return root;
            }
            DropdownField dropdownField = new DropdownField(property.displayName, choices,
                cddAttribute.defaultIndex,
                GetFunc(cddAttribute.formatSelectedValueFuncField, property),
                GetFunc(cddAttribute.formatListFuncField, property));
            dropdownField.BindProperty(property);
            root.Add(dropdownField);
            return root;
        }
        // public override bool CanCacheInspectorGUI(SerializedProperty property) {
        //     return base.CanCacheInspectorGUI(property);
        // }
        List<string> GetChoices(CustomDropDownAttribute cddAttribute, SerializedProperty property) {
            SerializedProperty sourcePropertyValue = null;
            if (!property.isArray) {
                // gets the property path for the full relative property path
                string propertyPath = property.propertyPath;
                string conditionPath = propertyPath.Replace(property.name, cddAttribute.choicesListSourceField);
                sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);
            } else {
                // note: with arrays doesn't work with nested serializedObjects
                sourcePropertyValue = property.serializedObject.FindProperty(cddAttribute.choicesListSourceField);
            }
            if (sourcePropertyValue == null) {
                sourcePropertyValue = property.serializedObject.FindProperty(cddAttribute.choicesListSourceField);
            }
            if (sourcePropertyValue != null) {
                // Debug.Log($"has value {sourcePropertyValue.ToString()}");
                // check the type
                if (sourcePropertyValue.isArray) {
                    // todo check if list counts
                    if (sourcePropertyValue.arrayElementType == typeof(string).ToString()) {
                        IEnumerator enumerator = sourcePropertyValue.GetEnumerator();
                        List<string> nlist = new List<string>();
                        do {
                            nlist.Add((string)enumerator.Current);
                        } while (enumerator.MoveNext());
                        return nlist;
                    }
                }
            }
            return null;
        }

        public List<string> GetChoicesRef(CustomDropDownAttribute cddAttribute, SerializedProperty property) {
            UnityEngine.Object targetObject = property.serializedObject.targetObject;
            string path = property.propertyPath.Replace(property.name, cddAttribute.choicesListSourceField);
            // Type parentType = targetObject.GetType();
            // Debug.Log($"getting choices field '{path}' on {targetObject} t:{parentType} p:{property.propertyPath}");
            if (ReflectionHelper.TryGetValue<string[]>(targetObject, path, out var val)) {
                return val.ToList();
            }
            return null;
        }
        public Func<string, string> GetFunc(string fieldname, SerializedProperty property) {
            UnityEngine.Object targetObject = property.serializedObject.targetObject;
            string path = property.propertyPath.Replace(property.name, fieldname);
            if (ReflectionHelper.TryGetValue<Func<string, string>>(targetObject, path, out var val)) {
                return val;
            }
            return null;
        }

    }
}