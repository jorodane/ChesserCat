using UnityEngine;

public class UI_Button_ActionInfo : UI_Button_PlayAction
{

	protected override void OnActivated()
	{
		base.OnActivated();
		InputManager.ClaimCommandInfo(true);
	}
}
