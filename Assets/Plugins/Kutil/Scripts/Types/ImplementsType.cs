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
        // public T SelectedType => choices.ToList().IndexOf(selectedName);

        public int defaultIndex = 0;
        public Func<string, string> formatSelectedValueFunc = null;
        public Func<string, string> formatListFunc = null;

        public string[] choices => GetAllAssignableTypes(typeof(T))
                                    .Select(t => t.Name)
                                    // .OrderBy(s => s.Length > 0 ? s[0] : 0)// alphabetical
                                    .OrderBy(s => s) 
                                    .ToArray();


        public static IEnumerable<Type> GetAllAssignableTypes(Type type) {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p));
        }
    }
}