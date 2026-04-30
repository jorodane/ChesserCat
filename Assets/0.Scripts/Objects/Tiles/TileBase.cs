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

    public void Set(TileInfo newInfo)
	{
		_info = newInfo;
		transform.localPosition = TileManager.GetTileWorldPosition(Info.position);

		if (((Info.position.x + Info.position.y) % 2) == 1)
		{
			renderBase.color = renderDeco.color = blackColor;
		}
		else
		{
			renderBase.color = renderDeco.color = whiteColor;
		}
	}

	public void VisualObjectPass(TileMoveStruct info)
	{
		anim.SetTrigger("Passed");
	}

	public void VisualObjectEnter(TileMoveStruct info)
	{
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
