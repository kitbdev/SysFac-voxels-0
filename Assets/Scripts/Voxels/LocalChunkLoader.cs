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
        // Debug.Log($"ccp:{world.ChunkposToWorldposCenter(Vector3.zero)} fp:{world.ChunkposToWorldpos(Vector3.zero)} ep:{world.ChunkposToWorldpos(Vector3.one)}");
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
        // todo currently using chunk the target is in
        foreach (var target in targets) {
            int ccheckDist = (int)(target.radius / world.chunkSize) + 1;
            Vector3Int centerChunk = world.WorldposToChunkpos(target.transform.position);
            Vector3 cchunkCenter = world.ChunkposToWorldposCenter(centerChunk);
            for (int y = -ccheckDist; y <= ccheckDist; y++) {
                for (int x = -ccheckDist; x <= ccheckDist; x++) {
                    for (int z = -ccheckDist; z <= ccheckDist; z++) {
                        Vector3Int checkChunk = centerChunk + new Vector3Int(x, y, z);
                        Vector3 nChunkCenter = world.ChunkposToWorldposCenter(checkChunk);
                        float chunkDist = Vector3.Distance(target.transform.position, nChunkCenter);
                        // float chunkDist = Vector3.Distance(cchunkCenter, nChunkCenter);
                        // todo chunk center check
                        if (chunkDist <= target.radius) {
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
    private void OnDrawGizmosSelected() {
        Gizmos.color = new Color(0.2f, 0.9f, 0.5f, 0.1f);
        foreach (var target in targets) {
            Gizmos.DrawSphere(target.transform.position, 0.1f);
            Gizmos.DrawSphere(target.transform.position, target.radius);
        }
        Gizmos.color = Color.white;
    }
}