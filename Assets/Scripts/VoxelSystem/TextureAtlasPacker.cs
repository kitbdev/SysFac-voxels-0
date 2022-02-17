using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kutil;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-20)]
[CreateAssetMenu(fileName = "TextureAtlasPacker", menuName = "VoxelSystem/TextureAtlasPacker", order = 0)]
public class TextureAtlasPacker : ScriptableObject {

    [SerializeField] string atlasName;
    [SerializeField, ReadOnly] Texture2D _atlas;
    [SerializeField] bool saveAtlas = true;
    [SerializeField, ReadOnly] int _textureResolution = 16;
    public int atlasSize = 512;
    [SerializeField] Texture2D[] topack;
    [System.Serializable] public class DictionaryStringVector2 : SerializableDictionary<string, Vector2> { }
    [SerializeField, HideInInspector] protected DictionaryStringVector2 _packDict = new DictionaryStringVector2();

    public Texture2D atlas { get => _atlas; protected set => _atlas = value; }
    public DictionaryStringVector2 packDict { get => _packDict; protected set => _packDict = value; }
    public int textureResolution { get => _textureResolution; protected set => _textureResolution = value; }
    public float textureBlockScale => ((float)textureResolution) / atlasSize;

    // private void Awake() {
    //     Pack();
    // }
    private void OnValidate() {
        if (topack.Length > 0) {
            textureResolution = topack[0].width;
        }
    }

    [ContextMenu("Pack")]
    public void Pack() {
        if (topack.Length == 0) {
            return;
        }
        packDict.Clear();
        textureResolution = topack[0].width;
        atlas = new Texture2D(atlasSize, atlasSize,
            TextureFormat.RGBA32, true);
        atlas.name = atlasName ?? name;
        // UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_UInt,);
        atlas.alphaIsTransparency = true;
        atlas.filterMode = FilterMode.Point;
        // List<Color> colors = new Color[atlasWidth * atlasWidth].ToList();
        // colors.ForEach((c) => c = new Color(1, 0, 0.5f, 1));
        // atlas.SetPixels(colors.ToArray());
        // atlas.Apply();
        // atlas.SetPixels(0,0,textureResolution,textureResolution,)
        Texture2D[] pack = topack.Where((tex) => tex.height == tex.width && tex.width == textureResolution).ToArray();
        var forgotten = topack.Except(pack);
        foreach (var ftex in forgotten) {
            // Debug.Log($"Texture {ftex.name} ({ftex.width},{ftex.height}) could not be packed!");
        }

        Rect[] rects = atlas.PackTextures(pack, 0, 2048);
        int iwidth = atlas.width;
        int iheight = atlas.height;
        ResizeTexture(atlas, atlasSize, atlasSize);
        for (int i = 0; i < pack.Length; i++) {
            Texture2D tex = pack[i];
            Vector2 coord = rects[i].xMin * iwidth * Vector2.right + rects[i].yMin * iheight * Vector2.up;
            Vector2Int texStartPos = Vector2Int.FloorToInt(coord / textureResolution);
            // Debug.Log($"'{tex.name}' {texStartPos} r:{rects[i].min}");
            packDict.Add(tex.name, texStartPos);
        }
        if (saveAtlas) {
            string path = Application.dataPath + "/data/" + atlasName + ".png";
            System.IO.File.WriteAllBytes(path, atlas.EncodeToPNG());
            Debug.Log($"Saving to '{path}'");
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
    }
    static void ResizeTexture(Texture2D texture, int width, int height) {
        Color[] colors = texture.GetPixels();
        int iwid = texture.width;
        int iheight = texture.height;
        texture.Reinitialize(width, height, TextureFormat.RGBA32, true);
        texture.SetPixels(0, 0, iwid, iheight, colors);
        texture.Apply();
    }
}