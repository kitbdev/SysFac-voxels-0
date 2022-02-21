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

        [NonSerialized]
        CustomDropDownData customDropDownData;
        [NonSerialized]
        int numLines = 1;

        void DrawDefGUI(Rect position, SerializedProperty property, GUIContent label) =>
            base.OnGUI(position, property, label);
        // EditorGUI.PropertyField(position, property, label, true);
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // GUI.Label(position, "CustomDropDownDrawer");
            CustomDropDownAttribute dropdownAtt = (CustomDropDownAttribute)attribute;

            // CustomDropDownData customDropDownData = null; 
            if (customDropDownData == null) {
                if (dropdownAtt.dropdownDataFieldName != null) {
                    customDropDownData = property.GetValueOnPropRefl<CustomDropDownData>(dropdownAtt.dropdownDataFieldName);
                    if (customDropDownData == null) {
                        Debug.LogError($"Invalid dropdownDataFieldName {dropdownAtt.dropdownDataFieldName} {property.propertyPath}");
                        numLines = 2;
                        // position.height /= 2;
                        Rect labelrect = EditorGUI.IndentedRect(position);
                        string warningText = $"Invalid dropdownDataFieldName {dropdownAtt.dropdownDataFieldName}";
                        // EditorGUI.HelpBox(labelrect, warningText, MessageType.Warning);
                        // backup textfield
                        // position.y += EditorGUIUtility.singleLineHeight;
                        EditorGUI.PropertyField(position, property, label);
                        // DrawDefGUI(position, property, label);
                        return;
                    }
                } else {
                    customDropDownData = CustomDropDownData.Create<object>(
                        property.GetValueOnPropRefl<object[]>(dropdownAtt.choicesListSourceField),
                        null,
                        formatListFunc: dropdownAtt.formatListFuncField == null ? null :
                            property.GetNeighborProperty(dropdownAtt.formatListFuncField)?.GetValue<Func<string, string>>(),
                        formatSelectedValueFunc: dropdownAtt.formatSelectedValueFuncField == null ? null :
                            property.GetNeighborProperty(dropdownAtt.formatSelectedValueFuncField)?.GetValue<Func<string, string>>(),
                        includeNullChoice: dropdownAtt.includeNullChoice,
                        noElementsText: dropdownAtt.noElementsText,
                        errorText: dropdownAtt.errorText
                    );
                    if (customDropDownData == null) {
                        Debug.LogError($"Invalid choicesListSourceField {dropdownAtt.choicesListSourceField}");
                        DrawDefGUI(position, property, label);
                        return;
                    }
                }
            }
            object selectedValue = property.GetValue();
            string selectedValueStr;
            if (customDropDownData.preFormatValueFunc != null) {
                selectedValueStr = customDropDownData.preFormatValueFunc(selectedValue);
            } else {
                selectedValueStr = selectedValue?.ToString() ?? "None";
            }
            if (customDropDownData.formatListFunc != null) {
                selectedValueStr = customDropDownData.formatListFunc(selectedValueStr);
            }
            if (customDropDownData.formatSelectedValueFunc != null) {
                selectedValueStr = customDropDownData.formatSelectedValueFunc(selectedValueStr);
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
                if (customDropDownData.data == null || customDropDownData.data.Length == 0) {
                    numLines = 2;
                    position.height /= 2;
                    Rect labelrect = EditorGUI.IndentedRect(position);
                    if (customDropDownData.data == null) {
                        string warningText;
                        if (customDropDownData.errorText != null) {
                            warningText = customDropDownData.errorText + property.propertyPath;
                        } else {
                            warningText = $"{property.propertyPath} not found. Set choicesListSourceField to a string array!";
                        }
                        EditorGUI.HelpBox(labelrect, warningText, MessageType.Warning);
                        // Debug.LogWarning(text);
                    } else {
                        string warningText = customDropDownData.noElementsText ?? "No choices found!";
                        EditorGUI.HelpBox(labelrect, warningText, MessageType.Warning);
                        // Debug.LogWarning(text);
                    }
                    // backup textfield
                    position.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(position, property, label);
                    return;
                }
                numLines = 1;
                Rect dropdownrect = EditorGUI.PrefixLabel(position, scope.content);
                // create dropdown button
                GUIContent buttonContent = new GUIContent(selectedValueStr);
                if (EditorGUI.DropdownButton(dropdownrect, buttonContent, FocusType.Passive)) {
                    // Debug.Log("clicked");
                    GenericMenu dmenu = new GenericMenu();
                    if (customDropDownData.includeNullChoice) {
                        bool isSet = selectedValue.Equals(null);
                        // bool isSet = selValue == null;
                        string content = "none";
                        if (isSet && customDropDownData.formatSelectedValueFunc != null) {
                            content = customDropDownData.formatSelectedValueFunc(content);
                        }
                        dmenu.AddItem(new GUIContent(content), isSet, SetMenuItemEvent, new ClickMenuData() {
                            property = property, value = null, action = () => {
                                customDropDownData.onSelectCallback?.Invoke(null);
                                customDropDownData = null;
                            }
                        });
                        dmenu.AddSeparator("");
                    }
                    for (int i = 0; i < customDropDownData.data.Length; i++) {
                        CustomDropDownData.Data data = customDropDownData.data[i];
                        object choice = data.value;
                        bool isSet = selectedValue.Equals(choice);
                        string content = customDropDownData.formatListFunc != null ? customDropDownData.formatListFunc(data.name) : data.name;
                        if (isSet && customDropDownData.formatSelectedValueFunc != null) {
                            content = customDropDownData.formatSelectedValueFunc(content);
                        }

                        dmenu.AddItem(new GUIContent(content), isSet, SetMenuItemEvent, new ClickMenuData() {
                            property = property, value = choice, action = () => {
                                customDropDownData.onSelectCallback?.Invoke(choice);
                                customDropDownData = null;
                            }
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

        [Serializable]
        public class ClickMenuData {
            public SerializedProperty property;
            public object value;
            // public int index;
            public Action action;
        }
        public static void SetMenuItemEvent(object data) {
            // Debug.Log("set");
            var clickData = (ClickMenuData)data;
            // todo? object/misc field support too

            clickData.property.serializedObject.Update();

            Undo.RecordObject(clickData.property.serializedObject.targetObject, $"Set DropDown '{clickData.value}' (by ref)");
            bool set = clickData.property.TrySetValueOnPropRefl(clickData.value);
            var valCheck = clickData.property.GetValue();
            // clickData.property.serializedObject.UpdateIfRequiredOrScript();
            // todo why does it fail?
            if (valCheck != clickData.value) {
                // Debug.Log("failed set ref");
                clickData.property.serializedObject.Update();
                // clickData.property.SetValue(clickData.value);
                if (clickData.property.propertyType == SerializedPropertyType.Integer) {
                    clickData.property.intValue = (int)clickData.value;
                }
                // todo other types
            }
            clickData.property.serializedObject.ApplyModifiedProperties();
            clickData.action?.Invoke();
            // var v3 = clickData.property.GetValue();
            // Debug.Log($"Set {(set ? "success" : "failed")} on {clickData.property.serializedObject.targetObject}.{clickData.property.propertyPath} to {clickData.value}={v3}");// = 0({v0})=1({v1})=2({v2})=3({v3})");
        }

        // public override VisualElement CreatePropertyGUI(SerializedProperty property) {
        //     // return base.CreatePropertyGUI(property);
        //     VisualElement root = new VisualElement();

        //     CustomDropDownAttribute cddAttribute = (CustomDropDownAttribute)attribute;
        //     var choices = GetChoices(cddAttribute, property);
        //     choices ??= GetChoicesRef(cddAttribute, property);
        //     if (choices == null) {
        //         root.Add(new Label((cddAttribute.errorText ??
        //                 "Set choicesListSourceField to a string array! ") + property.propertyPath));
        //         // backup string field
        //         TextField textField = new TextField(property.displayName);
        //         textField.BindProperty(property);
        //         root.Add(textField);
        //         return root;
        //     }
        //     if (choices.Count == 0) {
        //         root.Add(new Label(cddAttribute.noElementsText ?? "No choices found"));
        //         return root;
        //     }
        //     DropdownField dropdownField = new DropdownField(property.displayName, choices,
        //         property.stringValue,
        //         GetFunc(cddAttribute.formatSelectedValueFuncField, property),
        //         GetFunc(cddAttribute.formatListFuncField, property));
        //     dropdownField.BindProperty(property);
        //     root.Add(dropdownField);
        //     return root;
        // }

    }
}