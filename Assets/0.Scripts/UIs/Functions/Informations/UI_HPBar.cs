using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

public class UI_HPBar : CharacterTargetUIBase
{
	[SerializeField] TextMeshProUGUI hpAsText;
	[SerializeField] Slider hpAsSlider;
	HitPointModule targetHP;
	protected override void OnConnected(CharacterBase target)
	{
		targetHP = target.GetModule<HitPointModule>();
		if (targetHP)
		{
			targetHP.fill.OnChanged -= RefreshHP;
			targetHP.fill.OnChanged += RefreshHP;
		}
		Refresh();
	}

	protected override void OnDisconnect(CharacterBase target)
	{
		if(targetHP) targetHP.fill.OnChanged -= RefreshHP;
		Refresh();
	}

	private void RefreshHP(in FillValue value)
	{
		hpAsText.SetText($"{value.Current}/{value.Max}");
		hpAsSlider.value = value.Percent;
	}

	public override void Refresh()
	{
		if(targetHP)
		{
			gameObject.SetActive(true);
			RefreshHP(targetHP.fill);
		}
		else
		{
			gameObject.SetActive(false);
		}
	}

}
