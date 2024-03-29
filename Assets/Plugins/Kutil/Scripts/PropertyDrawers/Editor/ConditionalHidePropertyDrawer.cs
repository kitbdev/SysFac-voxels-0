﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Kutil {
    ///Original version of the ConditionalHideAttribute created by Brecht Lecluyse (www.brechtos.com)
    ///Modified by: Sebastian Lague

    [CustomPropertyDrawer(typeof(ConditionalHideAttribute))]
    public class ConditionalHidePropertyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            ConditionalHideAttribute condHAtt = (ConditionalHideAttribute)attribute;

            bool enabled = GetConditionalHideAttributeResult(condHAtt, property) == condHAtt.showIfTrue;

            if (enabled) {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            ConditionalHideAttribute condHAtt = (ConditionalHideAttribute)attribute;
            bool enabled = GetConditionalHideAttributeResult(condHAtt, property) == condHAtt.showIfTrue;

            if (enabled) {
                return EditorGUI.GetPropertyHeight(property, label);
            }
            //We want to undo the spacing added before and after the property
            return -EditorGUIUtility.standardVerticalSpacing;

        }

        bool GetConditionalHideAttributeResult(ConditionalHideAttribute condHAtt, SerializedProperty property) {
            SerializedProperty sourcePropertyValue = null;

            // Get the full relative property path of the sourcefield so we can have nested hiding.
            // Use old method when dealing with arrays
            if (!property.isArray) {
                // returns the property path of the property we want to apply the attribute to
                string propertyPath = property.propertyPath;
                // changes the path to the conditionalsource property path
                string conditionPath = propertyPath.Replace(property.name, condHAtt.conditionalSourceField);
                sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

            } else {
                // original implementation (doens't work with nested serializedObjects)
                sourcePropertyValue = property.serializedObject.FindProperty(condHAtt.conditionalSourceField);
            }

            // if the find failed->fall back to the old system
            if (sourcePropertyValue == null) {
                // original implementation (doens't work with nested serializedObjects)
                sourcePropertyValue = property.serializedObject.FindProperty(condHAtt.conditionalSourceField);
            }
            if (sourcePropertyValue != null) {
                return CheckPropertyType(condHAtt, sourcePropertyValue);
            } else {
                // use reflection instead, should support arrays and nesting
                string path = property.propertyPath.Replace(property.name, condHAtt.conditionalSourceField);
                Object targetObject = property.serializedObject.targetObject;
                if (ReflectionHelper.TryGetValue<bool>(targetObject, path, out var value)) {
                    return value;
                } else if (ReflectionHelper.TryGetValue<System.Enum>(targetObject, path, out var evalue)) {
                    if (condHAtt.enumIndices == null || condHAtt.enumIndices.Length == 0) {
                        return false;
                    }
                    if (System.Enum.GetUnderlyingType(typeof(System.Enum)) != typeof(int)) {
                        return true;
                    } 
                    // todo test this
                    int eintval = (int)System.Convert.ChangeType(evalue, typeof(int));
                    return condHAtt.enumIndices.Contains(eintval);
                }
            }
            return true;
        }

        bool CheckPropertyType(ConditionalHideAttribute condHAtt, SerializedProperty sourcePropertyValue) {
            //Note: add others for custom handling if desired
            switch (sourcePropertyValue.propertyType) {
                case SerializedPropertyType.Boolean:
                    return sourcePropertyValue.boolValue;
                case SerializedPropertyType.Enum:
                    if (condHAtt.enumIndices == null || condHAtt.enumIndices.Length == 0) {
                        return false;
                    }
                    return condHAtt.enumIndices.Contains(sourcePropertyValue.enumValueIndex);
                default:
                    Debug.LogError("Data type of the property used for conditional hiding [" + sourcePropertyValue.propertyType + "] is currently not supported");
                    return true;
            }
        }
    }
}