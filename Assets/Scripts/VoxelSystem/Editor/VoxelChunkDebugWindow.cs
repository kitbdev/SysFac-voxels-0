using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace VoxelSystem {
    public class VoxelChunkDebugWindow : EditorWindow {

        SerializedObject serializedObject;
        [SerializeField] VoxelWorld world;
        [SerializeField] VoxelChunk targetChunk;
        [SerializeField] bool clickToSelectVoxel = true;
        // [SerializeField] bool absorbClick = false;
        // [SerializeField] bool takekeyboard = true;
        // [Kutil.ReadOnly]
        [SerializeField] Vector3Int targetBlockPos = Vector3Int.zero;
        // [Kutil.ReadOnly]
        [SerializeField] Voxel targetVoxel;
        [SerializeField] LayerMask layerMask = Physics.DefaultRaycastLayers;
        [SerializeField] Label infoLabel;


        [MenuItem("VoxelSystem/VoxelChunkDebugWindow")]
        private static void ShowWindow() {
            var window = GetWindow<VoxelChunkDebugWindow>();
            window.titleContent = new GUIContent("VoxelChunkDebugWindow");
            window.Show();
        }

        private void OnEnable() {
            serializedObject = new SerializedObject(this);
            var container = new VisualElement();
            rootVisualElement.Add(container);

            ScrollView svcontainer = new ScrollView(ScrollViewMode.Vertical);
            container.Add(svcontainer);

            svcontainer.Add(new PropertyField(serializedObject.FindProperty(nameof(world))));
            svcontainer.Add(new PropertyField(serializedObject.FindProperty(nameof(layerMask))));
            svcontainer.Add(new PropertyField(serializedObject.FindProperty(nameof(clickToSelectVoxel))));
            // svcontainer.Add(new PropertyField(serializedObject.FindProperty(nameof(absorbClick))));
            PropertyField blockposfield = new PropertyField(serializedObject.FindProperty(nameof(targetBlockPos)));
            blockposfield.RegisterValueChangeCallback(_ => UpdateVoxelFromBlockPos());
            svcontainer.Add(blockposfield);
            Button refreshChunkbtn = new Button(() => {
                if (targetChunk != null) {
                    targetChunk.Refresh();
                    UpdateVoxelFromBlockPos();
                    Debug.Log($"Refreshed chunk {targetChunk.chunkPos}");
                }
            });
            refreshChunkbtn.text = "Refresh Chunk";
            svcontainer.Add(refreshChunkbtn);
            infoLabel = new Label("Info");
            svcontainer.Add(infoLabel);

            VisualElement navGroup = new VisualElement();
            navGroup.style.borderTopWidth = 10f;
            svcontainer.Add(navGroup);

            PropertyField voxelfield = new PropertyField(serializedObject.FindProperty(nameof(targetVoxel)));
            voxelfield.RegisterValueChangeCallback(c => {
                // todo not being called
                Debug.Log("Reiniting vox n");
                if (targetVoxel != null) {
                    Debug.Log("Reiniting vox");
                    targetVoxel.Initialize(targetChunk, VoxelWorld.BlockPosToLocalVoxelPos(targetBlockPos, targetChunk.chunkPos, world.chunkResolution));
                }
            });
            svcontainer.Add(voxelfield);

            Button navup = new Button(() => { targetBlockPos += Vector3Int.up; UpdateVoxelFromBlockPos(); });
            Button navdown = new Button(() => { targetBlockPos += Vector3Int.down; UpdateVoxelFromBlockPos(); });
            Button navleft = new Button(() => { targetBlockPos += Vector3Int.left; UpdateVoxelFromBlockPos(); });
            Button navright = new Button(() => { targetBlockPos += Vector3Int.right; UpdateVoxelFromBlockPos(); });
            Button navforward = new Button(() => { targetBlockPos += Vector3Int.forward; UpdateVoxelFromBlockPos(); });
            Button navback = new Button(() => { targetBlockPos += Vector3Int.back; UpdateVoxelFromBlockPos(); });
            navup.text = "up";
            navdown.text = "down";
            navleft.text = "left";
            navright.text = "right";
            navforward.text = "forw";
            navback.text = "back";
            VisualElement toprow = new VisualElement();
            VisualElement botrow = new VisualElement();
            toprow.style.flexDirection = FlexDirection.Row;
            toprow.style.flexWrap = new StyleEnum<Wrap>(Wrap.Wrap);
            botrow.style.flexDirection = FlexDirection.Row;
            botrow.style.flexWrap = new StyleEnum<Wrap>(Wrap.Wrap);
            navGroup.Add(toprow);
            navGroup.Add(botrow);
            toprow.Add(navup);
            toprow.Add(navforward);
            toprow.Add(navdown);
            botrow.Add(navleft);
            botrow.Add(navback);
            botrow.Add(navright);


            container.Bind(serializedObject);

            Selection.selectionChanged += OnSelChange;
            SceneView.duringSceneGui += OnScene;
        }
        private void OnDisable() {
            serializedObject = null;
            Selection.selectionChanged -= OnSelChange;
            SceneView.duringSceneGui -= OnScene;
        }
        void OnSelChange() {
            if (Selection.activeGameObject == null) return;
            if (Selection.activeGameObject.TryGetComponent<VoxelChunk>(out var chunk)) {
                targetChunk = chunk;
                world = chunk.world;
                // targetBlockPos = 
            }
        }
        void OnScene(SceneView sceneView) {
            // sceneView.
            Event e = Event.current;
            // if (e.type == EventType.KeyUp)
            if (clickToSelectVoxel && e.type == EventType.MouseDown && e.button == (int)MouseButton.LeftMouse) {
                Ray mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

                // Ray camRay = sceneView.camera.ScreenPointToRay(Event.current.mousePosition);
                if (Physics.Raycast(mouseRay, out var hit, 1000, layerMask, QueryTriggerInteraction.Ignore)) {
                    if (!world) {
                        if (hit.collider.gameObject.TryGetComponent<VoxelChunk>(out var chunk)) {
                            world = chunk.world;
                        }
                        if (!world) {
                            return;
                        }
                    }
                    // get block pos
                    targetBlockPos = world.WorldposToBlockpos(hit.point + mouseRay.direction * 0.001f);
                    // take mousedown?
                    // if (absorbClick) {
                    //     e.Use();
                    // }
                }
                // sceneView.Repaint();
            }
            if (e.type == EventType.Repaint) {
                if (!world) {
                    return;
                }

                Vector3 wpos = world.BlockposToWorldPos(targetBlockPos);
                // Handles.lineThickness = 0.04f;
                // draw contrast
                Handles.color = Color.white;
                Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                Handles.DrawWireCube(wpos, Vector3.one * (world.voxelSize + 0.001f));
                // draw over box xray
                Handles.color = new Color(0.1f, 0.1f, 0.2f, 0.7f);
                Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
                Handles.DrawWireCube(wpos, Vector3.one * (world.voxelSize + 0.05f));
                // draw on top of box
                Handles.color = Color.black;
                Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                Handles.DrawWireCube(wpos, Vector3.one * (world.voxelSize + 0.01f));
                // default
                Handles.color = Color.white;
                Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            }
        }
        void UpdateVoxelFromBlockPos() {
            serializedObject.Update();
            if (world == null) return;
            Vector3Int cpos = world.ChunkPosWithBlock(targetBlockPos);
            Focus();// to unselect any current fields we may be editing when targvoxel changes
            if (world.HasChunkActiveAt(cpos)) {
                targetChunk = world.GetChunkAt(cpos);
                targetVoxel = world.GetVoxelAt(targetBlockPos);
                infoLabel.text = $"Selected Voxel at {targetBlockPos} int chunk {cpos}";
            } else {
                targetVoxel = null;
                targetChunk = null;
                infoLabel.text = "world does not have voxel";
                // Debug.LogWarning($"Cant find block {targetBlockPos} in chunk {cpos}");
            }
            serializedObject.ApplyModifiedProperties();
            SceneView.RepaintAll();
        }
        void UpdateUI() {

        }
    }
}