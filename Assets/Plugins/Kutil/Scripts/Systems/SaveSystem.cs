using UnityEngine;
using System.IO;

namespace Kutil {
    public static class SaveSystem {
        [System.Serializable]
        public class SaveBuilder {
            public enum SerializeType {
                NONE,
                JSON, JSONPRETTY,
                TEXT,
                BINARY,
                // TODO
                // CUSTOM,
                XML,
                YAML,
            }
            [System.Serializable]
            public class SaveBuilderData {
                public SerializeType serializeType;
                public string filepath;
                public string filename;
                public string fileExtension;
                // [SerializeReference]
                public System.Object content;
                public string contentStr;
                public byte[] contentBytes;
                public bool createDirIfDoesntExistOnSave = false;
                public bool saveOverwrite = false;
                public bool saveIncrement = false;
                public System.Text.Encoding encoding = System.Text.Encoding.Default;
            }
            public SaveBuilderData data = new SaveBuilderData();
            // todo encryption

            string fullPath => data.filepath + data.filename + data.fileExtension;

            public SaveBuilder() { }

            private bool IsValid() {
                return (data.filepath != null && data.filename != null && data.fileExtension != null
                                    && data.serializeType != SerializeType.NONE);
            }
            private bool TryConvertFromJSON<T>(out T tContent) {
                try {
                    tContent = JsonUtility.FromJson<T>(data.contentStr);
                    if (tContent == null) {
                        Debug.LogWarning($"failed to convert '{data.contentStr}' JSON to {typeof(T).Name}");
                        return false;
                    }
                    return true;
                } catch (System.Exception e) {
                    Debug.LogWarning($"failed to convert '{data.contentStr}' JSON to {typeof(T).Name}. \nError: {e.ToString()}");
                    tContent = default;
                    return false;
                }
            }

            /// <summary>
            /// Actually save content
            /// </summary>
            /// <returns>True if successful</returns>
            public bool TrySave() {
                if (!IsValid()) {
                    Debug.LogWarning("Some SaveBuilder data not set");
                    return false;
                }
                // actually save
                // serialize content
                if (data.serializeType == SerializeType.JSON || data.serializeType == SerializeType.JSONPRETTY) {
                    if (data.content == null) {
                        Debug.LogWarning("object content not set!");
                        return false;
                    }
                    // json
                    data.contentStr = JsonUtility.ToJson(data.content, data.serializeType == SerializeType.JSONPRETTY);
                } else if (data.serializeType == SerializeType.BINARY) {
                    if (data.contentBytes == null) {
                        Debug.LogWarning("Binary content bytes not set");
                        return false;
                    }
                } else if (data.serializeType == SerializeType.TEXT) {
                    if (data.contentStr == null) {
                        Debug.LogWarning("string content not set!");
                        return false;
                    }
                    // } else if (data.serializeType == SerializeType.XML) {
                    // todo
                } else {
                    Debug.LogError($"Save failed unsupported serialization type {data.serializeType.ToString()}");
                    return false;
                }
                // check directory
                if (!Directory.Exists(data.filepath)) {
                    if (data.createDirIfDoesntExistOnSave) {
                        Directory.CreateDirectory(data.filepath);
                    } else {
                        Debug.LogError($"Save failed directory '{data.filepath}' does not exist");
                        return false;
                    }
                }
                // check overwrite
                string savepath = data.filepath + data.filename;
                string incrementer = "";
                bool baseFileExists = File.Exists(fullPath);
                if (baseFileExists && !data.saveOverwrite && !data.saveIncrement) {
                    Debug.LogWarning($"File '{fullPath}' already exists");
                    return false;
                }
                // increment
                if (baseFileExists && data.saveIncrement && !data.saveOverwrite) {
                    incrementer = GetIncrementInfix(savepath, data.fileExtension);
                }
                // save
                string saveFullPath = savepath + incrementer + data.fileExtension;
                if (data.serializeType == SerializeType.BINARY) {
                    File.WriteAllBytes(saveFullPath, data.contentBytes);
                } else {
                    File.WriteAllText(saveFullPath, data.contentStr, data.encoding);
                }
                Debug.Log($"Saved to '{saveFullPath}'");
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif
                return true;
            }

            private string GetIncrementInfix(string savepath, string fileExtension) {
                string incrementer;
                int saveNum = 1;
                incrementer = "_" + saveNum;
                // savepath = RemoveIncrementerInfix(savepath); //?
                while (File.Exists(savepath + incrementer + fileExtension)) {
                    saveNum++;
                    incrementer = "_" + saveNum;
                }
                return incrementer;
            }
            private static string RemoveIncrementerInfix(string fullpath) {
                int from = fullpath.LastIndexOf('_');
                int to = fullpath.LastIndexOf('.') - 1;
                if (from >= 0 && to > 0) {
                    return fullpath.Remove(from, to - from);
                } else {
                    // no incrementor found
                    return fullpath;
                }
            }

            public bool TryLoadText(out string text) {
                if (!IsValid()) {
                    Debug.LogWarning("Some SaveBuilder data not set!");
                    text = default;
                    return false;
                }
                if (File.Exists(fullPath)) {
                    text = File.ReadAllText(fullPath);
                    Debug.Log($"Loaded '{fullPath}'");
                    return true;
                }
                Debug.LogWarning($"File '{fullPath}' does not exist");
                text = null;
                return false;
            }
            public bool TryLoadBin(out byte[] bytes) {
                if (!IsValid()) {
                    Debug.LogWarning("Some SaveBuilder data not set!");
                    bytes = default;
                    return false;
                }
                if (File.Exists(fullPath)) {
                    bytes = File.ReadAllBytes(fullPath);
                    Debug.Log($"Loaded '{fullPath}'");
                    return true;
                }
                Debug.LogWarning($"File '{fullPath}' does not exist");
                bytes = default;
                return false;
            }
            /// <summary>
            /// Actually load content
            /// </summary>
            /// <returns>True if successful</returns>
            public bool TryLoad<T>(out T loadContent) {
                if (!IsValid()) {
                    Debug.LogWarning("Some SaveBuilder data not set!");
                    loadContent = default;
                    return false;
                }
                if (TryLoadText(out data.contentStr)) {
                    if (data.serializeType == SerializeType.JSON || data.serializeType == SerializeType.JSONPRETTY) {
                        if (TryConvertFromJSON<T>(out loadContent)) {
                            // loaded and converted successfully
                            return true;
                        }
                    }
                    // todo xml
                } else {
                    Debug.LogError($"Load failed unsupported serialization type {data.serializeType.ToString()}");
                }
                loadContent = default;
                return false;
            }


            public SaveBuilder Clear() {
                this.data = new SaveBuilderData();
                return this;
            }
            public SaveBuilder Content(Object saveObject) {
                this.data.content = saveObject;
                return this;
            }
            public SaveBuilder Content(byte[] contentBytes) {
                this.data.contentBytes = contentBytes;
                return this.As(SerializeType.BINARY);
            }
            public SaveBuilder Content(string contentStr) {
                this.data.contentStr = contentStr;
                if (this.data.serializeType == SerializeType.NONE) {
                    return this.AsText();
                }
                return this;
            }
            public SaveBuilder Content<T>(T saveObject) {
                this.data.content = (object)saveObject;// todo test
                return this;
            }
            public SaveBuilder InLocalPath(string filename) {
                this.data.filename = filename;
                this.data.filepath = localPath;
                return this;
            }
            public SaveBuilder InLocalDataPath(string filename) {
                this.data.filename = filename;
                this.data.filepath = localDataPath;
                return this;
            }
            public SaveBuilder InPersistentDataPath(string filename) {
                this.data.filename = filename;
                this.data.filepath = persistentPath;
                return this;
            }
            public SaveBuilder InCustomPath(string filepath, string filename) {
                this.data.filename = filename;
                this.data.filepath = filepath;
                return this;
            }
            public SaveBuilder InCustomFullPath(string filepath) {
                this.data.filename = "";
                this.data.fileExtension = "";
                this.data.filepath = filepath;
                return this;
            }
            public SaveBuilder AsJSON(bool prettyFormat = false) {
                this.data.fileExtension ??= ".json";
                this.data.serializeType = prettyFormat ? SerializeType.JSONPRETTY : SerializeType.JSON;
                return this;
            }
            public SaveBuilder AsText() {
                return this.As(SerializeType.TEXT);
            }
            public SaveBuilder As(SerializeType serializeType) {
                this.data.serializeType = serializeType;
                if (serializeType == SerializeType.JSON || serializeType == SerializeType.JSONPRETTY) {
                    this.data.fileExtension ??= ".json";
                } else if (serializeType == SerializeType.TEXT) {
                    this.data.fileExtension ??= ".txt";
                } else if (serializeType == SerializeType.BINARY) {
                    this.data.fileExtension ??= ".bin";
                } else if (serializeType == SerializeType.XML) {
                    this.data.fileExtension ??= ".xml";
                } else if (serializeType == SerializeType.YAML) {
                    this.data.fileExtension ??= ".yaml";
                }
                return this;
            }
            public SaveBuilder CustomExtension(string extension = "txt") {
                this.data.fileExtension = "." + extension;
                return this;
            }
            public SaveBuilder CanOverwrite(bool canOverwrite = true) {
                this.data.saveOverwrite = canOverwrite;
                return this;
            }
            public SaveBuilder IncrementIfExists(bool increment = true) {
                this.data.saveIncrement = increment;
                return this;
            }
            public SaveBuilder CreateDirIfDoesntExist(bool createIfDoesntExist = true) {
                this.data.createDirIfDoesntExistOnSave = createIfDoesntExist;
                return this;
            }
            public SaveBuilder SetEncoding(System.Text.Encoding encoding) {
                this.data.encoding = encoding;
                return this;
            }
        }

        static string localPath = Application.dataPath + "/";
        static string localDataPath => localPath + "Data/";
        static string persistentPath = Application.persistentDataPath;
        // static string savedataext = ".json";


        public static SaveBuilder StartSave() {
            return new SaveBuilder();
        }
        public static SaveBuilder StartLoad() {
            return new SaveBuilder();
        }


        // [System.Obsolete("Obsolete. use StartSave or StartLoad instead")]
        //         public static void SaveLocal(string filename, string content, bool overwrite = true) {
        //             if (!System.IO.Directory.Exists(localDataPath)) {
        //                 System.IO.Directory.CreateDirectory(localDataPath);
        //             }
        //             string savePath = localDataPath + filename;
        //             if (!overwrite) {
        //                 int saveNum = 0;
        //                 string checkPath = savePath;
        //                 while (File.Exists(checkPath + savedataext)) {
        //                     saveNum++;
        //                     checkPath = savePath + "_" + saveNum;
        //                 }
        //                 savePath = checkPath;
        //             }
        //             savePath += savedataext;
        //             Debug.Log($"Saved to {savePath}!");
        //             File.WriteAllText(savePath, content);
        // #if UNITY_EDITOR
        //             UnityEditor.AssetDatabase.Refresh();
        // #endif
        //         }
        //         public static string LoadLocal(string filename) {
        //             string savePath = localDataPath + filename + savedataext;
        //             if (File.Exists(savePath)) {
        //                 Debug.Log($"Loaded {savePath}!");
        //                 return File.ReadAllText(savePath);
        //             }
        //             return null;
        //         }

        //         public static bool TrySaveLocal<T>(string filename, T obj, bool overwrite) {
        //             string jsonstr = JsonUtility.ToJson(obj);
        //             if (jsonstr == "{}") {
        //                 Debug.LogWarning("failed to convert " + obj.ToString());
        //                 return false;
        //             }
        //             SaveLocal(filename, jsonstr, overwrite);
        //             return true;
        //         }
        //         public static bool TryLoadLocal<T>(string filename, out T obj) {
        //             var content = LoadLocal(filename);
        //             try {
        //                 obj = JsonUtility.FromJson<T>(content);
        //                 if (obj == null) {
        //                     Debug.LogWarning("failed to load, null");
        //                     return false;
        //                 }
        //                 return true;
        //             } catch (System.Exception e) {
        //                 Debug.LogWarning("failed to load " + e.ToString());
        //                 obj = default;
        //                 return false;
        //                 // throw;
        //             }
        //             // obj = default;
        //             // return false;
        //         }

        //         public static bool TrySave(string filename, string content, string ext = null) {
        //             ext ??= savedataext;
        //             string savepath = persistentPath + filename + ext;
        //             if (File.Exists(savepath)) {
        //                 try {
        //                     File.WriteAllText(savepath, content);
        //                 } catch (System.Exception e) {
        //                     Debug.LogWarning($"failed to save {filename} {e.ToString()}");
        //                     return false;
        //                     // throw;
        //                 }
        //                 return true;
        //             }
        //             return false;
        //         }
        //         public static bool TryLoad(string filename, out string content) {
        //             string savepath = persistentPath + filename;
        //             if (File.Exists(savepath)) {
        //                 try {
        //                     content = File.ReadAllText(savepath);
        //                 } catch (System.Exception e) {
        //                     Debug.LogWarning($"failed to load {filename} {e.ToString()}");
        //                     content = null;
        //                     return false;
        //                     // throw;
        //                 }
        //             }
        //             content = null;
        //             return false;
        //         }
        //         public static bool TrySave<T>(string filename, T obj) {
        //             string jsonstr = JsonUtility.ToJson(obj);
        //             if (TrySave(filename, jsonstr)) {
        //                 return true;
        //             }
        //             return false;
        //         }
        //         public static bool TryLoad<T>(string filename, out T obj) {
        //             if (TryLoad(filename, out var content)) {
        //                 try {
        //                     obj = JsonUtility.FromJson<T>(content);
        //                     return true;
        //                 } catch (System.Exception e) {
        //                     Debug.LogWarning($"failed to load {filename} {e.ToString()}");
        //                     obj = default;
        //                     return false;
        //                     // throw;
        //                 }
        //             }
        //             obj = default;
        //             return false;
        //         }
    }
}