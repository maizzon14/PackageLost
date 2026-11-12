using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
	public uint ItemID;
	public string ItemName;
	public Mesh mesh;
	public Material material;
}