using UnityEngine;

public class UI_Button_ActionAttack : UI_Button_PlayAction
{

	protected override void OnActivated()
	{
		base.OnActivated();
		InputManager.ClaimCancel(true);
	}
}
