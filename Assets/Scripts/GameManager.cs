// global using Kutil;
using Kutil;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelSystem;

[DefaultExecutionOrder(-5)]
public class GameManager : Singleton<GameManager> {
    
    public VoxelWorld _mainWorld;

    public VoxelWorld mainWorld { get => _mainWorld; private set => _mainWorld = value; }
    protected override void Awake() {
        base.Awake();
    }
}