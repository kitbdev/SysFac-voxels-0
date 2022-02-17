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
        int numLines = 1;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // GUI.Label(position, "CustomDropDownDrawer");
            CustomDropDownAttribute dropdownAtt = (CustomDropDownAttribute)attribute;

            if (fieldInfo.FieldType != typeof(string)) {
                Debug.LogError($"CustomDropDownAttribute must be on a string");
                base.OnGUI(position, property, label);
                return;
            }

            string parentPath = property.propertyPath.Replace("." + property.name, "");
            if (parentPath.EndsWith(']')) {
                // in array element name fix
                string propertyPath = property.propertyPath;
                int startIndex = propertyPath.LastIndexOf('[') + 1;
                // Debug.Log($"array {propertyPath} {startIndex} {parentPath}");
                // todo? shouldnt have to do this right?
                label.text = "Element " + propertyPath.Substring(startIndex, propertyPath.LastIndexOf(']') - startIndex);
            }

            using (var scope = new EditorGUI.PropertyScope(position, label, property)) {
                choices ??= GetChoices(dropdownAtt, property);
                choices ??= GetChoicesRef(dropdownAtt, property);
                if (choices == null || choices.Count == 0) {
                    numLines = 2;
                    position.height /= 2;
                    Rect labelrect = EditorGUI.IndentedRect(position);
                    if (choices == null) {
                        string warningText;
                        if (dropdownAtt.errorText != null) {
                            warningText = dropdownAtt.errorText + property.propertyPath;
                        } else {
                            warningText = $"{property.propertyPath} not found. Set choicesListSourceField to a string array!";
                        }
                        // GUI.Label(labelrect, text);
                        EditorGUI.HelpBox(labelrect, warningText, MessageType.Warning);
                        // Debug.LogWarning(text);
                    } else {
                        string warningText = dropdownAtt.noElementsText ?? "No choices found!";
                        EditorGUI.HelpBox(labelrect, warningText, MessageType.Warning);
                        // GUI.Label(labelrect, text);
                        // Debug.LogWarning(text);
                    }
                    // backup textfield
                    position.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(position, property, label);
                    return;
                }
                numLines = 1;
                Rect dropdownrect = EditorGUI.PrefixLabel(position, scope.content);
                Func<string, string> selValFunc = GetFunc(dropdownAtt.formatSelectedValueFuncField, property);
                Func<string, string> listFormatFunc = GetFunc(dropdownAtt.formatListFuncField, property);
                // create dropdown button
                GUIContent buttonContent = new GUIContent(property.stringValue);
                if (EditorGUI.DropdownButton(dropdownrect, buttonContent, FocusType.Passive)) {
                    // Debug.Log("clicked");
                    GenericMenu dmenu = new GenericMenu();
                    if (dropdownAtt.includeNullChoice) {
                        bool isSet = ReferenceEquals(property.stringValue, null);
                        // bool isSet = property.stringValue == null;
                        string content = "none";
                        if (isSet && selValFunc != null) {
                            content = selValFunc(content);
                        }
                        dmenu.AddItem(new GUIContent(content), isSet, SetMenuItemEvent, new ClickMenuData() {
                            property = property, value = null
                        });
                        if (!dropdownAtt.includeEmptyChoice) {
                            dmenu.AddSeparator("");
                        }
                    }
                    if (dropdownAtt.includeEmptyChoice) {
                        bool isSet = ReferenceEquals(property.stringValue, "");
                        // bool isSet = property.stringValue == "";
                        string content = " (empty)";
                        if (isSet && selValFunc != null) {
                            content = selValFunc(content);
                        }
                        dmenu.AddItem(new GUIContent(content), isSet, SetMenuItemEvent, new ClickMenuData() {
                            property = property, value = ""
                        });
                        dmenu.AddSeparator("");
                    }
                    foreach (var choice in choices) {
                        bool isSet = property.stringValue == choice;
                        string content = listFormatFunc != null ? listFormatFunc(choice) : choice;
                        if (isSet && selValFunc != null) {
                            content = selValFunc(content);
                        }
                        dmenu.AddItem(new GUIContent(content), isSet, SetMenuItemEvent, new ClickMenuData() {
                            property = property, value = choice
                        });
                    }
                    dmenu.DropDown(dropdownrect);
                }
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            // int numLines = (choices == null || choices.Count <= 0 ? 2 : 1);
            return EditorGUIUtility.singleLineHeight * numLines;
            // return base.GetPropertyHeight(property, label);
        }

        public class ClickMenuData {
            public SerializedProperty property;
            public string value;
        }
        public static void SetMenuItemEvent(object data) {
            // Debug.Log("set");
            var clickData = (ClickMenuData)data;
            // todo int field support too, and objects?
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
                root.Add(new Label((cddAttribute.errorText ??
                        "Set choicesListSourceField to a string array! ") + property.propertyPath));
                // backup string field
                TextField textField = new TextField(property.displayName);
                textField.BindProperty(property);
                root.Add(textField);
                return root;
            }
            if (choices.Count == 0) {
                root.Add(new Label(cddAttribute.noElementsText ?? "No choices found"));
                return root;
            }
            DropdownField dropdownField = new DropdownField(property.displayName, choices,
                property.stringValue,
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
            if (sourcePropertyValue == null) {
                sourcePropertyValue = property.FindPropertyRelative(cddAttribute.choicesListSourceField);
            }
            if (sourcePropertyValue != null) {
                // Debug.Log($"has value {sourcePropertyValue.ToString()}");
                // check the type
                if (sourcePropertyValue.isArray) {
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