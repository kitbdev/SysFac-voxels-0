using UnityEngine;
using UnityEditor;

namespace VoxelSystem {
    [CustomPropertyDrawer(typeof(VoxelMaterialId))]
    public class VoxelMaterialIdDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(VoxelMaterialId.id)), label);
        }
    }
}