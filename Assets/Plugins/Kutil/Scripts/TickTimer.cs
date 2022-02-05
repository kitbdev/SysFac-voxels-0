using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Kutil {
    /// <summary>
    /// triggers an event on time
    /// </summary>
    public class TickTimer : MonoBehaviour, ISerializationCallbackReceiver {

        [System.Serializable]
        public class TimerData {
            public event EventHandler<TickEventArgs> tickEvent; // todo cant serialize this
            public bool useFrameCount = false;
            public bool useUnscaledTimeScale = false;
            public int timerDurMS = 0;
            public float timerDurS => timerDurMS * 0.001f;
            public int timerDurFrames => timerDurMS;

            public int frameCounter = 0;
            public float lastTriggerTime = 0f;
            public float nextTriggerTime => lastTriggerTime + timerDurS;

            public void ResetTimer() {
                frameCounter = 0;
                lastTriggerTime = useUnscaledTimeScale ? Time.unscaledTime : Time.time;
            }
            internal void TriggerEvent(object sender, TickEventArgs args) {
                tickEvent.Invoke(sender, args);
            }

            [System.Serializable]
            public class TickEventArgs : EventArgs {
                public float elapsedDuration = 0f;
                public override string ToString() {
                    return $"tick elapsed:{elapsedDuration}";
                }
            }
        }
        [SerializeField, ReadOnly] int[] timerIndexes;
        [SerializeField, ReadOnly] TimerData[] timerData;
        Dictionary<int, TimerData> timers = new Dictionary<int, TimerData>();
        public bool anyTimers => timers.Count > 0;

        private void Update() {
            foreach (var timer in timers.Values) {
                if (!timer.useUnscaledTimeScale && Time.timeScale == 0) {
                    // paused
                    continue;
                }
                timer.frameCounter++;
                float sTime = timer.useUnscaledTimeScale ? Time.unscaledTime : Time.time;
                bool trigger = timer.useFrameCount ?
                    timer.frameCounter >= timer.timerDurFrames
                    : sTime >= timer.nextTriggerTime;
                if (trigger) {
                    TimerData.TickEventArgs args = new TimerData.TickEventArgs() {
                        elapsedDuration = sTime - timer.lastTriggerTime,
                    };
                    timer.TriggerEvent(this, args);
                    timer.ResetTimer();
                }
            }
        }
        int MakeHash(int a, int b) {
            // cantor pairing func
            return (a + b + 1) * (a + b) / 2 + b;
        }
        int TimerIndex(int dur, bool frames, bool useUnscaled) {
            int v2 = (frames ? 1 : 0) + (useUnscaled ? 2 : 0);
            return MakeHash(dur, v2);
        }
        public static TickTimer Create() {
            GameObject go = new GameObject();
            var timer = go.AddComponent<TickTimer>();
            go.name = "Tick timer";
            return timer;
        }
        /// <summary>
        /// frame timer counts number of frames, is framerate dependent
        /// </summary>
        /// <param name="durationFrames"></param>
        /// <param name="useUnscaled"></param>
        /// <returns></returns>
        public void FrameTimer(System.EventHandler<TimerData.TickEventArgs> action, int durationFrames, bool useUnscaled = false) {
            GetTimerData(durationFrames, true, useUnscaled).tickEvent += action;
        }
        public void TimerSec(System.EventHandler<TimerData.TickEventArgs> action, float durationSec, bool useUnscaled = false) {
            Timer(action, Mathf.FloorToInt(durationSec * 1000), useUnscaled);
        }
        public void Timer(System.EventHandler<TimerData.TickEventArgs> action, int durationMs, bool useUnscaled = false) {
            GetTimerData(durationMs, false, useUnscaled).tickEvent += action;
        }
        TimerData GetTimerData(int dur, bool frames, bool useUnscaled) {
            if (dur <= 0) {
                Debug.LogWarning("Invalid timer duration " + dur);
                return null;
            }
            int id = TimerIndex(dur, frames, useUnscaled);
            if (timers.ContainsKey(id)) {
                return timers[id];
            }
            TimerData timerData = new TimerData() {
                useFrameCount = frames,
                timerDurMS = dur,
                useUnscaledTimeScale = useUnscaled,
            };
            timers.Add(id, timerData);
            return timerData;
        }
        public void RemoveFrameTimer(int durationFrames, bool useUnscaled) {
            RemoveTimer(durationFrames, true, useUnscaled);
        }
        public void RemoveTimer(int durationMs, bool useUnscaled) {
            RemoveTimer(durationMs, false, useUnscaled);
        }
        void RemoveTimer(int dur, bool frames, bool useUnscaled) {
            int id = TimerIndex(dur, frames, useUnscaled);
            if (timers.ContainsKey(id)) {
                timers.Remove(id);
            }
        }

        public void OnBeforeSerialize() {
            if (timers.Count > 0) {
                timerIndexes = timers.Select((kv, i) => kv.Key).ToArray();
                timerData = timers.Select((kv, i) => kv.Value).ToArray();
            }
        }

        public void OnAfterDeserialize() {
            if (timerIndexes != null && timerIndexes.Length > 0) {
                // Debug.Log("restoring!");
                // timers = timerData.ToDictionary(td => {
                //     return TimerIndex(td.timerDurMS, td.useFrameCount, td.useUnscaledTimeScale);
                // });
                timerIndexes = null;
                timerData = null;
            }
        }
    }
}