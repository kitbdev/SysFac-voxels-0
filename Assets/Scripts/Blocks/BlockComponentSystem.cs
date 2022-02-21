using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kutil;

public class BlockComponentSystem : MonoBehaviour {

    public TickUpdater blockTick;
    public List<int> updateableBlockVDs = new List<int>();

    private void OnValidate() {
        blockTick ??= GetComponent<TickUpdater>();
    }
    // protected override void Awake() {
    void Awake() {
        // base.Awake();
        blockTick ??= GetComponent<TickUpdater>();
    }
    private void OnEnable() {
        blockTick.onTickUpdateEvent += TestBlockUpdate;
    }
    private void OnDisable() {
        blockTick.onTickUpdateEvent -= TestBlockUpdate;
    }
    void TestBlockUpdate(TickUpdater.TickUpdateArgs tickarg) {
        // Debug.Log($"update! {tickarg}");
    }
}