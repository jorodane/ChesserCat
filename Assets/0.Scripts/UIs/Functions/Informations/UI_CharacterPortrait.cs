using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

public class UI_CharacterPortrait : CharacterTargetUIBase
{
	[SerializeField] Image imageRender;

	Sprite receivedIcon;

	protected override void OnConnected(CharacterBase target)
	{
		receivedIcon = target.GetIcon();
		Refresh();
	}
	protected override void OnDisconnected(CharacterBase target)
	{
		Refresh();
	}

	public void RefreshUpdate(float deltaTime) => Refresh();
	public override void Refresh()
	{
		if(receivedIcon)
		{
			imageRender.sprite = receivedIcon;
			imageRender.enabled = true;
		}
		else
		{
			imageRender.enabled = false;
		}
	}

}
