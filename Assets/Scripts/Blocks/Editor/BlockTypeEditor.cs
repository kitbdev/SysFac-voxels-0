using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.Linq;

public class BlockTypeEditor : EditorWindow {

    [MenuItem("SysFac/BlockType")]
    private static void ShowWindow() {
        var window = GetWindow<BlockTypeEditor>();
        window.titleContent = new GUIContent("BlockType");
        window.Show();
    }
    private void OnEnable() {
        // setup window
        VisualElement container = new VisualElement();
        rootVisualElement.Add(container);

        // container.Add();
        
    }

}