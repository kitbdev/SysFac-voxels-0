using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelSystem;
using Kutil;

[System.Serializable]
public struct LiquidType {
    // viscosity
    public float flowSpeed;
    public float spread;
    public float entitySpeedMult;
    // mixing
    public float miscability;
    // movement
    public float permeability;// ?move 
    // elements etc
    public float baseElementalAttunement;//?
    public float baseAspects;//?
}
public class BlockLiquidSystem : MonoBehaviour {
    void InitBlock(Voxel voxel, VoxelChunk chunk, Vector3Int localPos){

    }
    void OnBlockTick(TickUpdater.TickUpdateArgs tickarg) {
        // Debug.Log($"update! {tickarg}");
    }
}
public struct IngredientTypeData {

}
public struct LiquidDataVD : VoxelData {
    public int liquidType;
    public struct Mixes {
        public int liquidType;
        public float amount;
    }
    public float toxicity;
    public Mixes mixes;
    public float elementAmounts;
    public int infusedAspects;
}
public struct FlamabilityDataVD : VoxelData {
    public int flamability;
    // burnto?
    public float burningDur;
}
public struct TemperatureDataVD : VoxelData {
    public float temperature;
    public float frozenAmount;
}
// public struct ToxicDataVD : VoxelData {
//     public float temperature;
// }