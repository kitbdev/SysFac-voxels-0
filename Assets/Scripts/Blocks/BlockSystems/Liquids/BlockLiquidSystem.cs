using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelSystem;
using Kutil;

public class BlockLiquidSystem : MonoBehaviour {

}
public struct LiquidTypeData {
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
public struct TemperatureDataVD : VoxelData {
    public float temperature;
    public float frozenAmount;
    public float burningDur;
}
// public struct ToxicDataVD : VoxelData {
//     public float temperature;
// }