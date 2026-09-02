using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_TileEditor : OpenableUIBase
{
	[SerializeField] Image basementImage;
	[SerializeField] Image decorationImage;
	[SerializeField] Image wallBasementImage;
	[SerializeField] Image wallDecorationImage;

	[SerializeField] UI_SwitchField_Drawable basementField;
	[SerializeField] UI_SwitchField_Drawable decorationField;
    [SerializeField] UI_SwitchField_Drawable wallBasementField;
    [SerializeField] UI_SwitchField_Drawable wallDecorationField;

    TileBasement basementLoaded;
	string basementVariationLoaded;
	TileDecoration decorationLoaded;
	string decorationVariationLoaded;

    WallBasement wallBasementLoaded;
    string wallBasementVariationLoaded;
    WallDecoration wallDecorationLoaded;
    string wallDecorationVariationLoaded;

    void Awake()
	{
		ConnectSwitchField(basementField, TileManager.GetBasement, OnBasementChanged, OnBasementVariationChanged);
		ConnectSwitchField(decorationField, TileManager.GetDecoration, OnDecorationTypeChanged, OnDecorationVariationChanged);
		ConnectSwitchField(wallBasementField, TileManager.GetWallBasement, OnWallBasementTypeChanged, OnWallBasementVariationChanged);
		ConnectSwitchField(wallDecorationField, TileManager.GetWallDecoration, OnWallDecorationTypeChanged, OnWallDecorationVariationChanged);
	}

	void OnEnable()
    {
		InputManager.OnMouseLeftButton -= OnLeftClick;
		InputManager.OnMouseLeftButton += OnLeftClick;

        InitiateSwitchField(basementField, OnBasementChanged, TileManager.GetBasementNames(false));
        InitiateSwitchField(decorationField, OnDecorationTypeChanged, TileManager.GetDecorationNames(true));
        InitiateSwitchField(wallBasementField, OnWallBasementTypeChanged, TileManager.GetWallBasementNames(true));
        InitiateSwitchField(wallDecorationField, OnWallDecorationTypeChanged, TileManager.GetWallDecorationNames(true));
    }

	void OnDisable()
    {
		InputManager.OnMouseLeftButton -= OnLeftClick;
    }
    public void OnLeftClick(bool value, Vector2 screenPosition, Vector3 worldPosition)
    {
        if (value)
        {
            if (InputManager.IsShift) DestroyTile(worldPosition);
            else if (InputManager.IsControl) CopyTile(worldPosition);
            else if (!InputManager.IsCursorHoverOnUI) CreateTile(worldPosition);
        }
    }

    void InitiateSwitchField(UI_SwitchField_Drawable targetField, SwitchDrawableValueChangeEvent InitFunction, IEnumerable<string> names)
	{
        if (targetField)
        {
            targetField.Initialize();
            targetField.SetContent(names);
            InitFunction?.Invoke(targetField.SelectedIndex, targetField.SelectedDrawable);
        }
    }

    void ConnectSwitchField(UI_SwitchField_Drawable targetField, GetDrawableFunction getter, SwitchDrawableValueChangeEvent valueChange, SwitchVariationValueChangeEvent variationChange)
    {
        if (targetField)
        {
            targetField.OnGetDrawable = getter;
            targetField.OnSwitchDrawableValueChanged -= valueChange;
            targetField.OnSwitchDrawableValueChanged += valueChange;
            targetField.OnSwitchVariationValueChanged -= variationChange;
            targetField.OnSwitchVariationValueChanged += variationChange;
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
			if (basementField) basementField.SetIndex(targetTile.Info.basement?.name, targetTile.Info.basementVariation);
			if (decorationField) decorationField.SetIndex(targetTile.Info.decoration?.name, targetTile.Info.decorationVariation);
            if (wallBasementField) wallBasementField.SetIndex(targetTile.Info.wallBasement?.name, targetTile.Info.wallBasementVariation);
            if (wallDecorationField) wallDecorationField.SetIndex(targetTile.Info.wallDecoration?.name, targetTile.Info.wallDecorationVariation);
        }
	}

	public void CreateTile(Vector3 worldPosition)
	{
		Vector3Int tilePosition = TileManager.GetTileCellPosition(worldPosition);

		TileBase targetTile = TileManager.GetTile(tilePosition);

		if (targetTile)
		{
			targetTile.SetVisualOrigin
            (
                basementLoaded, basementVariationLoaded, 
                decorationLoaded, decorationVariationLoaded, 
                wallBasementLoaded, wallBasementVariationLoaded, 
                wallDecorationLoaded, wallDecorationVariationLoaded
            );
		}
		else
		{
			TileManager.CreateTileWithBoardCalculation(new TileInfo()
			{
				location = tilePosition,
				basement = basementLoaded,
				basementVariation = basementVariationLoaded,
				decoration = decorationLoaded,
				decorationVariation = decorationVariationLoaded,
				wallBasement = wallBasementLoaded, 
				wallBasementVariation = wallBasementVariationLoaded,
				wallDecoration = wallDecorationLoaded,
				wallDecorationVariation = wallDecorationVariationLoaded,
			});
		}
	}

	void OnBasementChanged(int index, DrawableBase data)
	{
		basementLoaded = data as TileBasement;
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

	void OnDecorationTypeChanged(int index, DrawableBase data)
	{
		OnDecorationChanged(data as TileDecoration, null);
	}

	void OnDecorationVariationChanged(int index, string data)
	{
		decorationVariationLoaded = data;
		RefreshTileDecoration();
	}

	void OnDecorationChanged(TileDecoration data, string variation)
	{
		decorationVariationLoaded = variation;
		decorationLoaded = data;
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

    void OnWallBasementTypeChanged(int index, DrawableBase data)
    {
        OnWallBasementChanged(data as WallBasement, null);
    }

    void OnWallBasementVariationChanged(int index, string data)
    {
        wallBasementVariationLoaded = data;
        RefreshWallBasement();
    }

    void OnWallBasementChanged(WallBasement data, string variation)
    {
        wallBasementVariationLoaded = variation;
        wallBasementLoaded = data;
        RefreshWallBasement();
    }

    void RefreshWallBasement()
    {
        if (wallBasementLoaded)
        {
            wallBasementImage.enabled = true;
            wallBasementImage.sprite = wallBasementLoaded.GetVisual(wallBasementVariationLoaded);
        }
        else
        {
            wallBasementImage.enabled = false;
        }
    }

    void OnWallDecorationTypeChanged(int index, DrawableBase data)
    {
        OnWallDecorationChanged(data as WallDecoration, null);
    }

    void OnWallDecorationVariationChanged(int index, string data)
    {
        wallDecorationVariationLoaded = data;
        RefreshWallDecoration();
    }

    void OnWallDecorationChanged(WallDecoration data, string variation)
    {
        wallDecorationVariationLoaded = variation;
        wallDecorationLoaded = data;
        RefreshWallDecoration();
    }

    void RefreshWallDecoration()
    {
        if (wallDecorationLoaded)
        {
            wallDecorationImage.enabled = true;
            wallDecorationImage.sprite = wallDecorationLoaded.GetVisual(wallDecorationVariationLoaded);
        }
        else
        {
            wallDecorationImage.enabled = false;
        }
    }
}
