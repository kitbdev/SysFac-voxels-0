using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kutil {
    public static class Extentions {
    }
    public static class EnumerableExtensions {
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> enumerable, Func<T, TKey> keySelector) {
            return enumerable.GroupBy(keySelector).Select(grp => grp.First());
        }
    }
    public static class QuaternionExt {
        public static Quaternion ClampRotation(this Quaternion q, Vector3 bounds) {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
            angleX = Mathf.Clamp(angleX, -bounds.x, bounds.x);
            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            float angleY = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.y);
            angleY = Mathf.Clamp(angleY, -bounds.y, bounds.y);
            q.y = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleY);

            float angleZ = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.z);
            angleZ = Mathf.Clamp(angleZ, -bounds.z, bounds.z);
            q.z = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleZ);

            return q.normalized;
        }
        public static Quaternion ClampRotationX(this Quaternion q, float minDeg, float maxDeg) {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
            angleX = Mathf.Clamp(angleX, minDeg, maxDeg);
            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q.normalized;
        }
    }
}