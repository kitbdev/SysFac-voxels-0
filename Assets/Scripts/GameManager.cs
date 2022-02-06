// global using Kutil;
using Kutil;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager> {
    VoxelWorld _mainWorld;

    public VoxelWorld mainWorld { get => _mainWorld; private set => _mainWorld = value; }
}