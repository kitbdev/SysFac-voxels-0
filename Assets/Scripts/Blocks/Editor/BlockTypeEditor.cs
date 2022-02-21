using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.Linq;
using Kutil;
using VoxelSystem;

public class BlockTypeEditor : EditorWindow {

    SerializedObject serializedObject;
    [SerializeField] VoxelMaterialSetSO voxelMaterialSet;
    [SerializeField] BlockTypesHolderSO blockTypesHolder;

    [System.Serializable]
    class BlockTypeMaker {
        public string displayName = "";
        public TypeSelector<VoxelMaterial> vmat = new TypeSelector<VoxelMaterial>(typeof(TexturedMaterial));
        public List<TypeSelector<VoxelData>> customDatas = new List<TypeSelector<VoxelData>>();
    }
    [Header("Block editor")]
    [ContextMenuItem("add type", nameof(AddNewBlockType))]
    [ContextMenuItem("clear all", nameof(ClearAllBlockTypesAndMats))]
    [SerializeField] BlockTypeMaker blockTypeToAdd;


    [MenuItem("SysFac/BlockTypeEditor")]
    private static void ShowWindow() {
        var window = GetWindow<BlockTypeEditor>();
        window.titleContent = new GUIContent("BlockType");
        window.Show();
    }
    private void OnEnable() {
        serializedObject = new SerializedObject(this);
        // setup window
        VisualElement container = new VisualElement();
        rootVisualElement.Add(container);

        container.Add(new PropertyField(serializedObject.FindProperty(nameof(voxelMaterialSet))));
        container.Add(new PropertyField(serializedObject.FindProperty(nameof(blockTypesHolder))));
        container.Add(new PropertyField(serializedObject.FindProperty(nameof(blockTypeToAdd))));

        Button addbtn = new Button(() => AddNewBlockType());
        addbtn.text = "Add block type";
        container.Add(addbtn);

        Button clrbtn = new Button(() => ClearAllBlockTypesAndMats());
        clrbtn.text = "Remove all block types";
        container.Add(clrbtn);

        Button savebtn = new Button();
        savebtn.text = "Save all block types";
        // container.Add(savebtn);
        Button loadbtn = new Button();
        loadbtn.text = "Load block types";
        // container.Add(loadbtn);

        container.Bind(serializedObject);
    }

    void AddNewBlockType() {
        CreateBlockTypeAndMat(blockTypeToAdd);
        blockTypeToAdd = new BlockTypeMaker();
    }

    public void ClearAllBlockTypesAndMats() {
        voxelMaterialSet?.ClearVoxelMats();
        blockTypesHolder?.ClearAllBlockTypes();
        CreateBlockTypeAndMat(new BlockTypeMaker() {
            displayName = "Air", vmat = new TypeSelector<VoxelMaterial>(new TexturedMaterial() {
                isInvisible = true, isTransparent = true
            })
        });
        CreateBlockTypeAndMat(new BlockTypeMaker() {
            displayName = "Debug", vmat = new TypeSelector<VoxelMaterial>(new TexturedMaterial() {
                isInvisible = false, isTransparent = false
            })
        });
    }
    void CreateBlockTypeAndMat(BlockTypeMaker data) {
        if (voxelMaterialSet == null || data == null || data.displayName == "") {
            return;
        }
        BlockType blockType = new BlockType();
        blockType.displayName = data.displayName;
        blockType.idname = ToIdName(data.displayName);
        blockType.customDatas = data.customDatas.Select(tsvd => tsvd.type).ToHashSet().ToArray();
        // if (data.vmat.objvalue is TexturedMaterial bvm) {
        //     if (bvm.texname == "") {
        //         bvm.texname = blockType.idname;
        //     }
        // }
        VoxelMaterialId voxelMaterialId = voxelMaterialSet.AddVoxelMaterial(data.vmat.objvalue);
        blockType.voxelMaterialId = voxelMaterialId;
        blockTypesHolder?.AddBlockTypes(blockType);
    }

    static string ToIdName(string displayName) {
        return displayName.Replace(" ", "").ToLower();
    }
}