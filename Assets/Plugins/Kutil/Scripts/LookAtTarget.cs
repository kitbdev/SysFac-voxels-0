using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kutil {
    public class LookAtTarget : MonoBehaviour {

        public Transform target;
        [SerializeField] bool useMainCamera = false;
        // todo follow world up option

        private void Awake() {
            if (useMainCamera) {
                target = Camera.main.transform;
            }
        }

        private void LateUpdate() {
            if (target) {
                //todo
            }
        }

    }
}