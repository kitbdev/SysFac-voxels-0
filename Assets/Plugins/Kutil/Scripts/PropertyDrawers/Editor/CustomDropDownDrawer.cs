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

        // List<string> choices = null;
        int numLines = 1;

        void DrawDefGUI(Rect position, SerializedProperty property, GUIContent label) =>
            base.OnGUI(position, property, label);
        // EditorGUI.PropertyField(position, property, label, true);
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // GUI.Label(position, "CustomDropDownDrawer");
            CustomDropDownAttribute dropdownAtt = (CustomDropDownAttribute)attribute;

            CustomDropDownData customDropDownData;
            if (dropdownAtt.dropdownDataFieldName != null) {
                // SerializedProperty cddprop = property.GetNeighborProperty(dropdownAtt.dropdownDataFieldName);
                customDropDownData = GetValueOnProp<CustomDropDownData>(dropdownAtt.dropdownDataFieldName, property);
                if (customDropDownData == null) {
                    Debug.LogError($"Invalid dropdownDataFieldName {dropdownAtt.dropdownDataFieldName}");
                    // numLines = 2;
                    DrawDefGUI(position, property, label);
                    return;
                }
            } else {
                customDropDownData = CustomDropDownData.Create<object>(
                    // property.GetNeighborProperty(dropdownAtt.choicesListSourceField)?.GetValue<object[]>(),
                    GetValueOnProp<object[]>(dropdownAtt.choicesListSourceField, property),
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
            object selectedValue = property.GetValue();
            string selectedValueStr;
            if (customDropDownData.preFormatValueFunc != null) {
                selectedValueStr = customDropDownData.preFormatValueFunc(selectedValue);
            } else {
                selectedValueStr = selectedValue.ToString();
            }
            if (customDropDownData.formatListFunc != null) {
                selectedValueStr = customDropDownData.formatListFunc(selectedValueStr);
            }
            if (customDropDownData.formatSelectedValueFunc != null) {
                selectedValueStr = customDropDownData.formatSelectedValueFunc(selectedValueStr);
            }
            // if (fieldInfo.FieldType == typeof(string)) {
            //     selValue = property.stringValue;
            // } else if (fieldInfo.FieldType == typeof(int)) {
            //     selValue = GetValueOnProp<string>(dropdownAtt.selectedChoiceField, property);
            //     if (selValue == null) {
            //         Debug.LogError($"CustomDropDownDrawer invalid dropdownAtt.selectedChoiceField '{dropdownAtt.selectedChoiceField}'");
            //         DrawDefGUI(position, property, label);
            //         return;
            //     }
            // } else {
            //     // need 
            //     Debug.LogError($"CustomDropDownDrawer on {fieldInfo.FieldType},{property.propertyType},{property.type} CustomDropDownAttribute must be on a string or int");
            //     DrawDefGUI(position, property, label);
            //     return;
            // }

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
                // choices ??= GetChoices(dropdownAtt, property);
                // choices ??= GetChoicesRef(dropdownAtt, property);
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
                        // GUI.Label(labelrect, text);
                        EditorGUI.HelpBox(labelrect, warningText, MessageType.Warning);
                        // Debug.LogWarning(text);
                    } else {
                        string warningText = customDropDownData.noElementsText ?? "No choices found!";
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
                            property = property, value = null, index = -1
                        });
                        // if (!customDropDownData.includeEmptyChoice) {
                        dmenu.AddSeparator("");
                        // }
                    }
                    // if (customDropDownData.includeEmptyChoice) {
                    //     bool isSet = selectedValue == "";
                    //     // bool isSet = selValue == "";
                    //     string content = " (empty)";
                    //     if (isSet && customDropDownData.formatSelectedValueFunc != null) {
                    //         content = customDropDownData.formatSelectedValueFunc(content);
                    //     }
                    //     dmenu.AddItem(new GUIContent(content), isSet, SetMenuItemEvent, new ClickMenuData() {
                    //         property = property, value = "", index = -1
                    //     });
                    //     dmenu.AddSeparator("");
                    // }
                    for (int i = 0; i < customDropDownData.data.Length; i++) {
                        CustomDropDownData.Data data = customDropDownData.data[i];
                        object choice = data.value;
                        bool isSet = selectedValue.Equals(choice);
                        string content = customDropDownData.formatListFunc != null ? customDropDownData.formatListFunc(data.name) : data.name;
                        if (isSet && customDropDownData.formatSelectedValueFunc != null) {
                            content = customDropDownData.formatSelectedValueFunc(content);
                        }
                        dmenu.AddItem(new GUIContent(content), isSet, SetMenuItemEvent, new ClickMenuData() {
                            property = property, value = choice, index = i
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
            public int index;
            public object value;
        }
        public static void SetMenuItemEvent(object data) {
            // Debug.Log("set");
            var clickData = (ClickMenuData)data;
            // todo? object/misc field support too

            // if (clickData.property.propertyType == SerializedPropertyType.String) {
            //     clickData.property.stringValue = clickData.value;
            // } else if (clickData.property.propertyType == SerializedPropertyType.Integer) {
            //     clickData.property.intValue = clickData.index;
            // }
            // clickData.property.SetValue(clickData.value);
            // var v0 = clickData.property.GetValue();
            bool set = TrySetValueOnProp(clickData.value, clickData.property);
            var valCheck = clickData.property.GetValue();
            // todo why does it fail?
            if (valCheck != clickData.value) {
                // Debug.Log("failed set ref");
                clickData.property.serializedObject.Update();
                // clickData.property.SetValue(clickData.value);
                if (clickData.property.propertyType == SerializedPropertyType.Integer) {
                    clickData.property.intValue = (int)clickData.value;
                }
                // todo other types
                clickData.property.serializedObject.ApplyModifiedProperties();
            }
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
        // // public override bool CanCacheInspectorGUI(SerializedProperty property) {
        // //     return base.CanCacheInspectorGUI(property);
        // // }
        // List<string> GetChoices(CustomDropDownAttribute cddAttribute, SerializedProperty property) {
        //     SerializedProperty sourcePropertyValue = null;
        //     if (!property.isArray) {
        //         // gets the property path for the full relative property path
        //         string path = property.propertyPath.Replace(property.name, cddAttribute.choicesListSourceField);
        //         sourcePropertyValue = property.serializedObject.FindProperty(path);
        //     } else {
        //         // note: with arrays doesn't work with nested serializedObjects
        //         sourcePropertyValue = property.serializedObject.FindProperty(cddAttribute.choicesListSourceField);
        //     }
        //     if (sourcePropertyValue == null) {
        //         sourcePropertyValue = property.serializedObject.FindProperty(cddAttribute.choicesListSourceField);
        //     }
        //     if (sourcePropertyValue == null) {
        //         sourcePropertyValue = property.FindPropertyRelative(cddAttribute.choicesListSourceField);
        //     }
        //     if (sourcePropertyValue != null) {
        //         // Debug.Log($"has value {sourcePropertyValue.ToString()}");
        //         // check the type
        //         if (sourcePropertyValue.isArray) {
        //             if (sourcePropertyValue.arrayElementType == typeof(string).ToString()) {
        //                 IEnumerator enumerator = sourcePropertyValue.GetEnumerator();
        //                 List<string> nlist = new List<string>();
        //                 do {
        //                     nlist.Add((string)enumerator.Current);
        //                 } while (enumerator.MoveNext());
        //                 return nlist;
        //             }
        //         }
        //     }
        //     return null;
        // }

        // public static List<string> GetChoicesRef(CustomDropDownAttribute cddAttribute, SerializedProperty property) {
        //     return GetValueOnProp<string[]>(cddAttribute.choicesListSourceField, property)?.ToList();
        //     // UnityEngine.Object targetObject = property.serializedObject.targetObject;
        //     // string path = property.propertyPath.Replace(property.name, cddAttribute.choicesListSourceField);
        //     // // Type parentType = targetObject.GetType();
        //     // // Debug.Log($"getting choices field '{path}' on {targetObject} t:{parentType} p:{property.propertyPath}");
        //     // if (ReflectionHelper.TryGetValue<string[]>(targetObject, path, out var val)) {
        //     //     return val?.ToList();
        //     // }
        //     // return null;
        // }
        // public static Func<string, string> GetFunc(string fieldname, SerializedProperty property) {
        //     return GetValueOnProp<Func<string, string>>(fieldname, property);
        //     // UnityEngine.Object targetObject = property.serializedObject.targetObject;
        //     // string path = property.propertyPath.Replace(property.name, fieldname);
        //     // if (ReflectionHelper.TryGetValue<Func<string, string>>(targetObject, path, out var val)) {
        //     //     return val;
        //     // }
        //     // return null;
        // }
        public static T GetValueOnProp<T>(string fieldname, SerializedProperty property) {
            UnityEngine.Object targetObject = property.serializedObject.targetObject;
            string path = property.propertyPath.Replace(property.name, fieldname);
            if (ReflectionHelper.TryGetValue<T>(targetObject, path, out var val)) {
                return val;
            }
            return default;
        }
        public static bool TrySetValueOnProp(object value, SerializedProperty property, string fieldname = null) {
            UnityEngine.Object targetObject = property.serializedObject.targetObject;
            string path = fieldname == null ? property.propertyPath :
                property.propertyPath.Replace(property.name, fieldname);
            return ReflectionHelper.TrySetValue(value, targetObject, path);
        }

    }
}