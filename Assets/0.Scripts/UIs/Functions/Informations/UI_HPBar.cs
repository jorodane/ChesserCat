using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_HPBar : CharacterTargetUIBase
{
    public event Action OnAnimated;

	[SerializeField] TextMeshProUGUI hpAsText;
	[SerializeField] TextMeshProUGUI deltaAsText;
	[SerializeField] Slider hpAsSlider;
	[SerializeField] Slider damageAsSlider;
	[SerializeField] Slider healAsSlider;
    [SerializeField] Animator anim;
	HitPointModule targetHP;

    [SerializeField] Vector2 detailedSize;
    [SerializeField] Vector2 simplifiedSize;

    int _currentDelta;
	public int CurrentDelta => _currentDelta;
	public int CurrentHP => targetHP? targetHP.Current : 0;


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

	private void RefreshHP(in FillValue value, int delta, bool isAnimation)
	{
        if(isAnimation && anim)
        {
			if(value.IsEmpty)
			{
				anim.SetTrigger("HPDestroy");
			}
			else
			{
				anim.SetInteger("Delta", delta);
				anim.SetTrigger("HPChange");
			}
            OnAnimated?.Invoke();
        }

        if(deltaAsText)
        {
            if(delta == 0) deltaAsText.gameObject.SetActive(false);
            else
            {
                deltaAsText.gameObject.SetActive(true);
                deltaAsText.SetText($"{Mathf.Abs(delta)}");
            }
        }
		hpAsText.SetText($"{value.GetCurrent()}/{value.Max}");
		hpAsSlider.value = value.Percent;
    }

    public void SetDelta(int delta)
    {
        _currentDelta = delta;
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

    public void AddDelta(int value) => SetDelta(_currentDelta + value);

	public override void Refresh()
	{
		if(targetHP)
		{
			gameObject.SetActive(true);
			RefreshHP(targetHP.fill, 0, false);
		}
		else
		{
			gameObject.SetActive(false);
		}
	}

    public void SetSimple(bool value)
    {
        if (transform is RectTransform hpRect)
        {
            hpRect.sizeDelta = value ? simplifiedSize : detailedSize;
        }
        hpAsText.gameObject.SetActive(!value);
    }
}
