using UnityEngine;

public class TileBase : MonoBehaviour
{
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

    public void Set(TileInfo newInfo)
	{
		_info = newInfo;
		transform.localPosition = TileManager.GetTileWorldPosition(Info.position);
		baseColor = IsOddTile() ? whiteColor : blackColor;
		SetColor(baseColor);
	}

	public bool IsOddTile() => ((Info.position.x + Info.position.y) % 2) == 1;

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
		_info.objectOnTile = info.target;
		info.target.transform.SetParent(socket);
		anim.SetBool("HasObject", true);
	}

	public void VisualObjectExit(TileMoveStruct info)
	{
		info.target.transform.SetParent(null);
		info.target.transform.localScale = Vector3.one;
		anim.SetBool("HasObject", false);
	}
}
