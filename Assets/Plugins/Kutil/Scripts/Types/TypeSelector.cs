using UnityEngine;
using System;

namespace Kutil {
    /// <summary>
    /// Choose an inherited class and configure it
    /// </summary>
    /// <typeparam name="T">base type</typeparam>
    [Serializable]
    public class TypeSelector<T> : ISerializationCallbackReceiver {

        [SerializeField]
        internal TypeChoice<T> _type;

        [SerializeField]
        [SerializeReference]
        [ContextMenuItem("Update Object", nameof(UpdateObjectType))]
        internal T _obj;

        public TypeChoice<T> type {
            get => _type; set {
                _type = value;
                UpdateObjectType();
            }
        }
        public T obj {
            get => _obj;
            set => _obj = value;
            // protected set => _obj = value;
        }


        public TypeSelector() { }
        public TypeSelector(T objectData) {
            this._type = objectData.GetType();
            this.obj = objectData;
        }
        public TypeSelector(TypeChoice<T> type) {
            this.type = type;
        }

        private void UpdateObjectType() {
            type.UpdateTypeList();
            Type selType = type.selectedType;
            if (selType != null && (obj == null || obj.GetType() != selType)) {
                // todo? try to keep parts from old type? would need reflection
                type.TryCreateInstance(out _obj);
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