using UnityEngine;
using System;

namespace Kutil {
    /// <summary>
    /// Holds a Type that implements or inherits a base type and an object of that type.
    /// Uses a TypeChoice for editor inspector type selection
    /// </summary>
    /// <typeparam name="T">base type</typeparam>
    [Serializable]
    public class TypeSelector<T> : ISerializationCallbackReceiver {

        [SerializeField]
        internal TypeChoice<T> _type = new TypeChoice<T>() {
            onlyIncludeConcreteTypes = true,
        };

        [SerializeField]
        [SerializeReference]
        [ContextMenuItem("Update Object", nameof(UpdateObjectType))]
        internal T _objvalue;

        public TypeChoice<T> type {
            get => _type; set {
                var old = _type;
                _type = value;
                UpdateObjectType();
            }
        }
        public T objvalue {
            get => _objvalue;
            set => _objvalue = value;
        }

        public TypeSelector() { 
            this._type = null;
            // inspector doesnt initialize values like this
            // this._type.onSelectCallback = (v) => { UpdateObjectType(); };
        }
        public TypeSelector(T objectData) {
            this._type = objectData.GetType();
            this.objvalue = objectData;
        }
        public TypeSelector(TypeChoice<T> type) {
            this.type = type;
        }

        private void UpdateObjectType() {
            Type selType = type.selectedType;
            if (selType != null && (objvalue == null || objvalue.GetType() != selType)) {
                // todo? try to keep parts from old type? would need reflection
                // Debug.Log($"Updating object type to {selType}");
                type.TryCreateInstance(out _objvalue);
            }
        }

        int ticker = 0;
        public void OnBeforeSerialize() {
#if UNITY_EDITOR
            // todo? auto update in drawer instead
            // very janky way to reduce number of calls in inspector
            // at least it will only update when viewed
            if (!Application.isPlaying) {
                ticker++;
                // Debug.Log(ticker);
                const int frametarget = 70;
                if (ticker >= frametarget) {
                    UpdateObjectType();
                    ticker = 0;
                    // Debug.Log($"o:{obj?.GetType().FullName} t:{type.selectedType} {type}");
                }
            }
#endif
        }
        public void OnAfterDeserialize() { }

    }

}