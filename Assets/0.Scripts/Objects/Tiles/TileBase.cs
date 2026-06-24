using System;
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

public class TileBase : MonoBehaviour, ISelectable
{
	[SerializeField] GameObject hoverIcon;
	[SerializeField] Transform socket;
	[SerializeField] Animator anim;
	[SerializeField] SpriteRenderer renderBase;
	[SerializeField] SpriteRenderer renderDeco;

    TileHighlightType currentHighlight;
    static readonly TileHighlightType constantMask = TileHighlightType.Odd;

    TileInfo _info;
	public TileInfo Info => _info;

	public Color whiteColor = Color.white;
	public Color OddColor = Color.lightGray;
	public Color baseColor;
	public Color movableColor;
	public Color attackableColor;
	public Color lastMoveColor;

	public bool IsOddTile() => ((Info.position.x + Info.position.y) % 2) == 1;

	public GameObject GetHoveredObject() => Info.objectOnTile ? Info.objectOnTile : gameObject;

	public void Set(TileInfo newInfo)
	{
		_info = newInfo;
		transform.position = TileManager.GetTileWorldPosition(Info.position);
        currentHighlight |= IsOddTile() ? TileHighlightType.Odd : TileHighlightType.None;
		UpdateColor();
		SetObject(Info.objectOnTile);
	}


	public bool SetObject(GameObject newObject)
	{
		if (_info.objectOnTile && newObject != null) return false;

		if(newObject)
		{
			Transform newTransform = newObject.transform;
			newTransform.SetParent(socket);
			newTransform.localPosition = Vector3.zero;
			newTransform.localScale = Vector3.one;
            _info.characterOnTile = newObject.GetComponent<CharacterBase>();
			if(newObject.TryGetComponent(out ITilePlaceable asPlaceObject))
            {
				asPlaceObject.PlaceOnTile(Info, this);
			}
			else
			{

			}
			anim.SetBool("HasObject", true);
		}
		else
		{
			GameObject oldObject = _info.objectOnTile;
            _info.characterOnTile = null;

            if (oldObject)
			{
				Transform oldTransform = oldObject.transform;
				if(oldTransform)
				{
					oldTransform.SetParent(null);
					oldTransform.localScale = Vector3.one;
				}
			}
			anim.SetBool("HasObject", false);
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
        Color result = baseColor;
        if(CheckHighlight(TileHighlightType.Odd))        result *= OddColor;
        if(CheckHighlight(TileHighlightType.Movable))    result *= movableColor;
        if(CheckHighlight(TileHighlightType.Attackable)) result *= attackableColor;
        if(CheckHighlight(TileHighlightType.LastMove))   result *= lastMoveColor;
        SetColor(result);
        anim.SetBool("HasVisualizer", currentHighlight > TileHighlightType._Visualizer_);
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
