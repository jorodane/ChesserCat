using TMPro;
using UnityEngine;

public class UI_CharacterQuickInfo : CharacterTargetUIBase
{
	[SerializeField] TextMeshProUGUI numberText;
	[SerializeField] UI_HPBar hpBar;
	[SerializeField] UI_CharacterPortrait portrait;

	int currentIndex = -1;

	protected override void OnConnected(CharacterBase target)
	{
		hpBar.Connect(target);
		portrait.Connect(target);
		currentIndex = transform.GetSiblingIndex();
		Refresh();
	}

	protected override void OnDisconnected(CharacterBase target)
	{
		hpBar.Disconnect(target);
		portrait.Disconnect(target);
		Refresh();
	}

	public override void Refresh()
	{
		numberText.SetText($"{currentIndex + 1}");
		hpBar.Refresh();
		portrait.Refresh();
	}

	public void SelectTarget()
	{
		InputManager.ClaimSelectByNumber(currentIndex);
	}
}
