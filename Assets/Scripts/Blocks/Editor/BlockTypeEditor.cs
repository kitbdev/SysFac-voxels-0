using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.Linq;
using Kutil;
using VoxelSystem;
using static MapSO;

public class BlockTypeEditor : EditorWindow {

    SerializedObject serializedObject;
    [SerializeField] VoxelMaterialSetSO voxelMaterialSet;
    [SerializeField] BlockTypesHolderSO blockTypesHolder;
    [SerializeField] MapSO mapData;
    [SerializeField] TextAsset blockTypesCSV;

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

        container.Add(new PropertyField(serializedObject.FindProperty(nameof(blockTypesCSV))));
        Button loadbtn = new Button(() => ImportBlockTypesCSV());
        loadbtn.text = "Load block types CSV";
        container.Add(loadbtn);
        container.Add(new PropertyField(serializedObject.FindProperty(nameof(mapData))));
        // Button loadbtn2 = new Button(() => ImportBlockConversionCSV());
        // loadbtn2.text = "Load MapData conversion CSV";
        // container.Add(loadbtn2);
        // VisualElement mapcontainter = new VisualElement();
        // container.Add(mapcontainter);
        Button preprocessmapbtn = new Button(() => PreprocessMap());
        preprocessmapbtn.text = "Preprocess Map Data";
        container.Add(preprocessmapbtn);
        // Button clearmapdatabtn = new Button(() => ClearMap());
        // clearmapdatabtn.text = "Clear Map Data";
        // container.Add(clearmapdatabtn);

        container.Bind(serializedObject);
    }
    void PreprocessMap() => mapData.ProcessImportData();
    // void ClearMap() => mapData.ClearMapData();

    void AddNewBlockType() {
        CreateBlockTypeAndMat(blockTypeToAdd);
        blockTypeToAdd = new BlockTypeMaker();
    }

    void HardClearAllBlockTypesAndMats() {
        voxelMaterialSet?.ClearVoxelMats();
        blockTypesHolder?.ClearAllBlockTypes();
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
        if (data == null) {
            // add empty to take id
            blockTypesHolder?.AddBlockTypes(new BlockType() {
                idname = "none", voxelMaterialId = 1,// debug mat
            });
        }
        if (voxelMaterialSet == null || data == null || data.displayName == "") {
            return;
        }
        BlockType blockType = new BlockType();
        blockType.displayName = data.displayName;
        blockType.idname = ToIdName(data.displayName);
        blockType.customDatas = data.customDatas.Select(tsvd => tsvd.type).ToHashSet().ToArray();
        if (data.vmat.objvalue is TexturedMaterial bvm) {
            if (bvm.isTransparent) {
                bvm.materialIndex = 1;// todo set from file?
            }
            // if (bvm.texname == "") {
            //     bvm.texname = blockType.idname;
            // }
        }
        VoxelMaterialId voxelMaterialId = voxelMaterialSet.AddVoxelMaterial(data.vmat.objvalue);
        blockType.voxelMaterialId = voxelMaterialId;
        blockTypesHolder?.AddBlockTypes(blockType);
    }

    static string ToIdName(string displayName) {
        return displayName.Replace(" ", "").ToLower();
    }

    [ContextMenu("Import block types CSV")]
    public void ImportBlockTypesCSV() {
        if (blockTypesCSV == null) {
            Debug.LogWarning("Missing CSV file");
            return;
        }
        if (blockTypesHolder == null || voxelMaterialSet == null) {
            Debug.LogWarning($"no mat or type holder");
            return;
        }
        string[] lines = blockTypesCSV.text.Split("\n");
        int reqNumCols = 10;
        if (lines.Length <= 2 || !lines[0].Contains(",") || lines[0].Split(",").Length < reqNumCols) {
            Debug.LogWarning($"Invalid csv {blockTypesCSV.name}");
            return;
        }
        if (lines[0].Split(",")[0] != "Block Name") {
            Debug.LogWarning($"Invalid CSV {blockTypesCSV.name}");
            return;
        }
        List<BlockTypeMaker> blockTypeMakers = new List<BlockTypeMaker>();
        // line 0 is headers
        // get default
        string[] defcols = lines[1].Split(",");
        int defccol = 0;
        // 0 name
        defccol++;
        // 1 id
        defccol++;
        var defIsInvisible = ParseBool(defcols[defccol++], false);
        var defIsTransparent = ParseBool(defcols[defccol++], false);
        var defCol = ParseBool(defcols[defccol++], true);
        int defHardness = ParseInt(defcols[defccol++], 0);
        int defDens = ParseInt(defcols[defccol++], 0);
        int defFlam = ParseInt(defcols[defccol++], 0);
        int defBurntoid = ParseInt(defcols[defccol++], 0);
        int defDissolv = ParseInt(defcols[defccol++], 0);
        for (int i = 2; i < lines.Length; i++) {
            if (lines[i] == "") continue; // ignore empty lines
            string[] cols = lines[i].Split(",");
            if (cols.Length == 0 || cols[0] == "") {
                blockTypeMakers.Add(null);
                continue; // ignore if no name
            }
            int ccol = 0;
            BlockTypeMaker nblock = new BlockTypeMaker();
            nblock.displayName = cols[ccol++];
            // Debug.Log("Adding ");
            int.TryParse(cols[ccol++], out var blockid);
            nblock.vmat = new TypeSelector<VoxelMaterial>(new TexturedMaterial() {
                isInvisible = ParseBool(cols[ccol++], defIsInvisible),
                isTransparent = ParseBool(cols[ccol++], defIsTransparent),
                hasCollision = ParseBool(cols[ccol++], defCol),
                textureCoord = new Vector2(blockid - 1, 0f) * voxelMaterialSet.textureScale,
            });
            // todo
            // nblock.customDatas
            blockTypeMakers.Add(nblock);
        }
        Debug.Log($"Parsed csv {blockTypesCSV.name}, {blockTypeMakers.Count} lines, now setting data");

#if UNITY_EDITOR
        UnityEditor.Undo.RecordObjects(new Object[] { voxelMaterialSet, blockTypesHolder }, "load block types");
#endif
        HardClearAllBlockTypesAndMats();
        foreach (var btm in blockTypeMakers) {
            CreateBlockTypeAndMat(btm);
        }
        Debug.Log("Finished loading from csv");
        // todo also update map sblocks and mapdata?
    }

    //     [ContextMenu("Import block conversion CSV")]
    //     public void ImportBlockConversionCSV() {
    //         if (blockTypesCSV == null) {
    //             Debug.LogWarning("Missing CSV file");
    //             return;
    //         }
    //         if (mapData == null) {
    //             Debug.LogWarning($"no map");
    //             return;
    //         }
    //         string[] lines = blockTypesCSV.text.Split("\n");
    //         int reqNumCols = 3;
    //         if (lines.Length <= 1 || !lines[0].Contains(",") || lines[0].Split(",").Length < reqNumCols) {
    //             Debug.LogWarning($"Invalid csv");
    //             return;
    //         }
    //         if (lines[0].Split(",")[0] != "Source Id") {
    //             Debug.LogWarning($"Invalid CSV {blockTypesCSV.name}");
    //             return;
    //         }
    //         List<BlockLoadConverter> blockLoadConverters = new List<BlockLoadConverter>();
    //         foreach (var line in lines) {
    //             string[] cols = line.Split(",");
    //             BlockLoadConverter blc = new BlockLoadConverter();
    //             if (TryParseInt(cols[0], out var ival)) {
    //                 blc.importMatId = ival;
    //             } else {
    //                 continue;
    //             }
    //             if (TryParseInt(cols[1], out ival)) {
    //                 blc.blockType = new BlockTypeRef().SetBlockId(ival);
    //             } else {
    //                 continue;
    //             }
    //             if (System.Enum.TryParse<SpecialBlockType>(cols[2].Trim().ToUpper().Replace(" ", "_"), out var eval)) {
    //                 blc.specialType = eval;
    //             } else {
    //                 blc.specialType = SpecialBlockType.NONE;
    //             }
    //             blockLoadConverters.Add(blc);
    //         }
    // #if UNITY_EDITOR
    //         UnityEditor.Undo.RecordObject(mapData, "new converter values");
    // #endif
    //         Debug.Log($"Got block load converters {blockLoadConverters.Count} length, now setting");
    //         mapData.ClearMapData();
    //         mapData.allBlocksConverter.blockTypeConverter = blockLoadConverters.ToArray();
    //         Debug.Log($"Set block types, now prepocessing map");
    //         mapData.ProcessImportData();
    //     }

    static int ParseInt(string col, int defVal) {
        if (int.TryParse(col.Trim(), out var intval)) {
            return intval;
        }
        return defVal;
    }
    static bool ParseBool(string col, bool defVal) {
        if (bool.TryParse(col.Trim(), out var boolval)) {
            return boolval;
        }
        if (col == "y") {
            return true;
        }
        if (col == "n") {
            return false;
        }
        return defVal;
    }
    static bool TryParseBool(string col, out bool val) {
        if (col == "y") {
            val = true;
            return true;
        } else if (col == "n") {
            val = false;
            return true;
        }
        val = false;
        return false;
    }
    static bool TryParseInt(string col, out int intval) {
        return int.TryParse(col.Trim(), out intval);
    }
}