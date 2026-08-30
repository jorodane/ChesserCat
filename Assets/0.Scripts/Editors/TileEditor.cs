using System;
using UnityEngine;

public class TileEditor : MonoBehaviour
{
    void OnEnable()
    {
		InputManager.OnMouseLeftButton -= OnLeftClick;
		InputManager.OnMouseLeftButton += OnLeftClick;

        InputManager.OnDestroyTarget -= OnDestroyTarget;
        InputManager.OnDestroyTarget += OnDestroyTarget;
    }

	// Update is called once per frame
	void OnDisable()
    {
		InputManager.OnMouseLeftButton -= OnLeftClick;
        InputManager.OnDestroyTarget -= OnDestroyTarget;
    }



    public void OnLeftClick(bool value, Vector2 screenPosition, Vector3 worldPosition)
	{
		if(value) CreateTile(worldPosition);
	}

    void OnDestroyTarget(bool value)
    {
        Vector3Int tilePosition = TileManager.GetTileCellPosition(InputManager.CursorWorldPosition);
        if (TileManager.GetTile(tilePosition))
        {
            TileManager.RemoveTile(tilePosition);
        }
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
