using System;
using UnityEngine;

public class TileEditor : MonoBehaviour
{
    void OnEnable()
    {
		InputManager.OnMouseLeftButton -= OnLeftClick;
		InputManager.OnMouseLeftButton += OnLeftClick;
    }

	// Update is called once per frame
	void OnDisable()
    {
		InputManager.OnMouseLeftButton -= OnLeftClick;
	}

	public void OnLeftClick(bool value, Vector2 screenPosition, Vector3 worldPosition)
	{
		if(value) CreateTile(worldPosition);
	}

	public void CreateTile(Vector3 worldPosition)
	{
		Vector3Int tilePosition = TileManager.GetTileCellPosition(worldPosition);
		if (!TileManager.GetTile(tilePosition))
		{
			TileManager.CreateTileWithBoardCalculation(new TileInfo()
			{
				location = tilePosition,
				baseType = TileBaseType.Dirt,
				decoType = TileDecoType.Grass,
			});
		}
	}
}
