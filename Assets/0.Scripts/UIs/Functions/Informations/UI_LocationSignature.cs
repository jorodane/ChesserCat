using System;
using TMPro;
using UnityEngine;

public class UI_LocationSignature : UIBase
{
    [SerializeField] TextMeshProUGUI lineText;
    [SerializeField] Color defaultColor;
    [SerializeField] Color highlightedColor;

    public int  index;
    public bool isHorizontal;

    Vector3 calculatedPosition;

    void OnEnable()
    {
        if(isHorizontal)
        {
            CameraManager.OnCameraPositionChanged -= LocationHorizontalUpdate;
            CameraManager.OnCameraPositionChanged += LocationHorizontalUpdate;
        }
        else
        {
            CameraManager.OnCameraPositionChanged -= LocationVerticalUpdate;
            CameraManager.OnCameraPositionChanged += LocationVerticalUpdate;
        }
        TileManager.TileHoverEvent -= CheckTile;
        TileManager.TileHoverEvent += CheckTile;
    }

    void OnDisable()
    {
        CameraManager.OnCameraPositionChanged -= LocationHorizontalUpdate;
        CameraManager.OnCameraPositionChanged -= LocationVerticalUpdate;
        TileManager.TileHoverEvent -= CheckTile;
	}

	void LocationUpdate(Camera targetCamera = default, Vector3 newPosition = default)
	{
        if (isHorizontal)   LocationHorizontalUpdate();
        else                LocationVerticalUpdate();
	}

    void LocationHorizontalUpdate(Camera targetCamera = default, Vector3 newPosition = default)
    {
        calculatedPosition.x = TileManager.GetTileScreenPositionHorizontal(index).x;
        calculatedPosition.y = 0;
        transform.position   = calculatedPosition;
    }

    void LocationVerticalUpdate(Camera targetCamera = default, Vector3 newPosition = default)
    {
        calculatedPosition.x = 0;
        calculatedPosition.y = TileManager.GetTileScreenPositionVertical(index).y;
        transform.position   = calculatedPosition;
    }

    public void SetIndex(int newIndex)
    {
        index = newIndex;
		if(lineText) lineText.SetText(GetTileText(index));
		LocationUpdate();
	}

	string GetTileText(int wantIndex) => isHorizontal? TileManager.GetTileHorizonText(wantIndex) : TileManager.GetTileVerticalText(wantIndex);
    
    void CheckTile(Vector3Int hoverPosition, TileBase tile)
    {
        if (!lineText) return;
        bool isSame = isHorizontal ? hoverPosition.x == index : hoverPosition.y == index;
        lineText.color = isSame ? highlightedColor : defaultColor;
    }
}
