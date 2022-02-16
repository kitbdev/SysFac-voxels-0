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

        List<string> choices = null;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // GUI.Label(position, "CustomDropDownDrawer");
            CustomDropDownAttribute cddAttribute = (CustomDropDownAttribute)attribute;

            string parentPath = property.propertyPath.Replace("." + property.name, "");
            if (parentPath.EndsWith(']')) {
                // in array element name fix
                string propertyPath = property.propertyPath;
                int startIndex = propertyPath.LastIndexOf('[') + 1;
                // Debug.Log($"array {propertyPath} {startIndex} {parentPath}");
                // todo shouldnt have to do this right?
                label.text = "Element " + propertyPath.Substring(startIndex, propertyPath.LastIndexOf(']') - startIndex);
            }

            using (var scope = new EditorGUI.PropertyScope(position, label, property)) {
                Rect dropdownrect = EditorGUI.PrefixLabel(position, scope.content);
                choices ??= GetChoices(cddAttribute, property);
                choices ??= GetChoicesRef(cddAttribute, property);
                if (choices == null) {
                    GUI.Label(position, "Set choicesListSourceField to a string array! " + property.propertyPath);
                    // backup textfield?
                    return;
                }
                if (choices.Count == 0) {
                    GUI.Label(position, "No choices found!");
                    return;
                }
                // create dropdown button
                GUIContent buttonContent = new GUIContent(property.stringValue);
                if (EditorGUI.DropdownButton(dropdownrect, buttonContent, FocusType.Passive)) {
                    // Debug.Log("clicked");
                    GenericMenu dmenu = new GenericMenu();
                    foreach (var choice in choices) {
                        bool isSet = property.stringValue == choice;
                        dmenu.AddItem(new GUIContent(choice), isSet, SetMenuItemEvent, new ClickMenuData() {
                            property = property, value = choice
                        });
                    }
                    dmenu.DropDown(dropdownrect);
                }
            }
        }

        public class ClickMenuData {
            public SerializedProperty property;
            public string value;
        }
        public static void SetMenuItemEvent(object data) {
            // Debug.Log("set");
            var clickData = (ClickMenuData)data;
            // todo int field support too
            clickData.property.stringValue = clickData.value;
            clickData.property.serializedObject.ApplyModifiedProperties();
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            // return base.CreatePropertyGUI(property);
            VisualElement root = new VisualElement();

            CustomDropDownAttribute cddAttribute = (CustomDropDownAttribute)attribute;
            var choices = GetChoices(cddAttribute, property);
            choices ??= GetChoicesRef(cddAttribute, property);
            if (choices == null) {
                root.Add(new Label("Set choicesListSourceField to a string array! " + property.propertyPath));
                // backup string field
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
                string path = property.propertyPath.Replace(property.name, cddAttribute.choicesListSourceField);
                sourcePropertyValue = property.serializedObject.FindProperty(path);
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