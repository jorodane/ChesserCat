using System;
using UnityEditor.Rendering;
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

    public void UnsetObject()
    {
        GameObject oldObject = _info.objectOnTile;
        _info.characterOnTile = null;
        _info.objectOnTile = null;

        if (oldObject)
        {
            Transform oldTransform = oldObject.transform;
            if (oldTransform)
            {
                oldTransform.SetParent(null);
                oldTransform.localScale = Vector3.one;
            }
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
			if(newObject.TryGetComponent(out ITilePlaceable asPlaceObject)) asPlaceObject.PlaceOnTile(Info, this);
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
