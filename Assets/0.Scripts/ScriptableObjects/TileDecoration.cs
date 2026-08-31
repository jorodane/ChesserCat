using UnityEngine;

[CreateAssetMenu(fileName = "TileDecoration", menuName = "Tiles/TileDecoration")]
public class TileDecoration : ScriptableObject
{
	public TileEnterException enterCheck;
	public TileEnterException EnterCheck() => enterCheck;

	public Sprite visual;
}
