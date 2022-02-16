// https://answers.unity.com/questions/460727/how-to-serialize-dictionary-with-unity-serializati.html

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kutil {
    /// <summary>
    /// note: must inherit like so:
	/// [System.Serializable] public class DictionaryStringVector2 : SerializableDictionary<string, Vector2> {}
    /// </summary>
    [System.Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver {
        [SerializeField, HideInInspector]
        private List<TKey> keys = new List<TKey>();

        [SerializeField]
        private List<TValue> values = new List<TValue>();

        // save the dictionary to lists
        public void OnBeforeSerialize() {
            keys.Clear();
            values.Clear();
            if (typeof(TKey).IsSubclassOf(typeof(UnityEngine.Object)) || typeof(TKey) == typeof(UnityEngine.Object)) {
                // avoid copying UnityEngine.Objects that have been destroyed in the event that they're used as a key
                foreach (var element in this.Where(element => element.Key != null)) {
                    keys.Add(element.Key);
                    values.Add(element.Value);
                }
            } else {
                foreach (var element in this) {
                    keys.Add(element.Key);
                    values.Add(element.Value);
                }
            }
        }

        // load dictionary from lists
        public void OnAfterDeserialize() {
            this.Clear();

            if (keys.Count != values.Count)
                throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));

            for (int i = 0; i < keys.Count; i++)
                this.Add(keys[i], values[i]);
        }
    }
}