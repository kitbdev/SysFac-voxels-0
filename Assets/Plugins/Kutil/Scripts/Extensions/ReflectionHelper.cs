using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Kutil {
    public static class ReflectionHelper {

        public static BindingFlags defFlags = BindingFlags.Public
                                                    | BindingFlags.NonPublic
                                                    | BindingFlags.Instance
                                                    | BindingFlags.Static;
        /// <summary>
        /// Attempts to get a value on an object at a given path (supports nesting)
        /// </summary>
        /// <param name="target">object the value is on</param>
        /// <param name="memberPath">path of the property (nesting is '.' delimited)</param>
        /// <param name="value">output. gives the wanted value</param>
        /// <typeparam name="T">type of wanted value</typeparam>
        /// <returns>true if successful</returns>
        public static bool TryGetValue<T>(System.Object target, string memberPath, out T value) {
            return TryGetValue<T>(target, memberPath, defFlags, out value);
        }
        /// <summary>
        /// Attempts to get a value on an object at a given path (supports nesting)
        /// </summary>
        /// <param name="target">object the value is on</param>
        /// <param name="memberPath">path of the property (nesting is '.' delimited)</param>
        /// <param name="flags">custom binding flags </param>
        /// <param name="value">output. gives the wanted value</param>
        /// <typeparam name="T">type of wanted value</typeparam>
        /// <returns>true if successful</returns>
        public static bool TryGetValue<T>(System.Object target, string memberPath, BindingFlags flags, out T value) {
            if (target == null) {
                value = default;
                return false;
            }
            if (memberPath.Contains('.')) {
                // nested
                string[] splitpath = memberPath.Split('.', 2);
                // get child target and try again on that
                // Debug.LogWarning("TryGetValue path " + memberPath);
                const string arrayStr = "Array";
                if (splitpath[1].Contains(arrayStr) && splitpath[1].Split('.', 2)[0].Equals(arrayStr)) {
                    // this element is an enumerable
                    var newpath = splitpath[1].Split('.', 2)[1];
                    bool gotarray = TryGetValue<IEnumerable<System.Object>>(target, splitpath[0], defFlags, out var ntargets);
                    if (!gotarray) {
                        Debug.LogWarning($"TryGetValue Failed to get enumerable {memberPath} on {target}");
                        value = default; return false;
                    }
                    // "data[" = 5 characters
                    newpath = newpath.Remove(0, 5);
                    var t = newpath.Split(']', 2);
                    int arrayIndex = 0;
                    if (!int.TryParse(t[0], out arrayIndex)) {
                        Debug.LogWarning($"TryGetValue Failed to parse path {memberPath} on {target}");
                        value = default; return false;
                    }
                    newpath = t[1];
                    if (newpath.Contains('.')) {
                        // no more path inside of array
                        newpath = newpath.Remove(0, 1);
                    } else {
                        Debug.LogWarning("TryGetValue array base member check");
                        // value = default; return false;
                    }
                    var ntarget = ntargets.ToArray()[arrayIndex];
                    return TryGetValue<T>(ntarget, newpath, flags, out value);
                } else {
                    // get nested object
                    TryGetValue<System.Object>(target, splitpath[0], defFlags, out var ntarget);
                    return TryGetValue<T>(ntarget, splitpath[1], flags, out value);
                }
            } else {
                MemberInfo memberInfo = GetMemberInfo(target.GetType(), memberPath, flags);
                if (memberInfo == null) {
                    value = default;
                    return false;
                }
                // Debug.Log($"found member {memberInfo} {target}");
                return TryGetValue<T>(target, memberInfo, out value);
            }
        }
        /// <summary>
        /// Gets a value on an object given the MemberInfo
        /// </summary>
        /// <param name="target"></param>
        /// <param name="memberInfo"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns>true if successful</returns>
        public static bool TryGetValue<T>(System.Object target, MemberInfo memberInfo, out T value) {
            value = default;
            if (memberInfo is FieldInfo) {
                FieldInfo fieldInfo = memberInfo as FieldInfo;
                var targetObj = fieldInfo.IsStatic ? null : target;
                value = (T)fieldInfo.GetValue(targetObj);
                return true;
            } else if (memberInfo is PropertyInfo) {
                PropertyInfo propertyInfo = memberInfo as PropertyInfo;
                value = (T)propertyInfo.GetValue(target);
                return true;
            } else if (memberInfo is MethodInfo) {
                MethodInfo methodInfo = memberInfo as MethodInfo;
                var targetObj = methodInfo.IsStatic ? null : target;
                value = (T)methodInfo.Invoke(targetObj, new object[0]);
                return true;
            } else {
                Debug.LogWarning($"Failed to find valid member info on '{target}' {memberInfo}");
                return false;
            }
        }
        public static MemberInfo GetMemberInfo(Type objectType, string fname, Type matchType = null) {
            return GetMemberInfo(objectType, fname, defFlags, matchType);
        }
        public static MemberInfo GetMemberInfo(Type objectType, string fname, BindingFlags flags, Type matchType = null) {
            while (objectType != null && objectType != typeof(object)) {
                MemberInfo[] memberInfos = objectType.GetMember(fname, flags);
                if (memberInfos.Length > 0) {
                    MemberInfo memberInfo = memberInfos[0];
                    // Debug.Log("type is " + memberInfo.MemberType.ToString());
                    switch (memberInfo.MemberType) {
                        case MemberTypes.Field:
                            FieldInfo fieldInfo = objectType.GetField(fname, flags);
                            if (matchType != null && fieldInfo.FieldType != matchType) {
                                Debug.LogWarning($"GetValidFieldInfo Type Mismatch: expected:{matchType} found:{fieldInfo.FieldType}");
                            } else {
                                return fieldInfo;
                            }
                            break;
                        case MemberTypes.Property:
                            PropertyInfo propertyInfo = objectType.GetProperty(fname, flags);
                            if (matchType != null && propertyInfo.PropertyType != matchType) {
                                Debug.LogWarning($"GetMemberInfo Type Mismatch: expected:{matchType} found:{propertyInfo.PropertyType}");
                            } else {
                                return propertyInfo;
                            }

                            break;
                        case MemberTypes.Method:
                            MethodInfo methodInfo = objectType.GetMethod(fname, flags);
                            if (methodInfo.GetParameters().Length > 0) {
                                continue;
                            }
                            if (matchType != null && methodInfo.ReturnType != matchType) {
                                Debug.LogWarning($"GetMemberInfo Type Mismatch: expected:{matchType} found:{methodInfo.ReturnType}");
                            } else {
                                return methodInfo;
                            }
                            break;
                        default:
                            break;
                    }
                }
                objectType = objectType.BaseType;
            }
            return null;
        }
    }
}