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
                // TODO
                BINARY,
                XML, YAML
            }
            public SerializeType serializeType;
            public string filepath;
            public string filename;
            public string fileExtension;
            [SerializeReference]
            public System.Object content;
            public string contentStr;
            public bool createDirIfDoesntExistOnSave = false;
            public bool saveOverwrite = false;
            public bool saveIncrement = false;
            public System.Text.Encoding encoding = System.Text.Encoding.Default;
            // todo encryption

            string fullPath => filepath + filename + fileExtension;

            public SaveBuilder() { }

            private bool TryLoadText() {
                if (File.Exists(fullPath)) {
                    contentStr = File.ReadAllText(fullPath);
                    Debug.Log($"Loaded '{fullPath}'");
                    return true;
                }
                Debug.LogWarning($"File '{fullPath}' does not exist");
                return false;
            }
            private bool TryConvertFromJSON<T>(out T tContent) {
                try {
                    tContent = JsonUtility.FromJson<T>(contentStr);
                    if (tContent == null) {
                        Debug.LogWarning($"failed to convert '{contentStr}' JSON to {typeof(T).Name}");
                        return false;
                    }
                    return true;
                } catch (System.Exception e) {
                    Debug.LogWarning($"failed to convert '{contentStr}' JSON to {typeof(T).Name}. \nError: {e.ToString()}");
                    tContent = default;
                    return false;
                }
            }

            /// <summary>
            /// Actually save content
            /// </summary>
            /// <returns>True if successful</returns>
            public bool TrySave() {
                if (filepath == null || filename == null || fileExtension == null || content == null
                    || serializeType == SerializeType.NONE) {
                    Debug.LogWarning("Some SaveBuilder data not set");
                    return false;
                }
                // actually save
                // serialize content
                if (serializeType == SerializeType.JSON || serializeType == SerializeType.JSONPRETTY) {
                    // json
                    contentStr = JsonUtility.ToJson(content, serializeType == SerializeType.JSONPRETTY);
                    // } else if (serializeType == SerializeType.TEXT) {
                    // } else if (serializeType == SerializeType.BINARY) {
                    // todo
                } else {
                    Debug.LogError($"Save failed unsupported serialization type {serializeType.ToString()}");
                    return false;
                }
                // check directory
                if (!Directory.Exists(filepath)) {
                    if (createDirIfDoesntExistOnSave) {
                        Directory.CreateDirectory(filepath);
                    } else {
                        Debug.LogError($"Save failed directory '{filepath}' does not exist");
                        return false;
                    }
                }
                // check overwrite
                string savepath = filepath + filename;
                string incrementer = "";
                bool baseFileExists = File.Exists(fullPath);
                if (baseFileExists && !saveOverwrite && !saveIncrement) {
                    Debug.LogWarning($"File '{fullPath}' already exists");
                    return false;
                }
                // increment
                if (baseFileExists && saveIncrement && !saveOverwrite) {
                    int saveNum = 1;
                    incrementer = "_" + saveNum;
                    while (File.Exists(savepath + incrementer + fileExtension)) {
                        saveNum++;
                        incrementer = "_" + saveNum;
                    }
                }
                // save
                string saveFullPath = savepath + incrementer + fileExtension;
                if (serializeType == SerializeType.BINARY) {
                    // File.WriteAllBytes(saveFullPath, );
                } else {
                    File.WriteAllText(saveFullPath, contentStr, encoding);
                }
                Debug.Log($"Saved to '{saveFullPath}'");
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif
                return true;
            }
            /// <summary>
            /// Actually load content
            /// </summary>
            /// <returns>True if successful</returns>
            public bool TryLoad<T>(out T loadContent) {
                if (filepath == null || filename == null || fileExtension == null
                    || serializeType == SerializeType.NONE) {
                    Debug.LogWarning("Some SaveBuilder data not set!");
                    loadContent = default;
                    return false;
                }
                if (TryLoadText()) {
                    if (serializeType == SerializeType.JSON || serializeType == SerializeType.JSONPRETTY) {
                        if (TryConvertFromJSON<T>(out loadContent)) {
                            // loaded and converted successfully
                            return true;
                        }
                        // } else if (serializeType == SerializeType.TEXT) {
                    } else {
                        Debug.LogError($"Load failed unsupported serialization type {serializeType.ToString()}");
                    }
                }
                loadContent = default;
                return false;
            }

            public SaveBuilder Content(Object saveObject) {
                this.content = saveObject;
                return this;
            }
            public SaveBuilder Content<T>(T saveObject) {
                this.content = (object)saveObject;// todo test
                return this;
            }
            public SaveBuilder InLocalPath(string filename) {
                this.filename = filename;
                this.filepath = localPath;
                return this;
            }
            public SaveBuilder InLocalDataPath(string filename) {
                this.filename = filename;
                this.filepath = localDataPath;
                return this;
            }
            public SaveBuilder InPersistentDataPath(string filename) {
                this.filename = filename;
                this.filepath = persistentPath;
                return this;
            }
            public SaveBuilder InCustomPath(string filepath, string filename) {
                this.filename = filename;
                this.filepath = filepath;
                return this;
            }
            public SaveBuilder AsJSON(bool prettyFormat = false) {
                this.fileExtension ??= ".json";
                this.serializeType = prettyFormat ? SerializeType.JSONPRETTY : SerializeType.JSON;
                return this;
            }
            public SaveBuilder AsText() {
                this.serializeType = SerializeType.TEXT;
                this.fileExtension ??= ".txt";
                return this;
            }
            public SaveBuilder As(SerializeType serializeType) {
                this.serializeType = serializeType;
                if (serializeType == SerializeType.JSON || serializeType == SerializeType.JSONPRETTY) {
                    this.fileExtension ??= ".json";
                } else if (serializeType == SerializeType.TEXT) {
                    this.fileExtension ??= ".txt";
                } else if (serializeType == SerializeType.BINARY) {
                    this.fileExtension ??= ".bin";
                } else if (serializeType == SerializeType.XML) {
                    this.fileExtension ??= ".xml";
                } else if (serializeType == SerializeType.YAML) {
                    this.fileExtension ??= ".yaml";
                }
                return this;
            }
            public SaveBuilder WithCustomExtension(string extension = "txt") {
                this.fileExtension = "." + extension;
                return this;
            }
            public SaveBuilder CanOverwrite(bool canOverwrite = true) {
                this.saveOverwrite = canOverwrite;
                return this;
            }
            public SaveBuilder IncrementIfExists(bool increment = true) {
                this.saveIncrement = increment;
                return this;
            }
            public SaveBuilder CreateDirIfDoesntExist(bool createIfDoesntExist = true) {
                this.createDirIfDoesntExistOnSave = createIfDoesntExist;
                return this;
            }
            public SaveBuilder SetEncoding(System.Text.Encoding encoding) {
                this.encoding = encoding;
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