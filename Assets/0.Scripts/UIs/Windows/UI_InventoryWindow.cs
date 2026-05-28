using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_InventoryWindow : OpenableUIBase
{
	[SerializeField] Inventory targetInventory;
	[SerializeField] LayoutGroup layout;
	[SerializeField] string itemSlotPrefabName;

	public override void Registration(UIManager manager)
	{
		base.Registration(manager);
		targetInventory?.Initialize();
		ConnectInventory(targetInventory);
	}

	public override void Unregistration(UIManager manager)
	{
		base.Unregistration(manager);
		DisconnectInventory();
	}

	public void ConnectInventory(Inventory newInventory)
	{
		if (!newInventory) return; // 인벤토리가 있어야지!
		targetInventory = newInventory; //인벤토리 등록은 하고!

		if (!layout) return;

		//layout이 gridLayoutGroup이라면
		if(layout is GridLayoutGroup asGridLayout)
		{
			//그리드레이아웃의 제한 개수는 = 대상 인벤토리의 열 개수
			asGridLayout.constraintCount = targetInventory.columns;
		}

		//인벤토리가 있다면 그 안에 있는 슬롯을 전부 가져온다!
		//인벤토리 슬롯들을 하나하나 만들어줄 것임!
		foreach(ItemSlot currentSlot in newInventory.GetAllSlot())
		{
			if (currentSlot is null) continue; //슬롯이 없는데? 다음!
			//만들어서 Instance에 저장!
			GameObject instance = ObjectManager.CreateObject(itemSlotPrefabName, layout.transform);
			if(!instance) continue; //만든게 없는데?
			if(instance.TryGetComponent(out UI_ItemSlotInfo createdSlot)) //슬롯이 아닌데?
			{
				createdSlot.ConnectSlot(currentSlot);
			}
		}
	}

	public void DisconnectInventory()
	{
		if(!layout) return;
		//아직 집 안에 자식이 남아있는 동안
		//자식을 죽인다.
		while(layout.transform.childCount > 0)
		{
			Transform targetChild = layout.transform.GetChild(0);
			targetChild.SetParent(null);
			ObjectManager.DestroyObject(targetChild.gameObject);
		}
	}
}
