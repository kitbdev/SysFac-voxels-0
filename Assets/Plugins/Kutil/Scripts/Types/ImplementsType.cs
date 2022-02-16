// using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Kutil {
    [Serializable]
    public class ImplementsType<T> {
        [CustomDropDown(nameof(choices), 0)]
        [SerializeField] internal string _selectedName;
        public string selectedName { get => _selectedName; protected set => _selectedName = value; }

        public Type GetSelectedType() {
            IEnumerable<Type> choicesunsorted = GetAllAssignableTypes(typeof(T));
            int index = choicesunsorted.Select(t => t.Name).ToList().IndexOf(selectedName);
            if (index < 0) {
                // selectedname is not set
                return null;
            }
            return choicesunsorted.ElementAt(index);
        }
        public bool TryCreateInstance(out T instance) {
            Type selType = GetSelectedType();
            if (selType != null && !selType.IsAbstract) {
                instance = (T)Activator.CreateInstance(selType);
                return true;
            }
            instance = default;
            return false;
        }

        public int defaultIndex = 0;
        public Func<string, string> formatSelectedValueFunc = null;
        public Func<string, string> formatListFunc = null;

        public ImplementsType() {
            selectedName = choices[0];
        }

        public string[] choices => GetAllAssignableTypes(typeof(T))
                                    .Select(t => t.Name)
                                    // todo sort by hierarchy?
                                    // .OrderBy(s => s.Length > 0 ? s[0] : 0)// alphabetical
                                    // .OrderBy(s => s) 
                                    .ToArray();


        public static IEnumerable<Type> GetAllAssignableTypes(Type type) {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p));
        }
    }
}