using UnityEngine;
using System.IO;

namespace Kutil {
    public static class SaveSystem {

        static string localPath = Application.dataPath + "/Data/Saves/";
        static string persistentPath = Application.persistentDataPath;
        static string savedataext = ".json";

        // todo encryption

        public static void SaveLocal(string filename, string content, bool overwrite = true) {
            if (!System.IO.Directory.Exists(localPath)) {
                System.IO.Directory.CreateDirectory(localPath);
            }
            string savePath = localPath + filename;
            if (!overwrite) {
                int saveNum = 0;
                string checkPath = savePath;
                while (File.Exists(checkPath + savedataext)) {
                    saveNum++;
                    checkPath = savePath + "_" + saveNum;
                }
                savePath = checkPath;
            }
            savePath += savedataext;
            Debug.Log($"Saved to {savePath}!");
            File.WriteAllText(savePath, content);
        }
        public static string LoadLocal(string filename) {
            string savePath = localPath + filename + savedataext;
            if (File.Exists(savePath)) {
                Debug.Log($"Loaded {savePath}!");
                return File.ReadAllText(savePath);
            }
            return null;
        }

        public static bool TrySaveLocal<T>(string filename, T obj, bool overwrite) {
            string jsonstr = JsonUtility.ToJson(obj);
            if (jsonstr == "{}") {
                Debug.LogWarning("failed to convert " + obj.ToString());
                return false;
            }
            SaveLocal(filename, jsonstr, overwrite);
            return true;
        }
        public static bool TryLoadLocal<T>(string filename, out T obj) {
            var content = LoadLocal(filename);
            try {
                obj = JsonUtility.FromJson<T>(content);
                if (obj == null) {
                    Debug.LogWarning("failed to load, null");
                    return false;
                }
                return true;
            } catch (System.Exception e) {
                Debug.LogWarning("failed to load " + e.ToString());
                obj = default;
                return false;
                // throw;
            }
            // obj = default;
            // return false;
        }

        public static bool TrySave(string filename, string content, string ext = null) {
            ext ??= savedataext;
            string savepath = persistentPath + filename + ext;
            if (File.Exists(savepath)) {
                try {
                    File.WriteAllText(savepath, content);
                } catch (System.Exception e) {
                    Debug.LogWarning($"failed to save {filename} {e.ToString()}");
                    return false;
                    // throw;
                }
                return true;
            }
            return false;
        }
        public static bool TryLoad(string filename, out string content) {
            string savepath = persistentPath + filename;
            if (File.Exists(savepath)) {
                try {
                    content = File.ReadAllText(savepath);
                } catch (System.Exception e) {
                    Debug.LogWarning($"failed to load {filename} {e.ToString()}");
                    content = null;
                    return false;
                    // throw;
                }
            }
            content = null;
            return false;
        }
        public static bool TrySave<T>(string filename, T obj) {
            string jsonstr = JsonUtility.ToJson(obj);
            if (TrySave(filename, jsonstr)) {
                return true;
            }
            return false;
        }
        public static bool TryLoad<T>(string filename, out T obj) {
            if (TryLoad(filename, out var content)) {
                try {
                    obj = JsonUtility.FromJson<T>(content);
                    return true;
                } catch (System.Exception e) {
                    Debug.LogWarning($"failed to load {filename} {e.ToString()}");
                    obj = default;
                    return false;
                    // throw;
                }
            }
            obj = default;
            return false;
        }
    }
}