using UnityEngine;
using System;
using System.Collections;

namespace Kutil {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct
    |AttributeTargets.Field
         ,Inherited = true)]
    public class ShowAsChildAttribute : PropertyAttribute {
        public string childSourceField;
        // todo? multiple children?

        public ShowAsChildAttribute(string choicesListSourceField) {
            this.childSourceField = choicesListSourceField;
        }
    }
}