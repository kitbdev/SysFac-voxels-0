// using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Kutil {
    /// <summary>
    /// Holds a Type that implements or inherits a base type.
    /// </summary>
    /// <typeparam name="T">base type</typeparam>
    [Serializable]
    public class TypeChoice<T> {
    // alternate names: ImplementsType, childtype, typeholder, TypeSubclass, 
    // SubType, TypeMatcher, TypeInterface, TypeImplementer, TypeInheritChoice
        // todo choice to include self?

        [CustomDropDown(nameof(choices), 0, formatSelectedValueFunc: nameof(formatSelectedValueFunc), formatListFunc: nameof(formatListFunc))]
        [SerializeField] internal string _selectedName;
        public string selectedName { get => _selectedName; protected set => _selectedName = value; }

        public int defaultIndex = 0;//? needed
        // todo do these functions work?
        public Func<string, string> formatSelectedValueFunc = null;
        public Func<string, string> formatListFunc = null;

        [NonSerialized]
        protected IEnumerable<Type> choicesTypes = null;

        public string[] choices {
            get {
                CheckCache();
                // todo sort by hierarchy?
                return choicesTypes.Select(t => t.Name)
                    // .OrderBy(s => s.Length > 0 ? s[0] : 0)// alphabetical
                    // .OrderBy(s => s) 
                    .ToArray();
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
                choicesTypes = null;
                return true;
            }
            return false;
        }

        public Type SelectedType {
            get {
                CheckCache();
                int index = choicesTypes.Select(t => t.Name).ToList().IndexOf(selectedName);
                if (index < 0) {
                    // selectedname is not set
                    return null;
                }
                return choicesTypes.ElementAt(index);
            }
        }

        public T CreateInstance() {
            Type selType = SelectedType;
            if (selType != null && !selType.IsAbstract) {
                return (T)Activator.CreateInstance(selType);
            }
            Debug.LogError($"Cannot create {typeof(T).Name} instance of {selType}!");
            return default;
        }
        public bool TryCreateInstance(out T instance) {
            Type selType = SelectedType;
            if (selType != null && !selType.IsAbstract) {
                instance = (T)Activator.CreateInstance(selType);
                return true;
            }
            instance = default;
            return false;
        }

        protected void CheckCache() {
            choicesTypes ??= GetAllAssignableTypes(typeof(T));
        }

        public static IEnumerable<Type> GetAllAssignableTypes(Type type) {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p));
        }
        public static implicit operator TypeChoice<T>(Type type) => new TypeChoice<T>(type);
    }
}