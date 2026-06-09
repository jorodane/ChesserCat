using UnityEngine;

public interface ITilePlaceable
{
	public Vector3Int	CurrentTilePosition { get; set; }
	public TileBase		CurrentTileBase { get; set; }

	public bool PlaceOnTile(in TileInfo newInfo, TileBase newTile);
	public bool RemoveFromTile(in TileInfo oldInfo, TileBase oldTile);
}
