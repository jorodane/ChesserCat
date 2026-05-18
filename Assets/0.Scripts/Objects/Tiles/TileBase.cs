using System;
using UnityEngine;

public class TileBase : MonoBehaviour, ISelectable
{
	[SerializeField] GameObject hoverIcon;
	[SerializeField] Transform socket;
	[SerializeField] Animator anim;
	[SerializeField] SpriteRenderer renderBase;
	[SerializeField] SpriteRenderer renderDeco;

	Color whiteColor = Color.white;
	Color blackColor = Color.lightGray;

	TileInfo _info;
	public TileInfo Info => _info;

	public Color baseColor;
	public Color movableColor;
	public Color attackableColor;

	public bool IsOddTile() => ((Info.position.x + Info.position.y) % 2) == 1;

	public GameObject GetHoveredObject() => Info.objectOnTile ? Info.objectOnTile : gameObject;

	public void Set(TileInfo newInfo)
	{
		_info = newInfo;
		transform.localPosition = TileManager.GetTileWorldPosition(Info.position);
		baseColor = IsOddTile() ? whiteColor : blackColor;
		SetColor(baseColor);
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

	public void VisualAvailableMove()
	{
		SetColor(movableColor * baseColor);
		anim.SetBool("HasVisualizer", true);
	}

	public void VisualAvailableAttack()
	{
		SetColor(attackableColor * baseColor);
		anim.SetBool("HasVisualizer", true);
	}

	public void VisualAvailableClear()
	{
		SetColor(baseColor);
		anim.SetBool("HasVisualizer", false);
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
		SetObject(info.target);
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
