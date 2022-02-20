using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Kutil {
    public interface IObjectPool<T> where T : class {
        int CountInactive { get; }

        void Clear();
        T Get();
        PooledObject<T> Get(out T v);
        void Release(T element);
    }
    public struct PooledObject<T> : IDisposable {
        public void Dispose() {
            throw new NotImplementedException();
        }
    }
    public class TypedObjectPool<T> {

        // IObjectPool objectPool;
        void Clear() {
            // new Lookup();
        }
        T Get() {
            return default;
        }
        PooledObject<V> Get<V>(out T v) {
            v = default;
            return default;
        }
        void Release(T element) {

        }

    }
}