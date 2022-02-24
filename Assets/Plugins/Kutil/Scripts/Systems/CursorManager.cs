using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kutil {
    /// <summary>
    /// Manages the cursor lock and visible state. must be placed in scene
    /// </summary>
    public class CursorManager : MonoBehaviour {

        [SerializeField] InputActionReference unlockAction;
        [SerializeField] InputActionReference lockAction;
        [SerializeField] CursorLockMode lockMode = CursorLockMode.Locked;
        [SerializeField] bool hideWhenLocked = true;
        [SerializeField] bool lockOnAwake = true;
        [SerializeField] bool toggleOnPause = true;
        [SerializeField] bool toggleOnMKey = false;
        [ReadOnly] public bool isLocked = false;

        private void Awake() {
            if (lockOnAwake) {
                SetCursorLock(true);
            }
        }
        private void OnEnable() {
            if (unlockAction != null) {
                unlockAction.action.Enable();
                unlockAction.action.performed += c => SetCursorLock(false);
            }
            if (lockAction != null) {
                lockAction.action.Enable();
                lockAction.action.performed += c => SetCursorLock(true);
            }
            if (toggleOnPause) {
                // todo only if using keyboard and mouse
                PauseManager.Instance.pauseEvent.AddListener(UnlockCursor);
                PauseManager.Instance.unpauseEvent.AddListener(LockCursor);
            }
            // todo automatically detect focus?
        }
        private void OnDisable() {
            if (toggleOnPause) {
                PauseManager.Instance?.pauseEvent.RemoveListener(UnlockCursor);
                PauseManager.Instance?.unpauseEvent.RemoveListener(LockCursor);
            }
        }
        private void Update() {
            if (toggleOnMKey && (Keyboard.current?.mKey.wasPressedThisFrame ?? false)) {
                SetCursorLock(!isLocked);
            }
        }
        public void SetCursorLock(bool locked) {
            isLocked = locked;
            if (locked) {
                Cursor.lockState = lockMode;
                Cursor.visible = !hideWhenLocked;
            } else {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        public void UnlockCursor() {
            SetCursorLock(false);
        }
        public void LockCursor() {
            SetCursorLock(true);
        }
    }
}