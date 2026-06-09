using TMPro;
using UnityEngine;

public class UI_CharacterQuickInfo : CharacterTargetUIBase
{
	[SerializeField] TextMeshProUGUI numberText;
	[SerializeField] UI_HPBar hpBar;
	[SerializeField] UI_CharacterPortrait portrait;
    [SerializeField] UI_CharacterQuickInfo pawnInfo;

    int currentIndex = -1;

	protected override void OnConnected(CharacterBase target)
	{
		hpBar.Connect(target);
		portrait.Connect(target);
		currentIndex = transform.GetSiblingIndex();
        if(pawnInfo)
        {
            if (target.Pawns.Count > 0) pawnInfo.Connect(target.Pawns[0]);
            else pawnInfo.gameObject.SetActive(false);
            pawnInfo.currentIndex = currentIndex;
        }
        Refresh();
	}

	protected override void OnDisconnected(CharacterBase target)
	{
		hpBar.Disconnect(target);
		portrait.Disconnect(target);
        if(pawnInfo)
        {
            pawnInfo.Disconnect(pawnInfo.ConnectedCharacter);
        }
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
		InputManager.ClaimSelectByCharacter(ConnectedCharacter);
	}
}
