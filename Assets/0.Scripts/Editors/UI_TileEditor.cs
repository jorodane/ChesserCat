using UnityEngine;
using UnityEngine.UI;

public class UI_TileEditor : OpenableUIBase
{
	[SerializeField] Image basementImage;
	[SerializeField] Image decorationImage;

	[SerializeField] UI_SwitchField basementField;
	[SerializeField] UI_SwitchField decorationField;

	TileBasement basementLoaded;
	TileDecoration decorationLoaded;

	void Awake()
	{
		if(basementField)
		{
			basementField.OnSwitchValueChanged -= OnBasementChanged;
			basementField.OnSwitchValueChanged += OnBasementChanged;

			OnBasementChanged(basementField.SelectedIndex, basementField.SelectedData);
		}


		if(decorationField)
		{
			decorationField.OnSwitchValueChanged -= OnDecorationChanged;
			decorationField.OnSwitchValueChanged += OnDecorationChanged;
			OnBasementChanged(decorationField.SelectedIndex, decorationField.SelectedData);
		}

	}

	void OnEnable()
    {
		InputManager.OnMouseLeftButton -= OnLeftClick;
		InputManager.OnMouseLeftButton += OnLeftClick;
    }

	void OnDisable()
    {
		InputManager.OnMouseLeftButton -= OnLeftClick;
    }

    public void OnLeftClick(bool value, Vector2 screenPosition, Vector3 worldPosition)
	{
		if (value)
		{
			if(InputManager.IsShift) DestroyTile(worldPosition); 
			else if(!InputManager.IsCursorHoverOnUI) CreateTile(worldPosition);
		}
	}

    void DestroyTile(Vector3 worldPosition)
    {
        Vector3Int tilePosition = TileManager.GetTileCellPosition(worldPosition);
        if (TileManager.GetTile(tilePosition))
        {
            TileManager.RemoveTile(tilePosition);
        }
    }

    public void CreateTile(Vector3 worldPosition)
	{
		Vector3Int tilePosition = TileManager.GetTileCellPosition(worldPosition);

		TileBase targetTile = TileManager.GetTile(tilePosition);

		if (targetTile)
		{
			targetTile.SetVisualOrigin(basementLoaded, decorationLoaded);
		}
		else
		{
			TileManager.CreateTileWithBoardCalculation(new TileInfo()
			{
				location = tilePosition,
				basement = basementLoaded,
				decoration = decorationLoaded,
			});
		}
	}

	void OnBasementChanged(int index, string data)
	{
		if (string.IsNullOrEmpty(data)) basementLoaded = null;
		else basementLoaded = DataManager.LoadDataFile<TileBasement>(data);
		if (basementLoaded)
		{
			basementImage.enabled = true;
			basementImage.sprite = basementLoaded.visual;
		}
		else
		{
			basementImage.enabled = false;
		}
	}

	void OnDecorationChanged(int index, string data)
	{
		if (string.IsNullOrEmpty(data)) decorationLoaded = null;
		else decorationLoaded = DataManager.LoadDataFile<TileDecoration>(data);
		if (decorationLoaded)
		{
			decorationImage.enabled = true;
			decorationImage.sprite = decorationLoaded.visual;
		}
		else
		{
			decorationImage.enabled = false;
		}
	}


}
