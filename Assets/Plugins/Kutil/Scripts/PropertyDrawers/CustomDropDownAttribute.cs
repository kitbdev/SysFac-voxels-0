﻿using UnityEngine;
using System;
using System.Collections;

namespace Kutil {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property,
    //todo? support class and struct targets (not just string) is is possible?
    //| AttributeTargets.Class | AttributeTargets.Struct,
         Inherited = true)]
    public class CustomDropDownAttribute : PropertyAttribute {
        public string choicesListSourceField;
        public int defaultIndex = 0;
        public string formatSelectedValueFuncField = null;
        public string formatListFuncField = null;
        public string noElementsText = null;
        public string errorText = null;

        public CustomDropDownAttribute(string choicesListSourceField) {
            this.choicesListSourceField = choicesListSourceField;
        }
        public CustomDropDownAttribute(string choicesListSourceField, int defaultIndex = 0,
            string noElementsText = null,
            string missingText = null,
            string formatSelectedValueFunc = null,
            string formatListFunc = null
            ) {
            this.choicesListSourceField = choicesListSourceField;
            this.defaultIndex = defaultIndex;
            this.formatSelectedValueFuncField = formatSelectedValueFunc;
            this.formatListFuncField = formatListFunc;
        }
    }
}