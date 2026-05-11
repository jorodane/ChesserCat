using UnityEngine;
using TMPro;

public class UI_TargetNameTag : CharacterTargetUIBase
{
	[SerializeField] TextMeshProUGUI nameText;

	protected override void OnConnected(CharacterBase target)
	{
		target.OnNameChanged -= RefreshName;
		target.OnNameChanged += RefreshName;
		Refresh();
	}
	protected override void OnDisconnect(CharacterBase target)
	{
		target.OnNameChanged -= RefreshName;
		Refresh();
	}

	public override void Refresh()
	{
		if(ConnectedCharacter)
		{
			gameObject.SetActive(true);
			RefreshName(ConnectedCharacter.DisplayName);
		}
		else
		{
			gameObject.SetActive(false);
		}
	}

	void RefreshName(in string name)
	{
		nameText.SetText(name);
	}
}
