using UnityEngine;
using System;

namespace Kutil {
    /// <summary>
    /// Choose an inherited class and configure it. must be hooked up to onvalidate to work
    /// </summary>
    /// <typeparam name="T">base type</typeparam>
    [Serializable]
    public class TypeSelector<T> {
        public ImplementsType<T> type;
        [SerializeReference]
        public T obj;

        public void OnValidate() {
            // todo custom inspector to auto call this?
            Type selType = type.GetSelectedType();
            if (selType != null && (obj == null || obj.GetType() != selType)) {
                // todo try to keep parts from old type?
                type.TryCreateInstance(out obj);
            }
        }
    }

}