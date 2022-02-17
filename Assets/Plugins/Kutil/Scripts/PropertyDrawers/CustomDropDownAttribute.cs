using UnityEngine;
using System;
using System.Collections;

namespace Kutil {
    //todo? support class and struct targets (not just string) is is possible?
    //| AttributeTargets.Class | AttributeTargets.Struct,
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property,
         Inherited = true)]
    /// <summary>
    /// CustomDropDownAttribute Creates a enum like dropdown menu with custom options on a string
    /// </summary>
    public class CustomDropDownAttribute : PropertyAttribute {
        public string choicesListSourceField;
        public bool includeNullChoice = false;
        public bool includeEmptyChoice = false;
        public string formatSelectedValueFuncField = null;
        public string formatListFuncField = null;
        public string noElementsText = null;
        public string errorText = null;

        public CustomDropDownAttribute(string choicesListSourceField) {
            this.choicesListSourceField = choicesListSourceField;
        }
        /// <summary>
        /// CustomDropDownAttribute
        /// </summary>
        /// <param name="choicesListSourceField">nameof a string[]</param>
        /// <param name="noElementsText">optional string used if choices has no elements</param>
        /// <param name="errorText">optional string used if an error is encountered finding the choices</param>
        /// <param name="formatSelectedValueFunc">optional nameof a System.Func<string,string></param>
        /// <param name="formatListFunc">optional nameof a System.Func<string,string></param>
        public CustomDropDownAttribute(string choicesListSourceField,
            string formatSelectedValueFunc = null,
                // int defaultIndex = 0,
                string formatListFunc = null,
                string noElementsText = null,
                string errorText = null,
                bool includeNullChoice = false,
                bool includeEmptyChoice = false
            ) {
            this.choicesListSourceField = choicesListSourceField;
            this.formatSelectedValueFuncField = formatSelectedValueFunc;
            this.formatListFuncField = formatListFunc;
            this.errorText = errorText;
            this.noElementsText = noElementsText;
            this.includeNullChoice = includeNullChoice;            
            this.includeEmptyChoice = includeEmptyChoice;
        }
    }
}