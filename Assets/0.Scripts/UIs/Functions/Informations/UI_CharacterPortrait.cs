using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

public class UI_CharacterPortrait : CharacterTargetUIBase
{
	[SerializeField] Image imageRender;

	AnimationModule targetAnimation;
	SpriteRenderer targetRenderer;

	protected override void OnConnected(CharacterBase target)
	{
		targetAnimation = target.GetModule<AnimationModule>();
		if (targetAnimation)
		{
			targetRenderer = targetAnimation.GetComponent<SpriteRenderer>();
			GameManager.OnUpdateUI -= RefreshUpdate;
			GameManager.OnUpdateUI += RefreshUpdate;
		}
		Refresh();
	}
	protected override void OnDisconnected(CharacterBase target)
	{
		targetAnimation = null;
		targetRenderer = null;
		GameManager.OnUpdateUI -= RefreshUpdate;
		Refresh();
	}

	public void RefreshUpdate(float deltaTime) => Refresh();
	public override void Refresh()
	{
		if(targetRenderer)
		{
			imageRender.sprite = targetRenderer.sprite;
			imageRender.enabled = true;
		}
		else
		{
			imageRender.enabled = false;
		}
	}

}
