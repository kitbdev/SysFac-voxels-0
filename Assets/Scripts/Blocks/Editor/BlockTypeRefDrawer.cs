using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;

[CustomPropertyDrawer(typeof(BlockTypeRef))]
public class BlockTypeRefDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(BlockTypeRef.idname)), label);
    }
    // public override VisualElement CreatePropertyGUI(SerializedProperty property) {
    //     // return base.CreatePropertyGUI(property);
    //     VisualElement root = new VisualElement();

    //     BlockManager blockManager = BlockManager.Instance;
    //     // blockManager ??= GameObject.FindObjectOfType<BlockManager>();
    //     List<string> typeNames = blockManager.blockTypes.Select((b) => b.idname).ToList();
    //     if (typeNames.Count == 0) {
    //         root.Add(new Label("No Types! check BlockManager"));
    //         return root;
    //     }
    //     var idprop = property.FindPropertyRelative(nameof(BlockTypeRef.idname));

    //     DropdownField dropdownField = new DropdownField(typeNames,
    //         typeNames[0]);
    //     dropdownField.BindProperty(idprop);
    //     dropdownField.label = property.displayName;
    //     // dropdownField.choices = typeNames;
    //     // dropdownField.RegisterValueChangedCallback((cv)=>{pro cv.newValue});
    //     root.Add(dropdownField);
    //     return root;
    // }
}
