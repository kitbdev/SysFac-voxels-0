using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace VoxelSystem.Importer.OgtVox {

    public class OgtVoxImporter {
        // Content Flags for ogt_vox_matl values for a given material
        // static int k_ogt_vox_matl_have_metal = 1 << 0;
        // static int k_ogt_vox_matl_have_rough = 1 << 1;
        // static int k_ogt_vox_matl_have_spec = 1 << 2;
        // static int k_ogt_vox_matl_have_ior = 1 << 3;
        // static int k_ogt_vox_matl_have_att = 1 << 4;
        // static int k_ogt_vox_matl_have_flux = 1 << 5;
        // static int k_ogt_vox_matl_have_emit = 1 << 6;
        // static int k_ogt_vox_matl_have_ldr = 1 << 7;
        // static int k_ogt_vox_matl_have_trans = 1 << 8;
        // static int k_ogt_vox_matl_have_alpha = 1 << 9;
        // static int k_ogt_vox_matl_have_d = 1 << 10;
        // static int k_ogt_vox_matl_have_sp = 1 << 11;
        // static int k_ogt_vox_matl_have_g = 1 << 12;
        // static int k_ogt_vox_matl_have_media = 1 << 13;

        // color
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        [System.Serializable]
        public class ogt_vox_rgba {
            public byte r, g, b, a;            // red, green, blue and alpha components of a color.
        }

        // column-major 4x4 matrix
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        [System.Serializable]
        public class ogt_vox_transform {
            public float m00, m01, m02, m03;   // column 0 of 4x4 matrix, 1st three elements = x axis vector, last element always 0.0
            public float m10, m11, m12, m13;   // column 1 of 4x4 matrix, 1st three elements = y axis vector, last element always 0.0
            public float m20, m21, m22, m23;   // column 2 of 4x4 matrix, 1st three elements = z axis vector, last element always 0.0
            public float m30, m31, m32, m33;   // column 3 of 4x4 matrix. 1st three elements = translation vector, last element always 1.0
        }

        // a palette of colors
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        [System.Serializable]
        public class ogt_vox_palette {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256, ArraySubType = UnmanagedType.Struct)]
            public ogt_vox_rgba[] color;// = new ogt_vox_rgba[256];      // palette of colors. use the voxel indices to lookup color from the palette.
        }

        // Extended Material Chunk MATL types
        public enum ogt_matl_type {
            ogt_matl_type_diffuse = 0, // diffuse is default
            ogt_matl_type_metal = 1,
            ogt_matl_type_glass = 2,
            ogt_matl_type_emit = 3,
            ogt_matl_type_blend = 4,
            ogt_matl_type_media = 5,
        };

        // Extended Material Chunk MATL information
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        [System.Serializable]
        public class ogt_vox_matl {
            public uint content_flags; // set of k_ogt_vox_matl_* OR together to denote contents available
            // [MarshalAs(UnmanagedType)]
            public ogt_matl_type type;
            public float metal;
            public float rough;
            public float spec;
            public float ior;
            public float att;
            public float flux;
            public float emit;
            public float ldr;
            public float trans;
            public float alpha;
            public float d;
            public float sp;
            public float g;
            public float media;

        }

        // Extended Material Chunk MATL array of materials
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        [System.Serializable]
        public class ogt_vox_matl_array {
            // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct)]
            public ogt_vox_matl[] matl;// = new ogt_vox_matl[256];      // extended material information from Material Chunk MATL
        }

        // a 3-dimensional model of voxels
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        [System.Serializable]
        public class ogt_vox_model {
            public uint size_x;        // number of voxels in the local x dimension
            public uint size_y;        // number of voxels in the local y dimension
            public uint size_z;        // number of voxels in the local z dimension
            public uint voxel_hash;    // hash of the content of the grid.
            [MarshalAs(UnmanagedType.ByValArray)]
            public char[] voxel_data;    // grid of voxel data comprising color indices in x -> y -> z order. a color index of 0 means empty, all other indices mean solid and can be used to index the scene's palette to obtain the color for the voxel.
            // [MarshalAs(UnmanagedType.I1)]
            // System.UIntPtr voxel_data;    // grid of voxel data comprising color indices in x -> y -> z order. a color index of 0 means empty, all other indices mean solid and can be used to index the scene's palette to obtain the color for the voxel.
        }

        // an instance of a model within the scene
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        [System.Serializable]
        public class ogt_vox_instance {
            [MarshalAs(UnmanagedType.LPTStr)]
            public string name;         // name of the instance if there is one, will be NULL otherwise.
            [MarshalAs(UnmanagedType.Struct)]
            public ogt_vox_transform transform;    // orientation and position of this instance within the scene. This is relative to its group local transform if group_index is not 0
            public uint model_index;  // index of the model used by this instance. used to lookup the model in the scene's models[] array.
            public uint layer_index;  // index of the layer used by this instance. used to lookup the layer in the scene's layers[] array.
            public uint group_index;  // this will be the index of the group in the scene's groups[] array. If group is zero it will be the scene root group and the instance transform will be a world-space transform, otherwise the transform is relative to the group.
            public bool hidden;       // whether this instance is individually hidden or not. Note: the instance can also be hidden when its layer is hidden, or if it belongs to a group that is hidden.
        }

        // describes a layer within the scene
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        [System.Serializable]
        public class ogt_vox_layer {
            [MarshalAs(UnmanagedType.LPTStr)]
            public string name;               // name of this layer if there is one, will be NULL otherwise.
            public bool hidden;             // whether this layer is hidden or not.
        }

        // describes a group within the scene
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        [System.Serializable]
        public class ogt_vox_group {
            [MarshalAs(UnmanagedType.Struct)]
            public ogt_vox_transform transform;            // transform of this group relative to its parent group (if any), otherwise this will be relative to world-space.
            public uint parent_group_index;   // if this group is parented to another group, this will be the index of its parent in the scene's groups[] array, otherwise this group will be the scene root group and this value will be k_invalid_group_index
            public uint layer_index;          // which layer this group belongs to. used to lookup the layer in the scene's layers[] array.
            public bool hidden;               // whether this group is hidden or not.
        }

        // the scene parsed from a .vox file.
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        [System.Serializable]
        public class ogt_vox_scene {
            public uint num_models;     // number of models within the scene.
            public uint num_instances;  // number of instances in the scene
            public uint num_layers;     // number of layers in the scene
            public uint num_groups;     // number of groups in the scene
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct)]
            public ogt_vox_model[] models;         // array of models. size is num_models
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct)]
            public ogt_vox_instance[] instances;      // array of instances. size is num_instances
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct)]
            public ogt_vox_layer[] layers;         // array of layers. size is num_layers
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct)]
            public ogt_vox_group[] groups;         // array of groups. size is num_groups
            [MarshalAs(UnmanagedType.Struct)]
            public ogt_vox_palette palette;        // the palette for this scene
            [MarshalAs(UnmanagedType.Struct)]
            ogt_vox_matl_array materials;      // the extended materials for this scene
            // todo fix materials
        }


        // creates a scene from a vox file within a memory buffer of a given size.
        // you can destroy the input buffer once you have the scene as this function will allocate separate memory for the scene objecvt.
        [DllImport("ogt_vox_clr.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        static extern ogt_vox_scene ogt_vox_read_scene(byte[] buffer, uint buffer_size);

        // // just like ogt_vox_read_scene, but you can additionally pass a union of k_read_scene_flags
        // [DllImport("ogt_vox_clr.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        // static extern ogt_vox_scene ogt_vox_read_scene_with_flags(byte[] buffer, byte buffer_size, byte read_flags);

        // destroys a scene object to release its memory.
        [DllImport("ogt_vox_clr.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        static extern void ogt_vox_destroy_scene(ogt_vox_scene scene);

        // // writes the scene to a new buffer and returns the buffer size. free the buffer with ogt_vox_free
        // [DllImport("ogt_vox_clr.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        // static extern byte[] ogt_vox_write_scene(ogt_vox_scene scene, byte[] buffer_size);

        // // merges the specified scenes together to create a bigger scene. Merged scene can be destroyed using ogt_vox_destroy_scene
        // // If you require specific colors in the merged scene palette, provide up to and including 255 of them via required_colors/required_color_count.
        // [DllImport("ogt_vox_clr.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        // static extern ogt_vox_scene ogt_vox_merge_scenes(ogt_vox_scene[] scenes, byte scene_count, ogt_vox_rgba[] required_colors, byte required_color_count);


        static bool debug = true;

        public static ogt_vox_scene GetScene(VoxelImportSettings importSettings) {
            Kutil.SaveSystem.StartLoad()
                       .InCustomFullPath(importSettings.filepath)
                       .As(Kutil.SaveSystem.SaveBuilder.SerializeType.BINARY)
                       .TryLoadBin(out var allbytes);
            if (debug) Debug.Log($"Loaded vox file {importSettings?.filepath} {allbytes?.Length}");
            ogt_vox_scene scene = ogt_vox_read_scene(allbytes, (uint)allbytes.Length);
            if (debug) Debug.Log($"Scene {scene}");
            // FullVoxelImportData fullVoxelImportData = LoadScene(scene, importSettings);
            // ogt_vox_destroy_scene(scene);
            if (debug) Debug.Log($"Loaded vox file {importSettings.filepath} successfully");
            // return loader.fullVoxelImportData;
            return scene;
        }
        // public static FullVoxelImportData Load(VoxelImportSettings importSettings) {
        //     Kutil.SaveSystem.StartLoad()
        //                .InCustomFullPath(importSettings.filepath)
        //                .As(Kutil.SaveSystem.SaveBuilder.SerializeType.BINARY)
        //                .TryLoadBin(out var allbytes);
        //     if (debug) Debug.Log($"Loaded vox file {importSettings?.filepath} {allbytes?.Length}");
        //     ogt_vox_scene scene = ogt_vox_read_scene(allbytes, (uint)allbytes.Length);
        //     if (debug) Debug.Log($"Scene {scene}");
        //     FullVoxelImportData fullVoxelImportData = LoadScene(scene, importSettings);
        //     // ogt_vox_destroy_scene(scene);
        //     if (debug) Debug.Log($"Loaded vox file {importSettings.filepath} successfully");
        //     // return loader.fullVoxelImportData;
        //     return default;
        // }
        static FullVoxelImportData LoadScene(ogt_vox_scene scene, VoxelImportSettings importSettings) {
            FullVoxelImportData fullVoxelImportData = new FullVoxelImportData();
            fullVoxelImportData.chunkResolution = importSettings.chunkResolution;
            fullVoxelImportData.voxelSize = importSettings.voxelSize;
            fullVoxelImportData.models = new VoxelModelImportData[0];
            List<VoxelModelImportData> voxelRoomModelImportDatas = fullVoxelImportData.models.ToList();
            foreach (var model in scene.models) {
                VoxelModelImportData voxelModelImportData = LoadModel(model, importSettings.chunkResolution);
                if (voxelModelImportData != null) {
                    voxelRoomModelImportDatas.Add(voxelModelImportData);
                }
            }
            fullVoxelImportData.models = voxelRoomModelImportDatas.ToArray();
            return fullVoxelImportData;
        }
        static VoxelModelImportData LoadModel(ogt_vox_model model, int chunkRes) {
            Vector3Int modelSize = new Vector3Int((int)model.size_x, (int)model.size_y, (int)model.size_z);
            if (debug) Debug.Log($"LoadModel size{modelSize}");
            // get voxels
            // ImportedVoxel[] voxels = new ImportedVoxel[sizeX * sizeY * sizeZ];
            Vector3Int numChunksPerDir = Vector3Int.one + (modelSize - Vector3Int.one) / chunkRes;
            int numChunksTotal = numChunksPerDir.x * numChunksPerDir.y * numChunksPerDir.z;
            ChunkImportData[] chunks = new ChunkImportData[numChunksTotal];
            for (int y = 0, rci = 0; y < numChunksPerDir.y; y++) {
                for (int z = 0; z < numChunksPerDir.z; z++) {
                    for (int x = 0; x < numChunksPerDir.x; x++, rci++) {
                        // for (int rci = 0; rci < chunks.Length; rci++) {
                        chunks[rci] = new ChunkImportData() {
                            voxels = new ImportedVoxel[chunkRes * chunkRes * chunkRes],
                            chunkPos = new Vector3Int(x, y, z),
                        };
                    }
                }
            }
            if (debug) Debug.Log($"model size {modelSize} ncpd{numChunksPerDir} numchunkst{numChunksTotal} chunkres{chunkRes}");
            for (int x = 0; x < model.size_x; x++) {
                for (int y = 0; y < model.size_y; y++) {
                    for (int z = 0; z < model.size_z; z++) {
                        Vector3Int voxelpos = new Vector3Int(x, y, z);
                        Vector3Int chunkpos = VoxelWorld.ChunkPosWithBlock(voxelpos, chunkRes);
                        Vector3Int localpos = VoxelWorld.BlockPosToLocalVoxelPos(voxelpos, chunkpos, chunkRes);
                        int chunkIndex =
                            // (chunkpos.y / numChunksPerDir.y) * chunkRes * chunkRes +
                            // (chunkpos.z / numChunksPerDir.z) * chunkRes +
                            // (chunkpos.x / numChunksPerDir.x);
                            chunks.ToList().FindIndex(c => c.chunkPos == chunkpos);
                        if (chunkIndex == -1) {
                            Debug.LogError($"Vox importer failed to get chunk index {chunkIndex} from cp:{chunkpos} ncpd:{numChunksPerDir} vp{voxelpos} lp{localpos} chunkres{chunkRes}");
                            return default;
                        }
                        ChunkImportData chunkData = chunks[chunkIndex];
                        //({chunkData.chunkPos})
                        // chunkData.chunkPos = chunkpos;
                        long data_index = x * model.size_z * model.size_y + y * model.size_z + z;
                        int matid = (int)model.voxel_data[data_index];
                        // if (matid != 0) {
                        // Debug.Log($"found {chunkpos} {voxelpos} is {matid} ({data_index})");
                        // }
                        // int index = x + y * chunkRes + z * chunkRes * chunkRes;
                        // voxels[index] = new ImportedVoxel() { materialId = (int)data[x, y, z] };
                        chunkData.voxels[VoxelChunk.IndexAt(localpos, chunkRes)] = new ImportedVoxel() {
                            materialId = matid,
                            // materialColor = color
                        };
                    }
                }
            }
            var modelImportData = new VoxelModelImportData() {
                // id = 
                modelSize = modelSize,
                // offset = latestPos,
                numChunksByAxis = numChunksPerDir,
                chunks = chunks
            };
            return modelImportData;
            // List<VoxelModelImportData> voxelRoomModelImportDatas = fullVoxelImportData.rooms.ToList();
            // voxelRoomModelImportDatas.Add(roomModelImportData);
            // fullVoxelImportData.rooms = voxelRoomModelImportDatas.ToArray();
        }
    }
}