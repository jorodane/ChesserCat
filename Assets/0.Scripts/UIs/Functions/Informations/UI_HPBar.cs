using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

public class UI_HPBar : CharacterTargetUIBase
{
	[SerializeField] TextMeshProUGUI hpAsText;
	[SerializeField] Slider hpAsSlider;
	[SerializeField] Slider damageAsSlider;
	[SerializeField] Slider healAsSlider;
	HitPointModule targetHP;
    int currentDelta;
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
	protected override void OnDisconnected(CharacterBase target)
	{
		if(targetHP) targetHP.fill.OnChanged -= RefreshHP;
		Refresh();
	}

	private void RefreshHP(in FillValue value)
	{
		hpAsText.SetText($"{value.Current}/{value.Max}");
		hpAsSlider.value = value.Percent;
    }

    public void SetDelta(int delta)
    {
        currentDelta = delta;
        if (delta > 0)
        {
            healAsSlider.value = (targetHP.Current + delta) / (float)targetHP.Max;
            healAsSlider.gameObject.SetActive(true);
        }
        else
        {
            healAsSlider.gameObject.SetActive(false);
        }

        if (delta < 0)
        {
            damageAsSlider.value = (-delta / (float)targetHP.Max) / hpAsSlider.value;
            damageAsSlider.gameObject.SetActive(true);
        }
        else
        {
            damageAsSlider.gameObject.SetActive(false);
        }
    }

    public void AddDelta(int value) => SetDelta(currentDelta + value);

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
