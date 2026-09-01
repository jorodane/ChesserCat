using UnityEngine;
using UnityEngine.UI;

public class UI_TileEditor : OpenableUIBase
{
	[SerializeField] Image basementImage;
	[SerializeField] Image decorationImage;

	[SerializeField] UI_SwitchField basementField;
	[SerializeField] UI_SwitchField basementVariationField;
	[SerializeField] UI_SwitchField decorationField;
	[SerializeField] UI_SwitchField decorationVariationField;

	TileBasement basementLoaded;
	string basementVariationLoaded;
	TileDecoration decorationLoaded;
	string decorationVariationLoaded;

	void Awake()
	{
		if(basementField)
		{
			basementField.OnSwitchValueChanged -= OnBasementChanged;
			basementField.OnSwitchValueChanged += OnBasementChanged;
		}

		if (basementVariationField)
		{
			basementVariationField.OnSwitchValueChanged -= OnBasementVariationChanged;
			basementVariationField.OnSwitchValueChanged += OnBasementVariationChanged;
		}

		if (decorationField)
		{
			decorationField.OnSwitchValueChanged -= OnDecorationTypeChanged;
			decorationField.OnSwitchValueChanged += OnDecorationTypeChanged;
		}

		if (decorationVariationField)
		{
			decorationVariationField.OnSwitchValueChanged -= OnDecorationVariationChanged;
			decorationVariationField.OnSwitchValueChanged += OnDecorationVariationChanged;
		}
	}

	void OnEnable()
    {
		InputManager.OnMouseLeftButton -= OnLeftClick;
		InputManager.OnMouseLeftButton += OnLeftClick;
		if(basementField)
		{
			basementField.SetContent(TileManager.GetBasementNames(true));
			OnBasementChanged(basementField.SelectedIndex, basementField.SelectedData);
		}

		if (decorationField)
		{
			decorationField.SetContent(TileManager.GetDecorationNames(true));
			OnDecorationTypeChanged(decorationField.SelectedIndex, decorationField.SelectedData);
		}
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
			else if(InputManager.IsControl) CopyTile(worldPosition);
			else if (!InputManager.IsCursorHoverOnUI) CreateTile(worldPosition);
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

	void CopyTile(Vector3 worldPosition)
	{
		Vector3Int tilePosition = TileManager.GetTileCellPosition(worldPosition);
		TileBase targetTile = TileManager.GetTile(tilePosition);
		if (targetTile)
		{
			if (basementField) basementField.SetIndex(targetTile.Info.basement.name);
			if (basementVariationField) basementVariationField.SetIndex(targetTile.Info.basementVariation);
			if (decorationField) decorationField.SetIndex(targetTile.Info.decoration?.name);
			if (decorationVariationField) decorationVariationField.SetIndex(targetTile.Info.decorationVariation);
		}
	}

	public void CreateTile(Vector3 worldPosition)
	{
		Vector3Int tilePosition = TileManager.GetTileCellPosition(worldPosition);

		TileBase targetTile = TileManager.GetTile(tilePosition);

		if (targetTile)
		{
			targetTile.SetVisualOrigin(basementLoaded, basementVariationLoaded, decorationLoaded, decorationVariationLoaded);
		}
		else
		{
			TileManager.CreateTileWithBoardCalculation(new TileInfo()
			{
				location = tilePosition,
				basement = basementLoaded,
				basementVariation = basementVariationLoaded,
				decoration = decorationLoaded,
				decorationVariation = decorationVariationLoaded
			});
		}
	}

	void OnBasementChanged(int index, string data)
	{
		if (string.IsNullOrEmpty(data)) basementLoaded = null;
		else basementLoaded = TileManager.GetBasement(data);
		RefreshTileBasement();
	}

	void OnBasementVariationChanged(int index, string data)
	{
		basementVariationLoaded = data;
		RefreshTileBasement();
	}

	void RefreshTileBasement()
	{
		if (basementLoaded)
		{
			basementImage.enabled = true;
			basementImage.sprite = basementLoaded.GetVisual(basementVariationLoaded);
		}
		else
		{
			basementImage.enabled = false;
		}
	}

	void OnDecorationTypeChanged(int index, string data)
	{
		OnDecorationChanged(index, data, null);
		if (decorationVariationField)
		{
			decorationVariationField.SetContent(decorationLoaded ? decorationLoaded.GetVariationNames(true) : null);
		}
	}
	void OnDecorationVariationChanged(int index, string data)
	{
		decorationVariationLoaded = data;
		RefreshTileDecoration();
	}

	void OnDecorationChanged(int index, string data, string variation)
	{
		decorationVariationLoaded = variation;
		if (string.IsNullOrEmpty(data)) decorationLoaded = null;
		else decorationLoaded = TileManager.GetDecoration(data);
		RefreshTileDecoration();
	}

	void RefreshTileDecoration()
	{
		if (decorationLoaded)
		{
			decorationImage.enabled = true;
			decorationImage.sprite = decorationLoaded.GetVisual(decorationVariationLoaded);
		}
		else
		{
			decorationImage.enabled = false;
		}
	}
}
