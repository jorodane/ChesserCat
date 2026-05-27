using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ItemSlotInfo : UIBase
{
	[SerializeField] Image iconImage;
	[SerializeField] TextMeshProUGUI amountText;

	[SerializeField] Sprite noneIcon;

	ItemSlot connectedSlot;

	public void ConnectSlot(ItemSlot targetSlot)
	{
		if (targetSlot is null) return;
		connectedSlot = targetSlot;
		VisualUpdate(connectedSlot);
	}

	protected virtual void VisualUpdate(ItemSlot targetSlot)
	{
		if (targetSlot is null) return;
		ItemContainer targetItem = targetSlot.GetItem();
		if (iconImage)
		{
			if(targetItem)
			{
				//            targetItem의 아이콘 없으면 noneIcon
				iconImage.sprite = targetItem.icon ?? noneIcon;
				iconImage.enabled = true; //아이템이 있어야 이미지가 켜짐!
			}
			else
			{
				iconImage.enabled = false; //아이템이 없으면 이미지를 끄기!
			}
		}
		if (amountText)
		{
			int targetStack = targetSlot.GetStack();
			if(!targetItem || targetItem.maxStack <= 1 || targetStack <= 0)
			{
				amountText.SetText("");
			}
			else
			{
				//bool isMax = targetSlot.GetMax(); //너, 다 찬거니?
				//if(isMax) amountText.color = Color.yellow;
				//else	    amountText.color = Color.white;
				amountText.SetText($"{targetStack}");
			}
		}
	}
}
