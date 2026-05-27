using System;
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
	public ItemContainer GetItem()	=> item;
	public int GetStack()			=> currentStack;
	//                                 조건 ?      맞으면                    : 아니면
	public bool GetIsMax() =>          item ? currentStack >= item.maxStack : false;

	//internal : 내부적인! => 나랑 같은 프로젝트에 있는 대상은 모두 쓸 수 있음!
	//반환값 : 추가했더니 못 추가하고 넘겨버린 것!
	public int AddItem(ItemContainer wantItem, int amount)
	{
		if (!wantItem) return 0; //아이템이 없는데 남는게 어딨어
		if (amount <= 0) return 0;      //준게 없는데 남는게 어딨어
		// 내가 아이템이 있는데   내 아이템이랑 들어온 아이템이 달라!
		if (item			  && item != wantItem) return amount;
		//                                        그거 그대로 들고 나와

		return amount; //남은 값을 돌려준다!
	}
}
