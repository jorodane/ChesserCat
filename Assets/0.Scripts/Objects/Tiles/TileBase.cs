using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum TileHighlightType
{
    None        = 0, 
    Odd         = 1 << 0,
    LastMove    = 1 << 1,

    _Visualizer_    = 1 << 2,

    Movable     = 1 << 3, 
    Attackable  = 1 << 4, 
}

public class TileBase : MonoBehaviour, ISelectable, ISavable<TileSaveData>
{
	[SerializeField] GameObject hoverIcon;
	[SerializeField] Transform socket;
	[SerializeField] Animator anim;
	[SerializeField] SpriteRenderer renderBase;
	[SerializeField] SpriteRenderer renderDeco;
	[SerializeField] SpriteRenderer renderWall;
	[SerializeField] SpriteRenderer renderWallDeco;
    [SerializeField] TextMeshPro tileText;

    TileHighlightType currentHighlight;
    static readonly TileHighlightType constantMask = TileHighlightType.Odd;

    TileInfo _originInfo;

    TileInfo _info;
	public TileInfo Info => _info;

	public Color whiteColor = Color.white;
	public Color OddColor = Color.lightGray;
	public Color baseColor;
	public Color movableColor;
	public Color attackableColor;
	public Color lastMoveColor;

	public bool IsOddTile() => ((Info.location.x + Info.location.y) % 2) == 1;

	public GameObject GetHoveredObject() => Info.objectOnTile ? Info.objectOnTile : gameObject;

    public TileSaveData MakeSaveData() => new()
    {
        basement = _originInfo.basement ? _originInfo.basement.name : "",
        basementVariation = _originInfo.basementVariation,
        decoration = _originInfo.decoration ? _originInfo.decoration.name : "",
		decorationVariation = _originInfo.decorationVariation,
        wallBasement = _originInfo.wallBasement ? _originInfo.wallBasement.name : "",
        wallBasementVariation = _originInfo.wallBasementVariation,
        wallDecoration = _originInfo.wallDecoration ? _originInfo.wallDecoration.name : "",
        wallDecorationVariation = _originInfo.wallDecorationVariation,
        location = _originInfo.location,
        saveDataList = this.MakeCustomSaveData(),
    };

    public void LoadData(in TileSaveData data)
    {
        Set(new TileInfo(data));
    }

    public void ConstructCustomSaveData(Dictionary<string, string> result) { }

    public void ResetAll()
    {
		if(_info.objectOnTile)
		{
			UnsetObject();
		}
		hoverIcon.SetActive(false);
        currentHighlight = TileHighlightType.None;
    }

	public void SetOriginLocation(in Vector3Int location)
	{
		Vector3Int originLocation = _originInfo.location;
		if (originLocation == location) return;
		_originInfo.location = _info.location = location;
		SetLocation(location);
		_info.placeableOnTile?.OriginShifted(location - originLocation);
	}

	public void SetLocation(in Vector3Int location)
	{
		transform.position = TileManager.GetTileWorldPosition(location);
	}

	public void Set(in TileInfo newInfo) => Set(newInfo, newInfo.location);
	public void Set(in TileInfo newInfo, in Vector3Int newLocation)
	{
		ResetAll();
		_info = newInfo;
		_info.location = newLocation;
		_originInfo = _info;
		SetLocation(Info.location);
		currentHighlight |= IsOddTile() ? TileHighlightType.Odd : TileHighlightType.None;
		tileText.SetText(TileManager.GetTileText(Info.location).ToUpper());
		hoverIcon.SetActive(false);
		SetVisual(newInfo);
		UpdateColor();
		SetObject(Info.objectOnTile);
	}

	public void SetVisual(in TileInfo from)
	{
		SetVisual
		(
			from.basement, from.basementVariation,
			from.decoration, from.decorationVariation,
			from.wallBasement, from.wallBasementVariation,
			from.wallDecoration, from.wallDecorationVariation
		);
	}

	void SetVisual(TileBasement basement, string basementVariation, TileDecoration decoration, string decorationVariation, WallBasement wallBasement, string wallBasementVariation, WallDecoration wallDecoration, string wallDecorationVariation)
	{
		_info.basement = basement;
		_info.basementVariation = basementVariation;
		_info.decorationVariation = decorationVariation;
		_info.decoration = decoration;
        _info.wallBasement = wallBasement;
		_info.wallBasementVariation = wallBasementVariation;
		_info.wallDecoration = wallDecoration;
		_info.wallDecorationVariation = wallDecorationVariation;

        SetVisual(renderBase, basement ? basement.GetVisual(basementVariation) : null);
        SetVisual(renderDeco, decoration ? decoration.GetVisual(decorationVariation) : null);
        SetVisual(renderWall, wallBasement ? wallBasement.GetVisual(wallBasementVariation) : null);
        SetVisual(renderWallDeco, wallDecoration ? wallDecoration.GetVisual(wallDecorationVariation) : null);
    }

	void SetVisual(SpriteRenderer targetRender, Sprite newSprite)
	{
        if (targetRender)
        {
            targetRender.sprite = newSprite;
            targetRender.enabled = targetRender.sprite;
        }
    }

    public void SetVisualOrigin(in TileInfo from)
    {
        SetVisualOrigin
        (
            from.basement, from.basementVariation,
            from.decoration, from.decorationVariation,
            from.wallBasement, from.wallBasementVariation,
            from.wallDecoration, from.wallDecorationVariation
        );
    }

    public void SetVisualOrigin(TileBasement basement, string basementVariation, TileDecoration decoration, string decorationVariation, WallBasement wallBasement, string wallBasementVariation, WallDecoration wallDecoration, string wallDecorationVariation)
	{
		_originInfo.basement = basement;
		_originInfo.basementVariation = basementVariation;
		_originInfo.decoration = decoration;
		_originInfo.decorationVariation = decorationVariation;
        _originInfo.wallBasement = wallBasement;
        _originInfo.wallBasementVariation = wallBasementVariation;
        _originInfo.wallDecoration = wallDecoration;
        _originInfo.wallDecorationVariation = wallDecorationVariation;
        SetVisual(_originInfo);
	}

	public void UnsetObject()
    {
        GameObject oldObject = Info.objectOnTile;
        _info.characterOnTile = null;
        _info.objectOnTile = null;
		ITilePlaceable oldPlaceable = Info.placeableOnTile;
		_info.placeableOnTile = null;

		if (oldObject)
        {
            Transform oldTransform = oldObject.transform;
            if (oldTransform)
            {
                oldTransform.SetParent(null);
                oldTransform.localScale = Vector3.one;
            }
			oldPlaceable?.RemoveFromTile(Info, this);
        }
        anim.SetBool("HasObject", false);
    }


    public bool SetObject(GameObject newObject)
    {
		if(newObject)
		{
			Transform newTransform = newObject.transform;
			newTransform.SetParent(socket);
			newTransform.localPosition = Vector3.zero;
			newTransform.localScale = Vector3.one;
            _info.characterOnTile = newObject.GetComponent<CharacterBase>();
			if(newObject.TryGetComponent(out _info.placeableOnTile)) _info.placeableOnTile.PlaceOnTile(Info, this);
			anim.SetBool("HasObject", true);
		}
		else
		{
			UnsetObject();
		}
		_info.objectOnTile = newObject;
		return true;
	}

	public void SetColor(Color newColor)
	{
		renderBase.color = renderDeco.color = newColor;
	}

    public bool CheckHighlight(TileHighlightType wantType) => (currentHighlight & wantType) > 0;

    public void AddHighlight(TileHighlightType wantType)
    {
        currentHighlight |= wantType;
        UpdateColor();
    }

    public void RemoveHighlight(TileHighlightType wantType)
    {
        currentHighlight &= ~wantType;
        UpdateColor();
    }

    public void RemoveHighlight(params TileHighlightType[] wantType)
    {
        TileHighlightType mask = constantMask;
        foreach (TileHighlightType currentType in wantType) mask |= currentType;
        mask &= ~constantMask;
        currentHighlight &= ~mask;
        UpdateColor();
    }

    public void RemoveHighlight()
    {
        currentHighlight &= constantMask;
        UpdateColor();
    }

    public void UpdateColor()
    {
        bool hasVisualizer = currentHighlight > TileHighlightType._Visualizer_;
        if (currentHighlight > 0)
        {
            Color result = OddColor;
            int added = 1;
            if (!CheckHighlight(TileHighlightType.Odd)) result *= 1.2f; 
            if (CheckHighlight(TileHighlightType.Movable))
            { result += movableColor; ++added; }
            if (CheckHighlight(TileHighlightType.Attackable))
            {result += attackableColor; ++added; }
            if (CheckHighlight(TileHighlightType.LastMove))
            { result += lastMoveColor; ++added; }
            result /= added;
            result.a = 1.0f;
            SetColor(baseColor * result);
        }
        else 
        {
            SetColor(baseColor);
        }

        anim.SetBool("HasVisualizer", hasVisualizer);
    }

	public void VisualObjectPass(TileMoveStruct info)
	{
		anim.SetTrigger("Passed");
	}

	public void VisualObjectEnter(TileMoveStruct info)
	{
		SetObject(info.target);
	}

	public void VisualObjectExit(TileMoveStruct info)
	{
		SetObject(null);
	}

	public void MouseHoverEnter()
	{
		anim.SetBool("Hovered", true);
		hoverIcon.SetActive(true);
	}

	public void MouseHoverExit()
	{
		anim.SetBool("Hovered", false);
		hoverIcon.SetActive(false);
	}

	public bool Select(ControllerBase from)
	{
		return true;
	}

	public bool Unselect(ControllerBase from)
	{
		return true;
	}
}
