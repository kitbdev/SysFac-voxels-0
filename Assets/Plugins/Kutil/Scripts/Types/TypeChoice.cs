// using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Kutil {
    /// <summary>
    /// Holds a Type that implements or inherits a base type. Basically a dynamic enum for types.
    /// </summary>
    /// <typeparam name="T">base type</typeparam>
    [Serializable]
    public class TypeChoice<T> {
        // alternate names: ImplementsType, childtype, typeholder, TypeSubclass, 
        // SubType, TypeMatcher, TypeInterface, TypeImplementer, TypeInheritChoice

        [CustomDropDown(nameof(choices),
            formatSelectedValueFunc: nameof(formatSelectedValueFunc), formatListFunc: nameof(formatListFunc),
            noElementsText: "No inherited or implemented types found!")]
        [SerializeField] internal string _selectedName;
        public string selectedName { get => _selectedName; protected set => _selectedName = value; }
        public bool onlyIncludeConcreteTypes = true;
        // todo? seperate no self option

        public Func<string, string> formatSelectedValueFunc = null;
        public Func<string, string> formatListFunc = s => {
            Type curType = GetTypeFor(s);
            if (curType != null && curType.BaseType != null) {
                return $"{s} : ({curType.BaseType.Name})";
            }
            return s;
        };


        [NonSerialized]
        protected IEnumerable<Type> choicesTypes = null;
        [NonSerialized]
        protected Type selTypeCache = null;

        public string[] choices {
            get {
                UpdateCache();
                // todo? sort by hierarchy? if not already
                return choicesTypes.Select(t => t.Name)
                    // .OrderBy(s => s.Length > 0 ? s[0] : 0) // alphabetical
                    // .OrderBy(s => s) 
                    .ToArray();
            }
        }

        public Type selectedType {
            get {
                if (selTypeCache != null && selectedName != null) {
                    return selTypeCache;
                }
                UpdateCache();
                int index = choicesTypes.Select(t => t.Name).ToList().IndexOf(selectedName);
                if (index < 0) {
                    // selectedname is not set
                    return null;
                }

                selTypeCache = choicesTypes.ElementAt(index);
                return selTypeCache;
            }
        }

        public TypeChoice() {
            selectedName = choices[0];
        }
        public TypeChoice(Type setType, Func<string, string> formatSelectedValueFunc = null, Func<string, string> formatListFunc = null) {
            SetType(setType);
            this.formatSelectedValueFunc = formatSelectedValueFunc;
            this.formatListFunc = formatListFunc;
        }

        public bool IsTypeValid(Type type) {
            // if (type.IsAssignableFrom(typeof(T))) {
            if (typeof(T).IsAssignableFrom(type)) {
                return true;
            }
            return false;
        }

        public bool SetType(Type type) {
            if (IsTypeValid(type)) {
                selectedName = type.Name;
                ClearCache();
                return true;
            }
            return false;
        }
        public void UpdateTypeList() {
            ClearCache();
        }
        private void ClearCache() {
            choicesTypes = null;
            selTypeCache = null;
        }
        protected void UpdateCache() {
            choicesTypes ??= GetAllAssignableTypes(typeof(T), onlyIncludeConcreteTypes);
        }


        public T CreateInstance() {
            Type selType = selectedType;
            if (selType != null && !selType.IsAbstract && !selType.IsInterface) {
                return (T)Activator.CreateInstance(selType);
            }
            Debug.LogError($"Cannot create {typeof(T).Name} instance of {selType}!");
            return default;
        }
        public bool TryCreateInstance(out T instance) {
            Type selType = selectedType;
            if (selType != null && !selType.IsAbstract) {
                instance = (T)Activator.CreateInstance(selType);
                return true;
            }
            instance = default;
            return false;
        }
        public override string ToString() {
            return $"TypeChoice<{typeof(T).Name}> {selectedName}";
        }

        public static IEnumerable<Type> GetAllAssignableTypes(Type type, bool onlyConcrete = true) {
            IEnumerable<Type> enumerable = AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(s => s.GetTypes())
                            .Where(p => type.IsAssignableFrom(p));
            if (onlyConcrete) {
                enumerable = enumerable.Where(p => !p.IsAbstract && !p.IsInterface);
            }
            return enumerable;
        }
        private static Type GetTypeFor(string typeName) {
            var choicesTypes = GetAllAssignableTypes(typeof(T));
            int index = choicesTypes.Select(t => t.Name).ToList().IndexOf(typeName);
            if (index >= 0) {
                return choicesTypes.ElementAt(index);
            }
            return null;
        }
        public static implicit operator TypeChoice<T>(Type type) => new TypeChoice<T>(type);
    }
}