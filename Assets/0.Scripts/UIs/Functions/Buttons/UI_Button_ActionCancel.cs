using UnityEngine;

public class UI_Button_ActionCancel : UI_Button_PlayAction
{

	protected override void OnActivated()
	{
		base.OnActivated();
		InputManager.ClaimCommandCancel(true);
	}
}
