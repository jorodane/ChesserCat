using UnityEngine;

public interface ITilePlaceable
{
	public Vector3Int	CurrentTilePosition { get; set; }
	public TileBase		CurrentTileBase { get; set; }

	public bool PlaceOnTile(TileInfo newInfo, TileBase newTile);
	public bool RemoveFromTile(TileInfo oldInfo, TileBase oldTile);
}
