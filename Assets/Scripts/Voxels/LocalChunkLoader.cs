using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kutil;
using System.Linq;

public class LocalChunkLoader : MonoBehaviour {

    [System.Serializable]
    public class FollowTargets {
        public Transform transform;
        [Min(0)]
        public float radius = 50;
    }

    public VoxelWorld world;
    public List<FollowTargets> targets = new List<FollowTargets>();

    private void Awake() {
        world ??= GetComponent<VoxelWorld>();
    }

    private void Update() {
        // todo not do this every frame
        // only when something happens
        CheckTargets();
    }

    public List<Vector3Int> validChunks = new List<Vector3Int>();
    public void CheckTargets() {
        if (targets == null || targets.Count == 0) {
            return;
        }
        validChunks.Clear();
        foreach (var target in targets) {
            int ccheckDist = (int)(target.radius / world.chunkSize) + 1;
            Vector3Int centerChunk = world.WorldposToChunkpos(target.transform.position);
            for (int y = -ccheckDist; y < ccheckDist; y++) {
                for (int x = -ccheckDist; x < ccheckDist; x++) {
                    for (int z = -ccheckDist; z < ccheckDist; z++) {
                        Vector3Int checkChunk = centerChunk + new Vector3Int(x, y, z);
                        float chunkDist = Vector3.Distance(target.transform.position, world.ChunkposToWorldposCenter(checkChunk));
                        // todo chunk center check
                        if (chunkDist < target.radius) {
                            // in range
                            validChunks.Add(checkChunk);
                        }
                    }
                }
            }
        }

        var loadChunks = validChunks.Except(world.activeChunksPos);
        var unloadChunks = world.activeChunksPos.Except(validChunks);
        world.LoadChunks(loadChunks.ToArray());
        world.UnloadChunks(unloadChunks.ToArray());
    }
}