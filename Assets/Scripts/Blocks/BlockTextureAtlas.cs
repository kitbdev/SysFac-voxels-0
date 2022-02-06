using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kutil;
using System.Linq;

public class BlockTextureAtlas : MonoBehaviour {

    [SerializeField] Texture2D _atlas;
    [SerializeField, ReadOnly] int _textureResolution = 16;
    public int atlasSize = 512;
    [SerializeField] Texture2D[] topack;
    [SerializeField] protected Dictionary<string, Vector2> _packDict = new Dictionary<string, Vector2>();

    public Texture2D atlas { get => _atlas; protected set => _atlas = value; }
    public Dictionary<string, Vector2> packDict { get => _packDict; protected set => _packDict = value; }
    public int textureResolution { get => _textureResolution; protected set => _textureResolution = value; }

    private void Awake() {
        Pack();
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
        // UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_UInt,);
        atlas.name = "BlockAtlas";
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
        // atlas.Reinitialize(atlasWidth, atlasWidth, TextureFormat.RGBA32, true);
        // atlas.Apply();
        ResizeTexture(atlas, atlasSize, atlasSize);
        for (int i = 0; i < pack.Length; i++) {
            Texture2D tex = pack[i];
            Vector2Int texStartPos = Vector2Int.FloorToInt(rects[i].min / textureResolution);
            packDict.Add(tex.name, texStartPos);
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