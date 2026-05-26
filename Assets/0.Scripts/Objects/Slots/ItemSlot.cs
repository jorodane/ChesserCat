using UnityEngine;

public class ItemSlot
{
	//이 칸에 들어있는 아이템의 정보
	[SerializeField] ItemContainer item;
	//이 칸 만의 정보
	[SerializeField] int currentStack;

	public virtual bool Containable(ItemContainer newItem)
	{
		if (item)	return true;
		else		return false;
	}
}
