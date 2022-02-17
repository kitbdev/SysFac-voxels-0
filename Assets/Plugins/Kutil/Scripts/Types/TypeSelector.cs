using UnityEngine;
using System;

namespace Kutil {
    /// <summary>
    /// Choose an inherited class and configure it
    /// </summary>
    /// <typeparam name="T">base type</typeparam>
    [Serializable]
    public class TypeSelector<T> {
        [UnityEngine.Serialization.FormerlySerializedAs("type")]
        private TypeChoice<T> _type;
        [SerializeReference]
        public T obj;

        public TypeChoice<T> type {
            get => _type; set {
                _type = value;
                Type selType = type.SelectedType;
                if (selType != null && (obj == null || obj.GetType() != selType)) {
                    // todo? try to keep parts from old type? would need reflaction
                    type.TryCreateInstance(out obj);
                }
            }
        }

    }

}