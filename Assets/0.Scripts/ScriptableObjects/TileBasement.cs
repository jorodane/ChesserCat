using UnityEngine;

[CreateAssetMenu(fileName = "TileBasement", menuName = "Tiles/TileBasement")]
public class TileBasement : ScriptableObject
{
	public TileEnterException enterCheck;
	public TileEnterException EnterCheck() => enterCheck;

	public Sprite visual;
}
