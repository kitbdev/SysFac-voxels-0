using UnityEngine;

[CreateAssetMenu(fileName = "BlockType", menuName = "SysFac/BlockType", order = 0)]
public class BlockType : ScriptableObject {
	public string idName;
	public string displayName;
	public int textureId;
}