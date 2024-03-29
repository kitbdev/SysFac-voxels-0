using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kutil {
    [System.Serializable]
    public class Layer {
        public int layerValue;

        public Layer(int layer) {
            this.layerValue = layer;
        }
        public bool InLayerMask(int layermask) {
            return (layermask & (1 << layerValue)) > 0;
        }
        public LayerMask AddToMask(LayerMask mask) {
            return mask | layerValue;
        }
        public int GetMask() {
            return 1 << layerValue;
        }

        public void SetLayer(GameObject go) {
            go.layer = layerValue;
        }
        public void SetLayerAllChildren(GameObject go, LayerMask? ignoreLayers = null) {
            // ignore certain layers, like UI
            if (ignoreLayers != null && ((Layer)go.layer).InLayerMask((LayerMask)ignoreLayers)) {
                return;
            }
            go.layer = layerValue;
            int childCount = go.transform.childCount;
            for (int i = 0; i < childCount; i++) {
                SetLayerAllChildren(go.transform.GetChild(i).gameObject, ignoreLayers);
            }
        }

        public static Layer DefaultLayer => 0;
        public static Layer NameToLayer(string layerName) => LayerMask.NameToLayer(layerName);
        public static string LayerToName(Layer layer) => LayerMask.LayerToName(layer);

        public static implicit operator int(Layer l) => l.layerValue;
        public static implicit operator Layer(int l) => new Layer(l);

    }
}