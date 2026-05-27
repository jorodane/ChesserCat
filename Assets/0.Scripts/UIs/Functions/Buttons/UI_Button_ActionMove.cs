using UnityEngine;

public class UI_Button_ActionMove : UI_Button_PlayAction
{

	protected override void OnActivated()
	{
		base.OnActivated();
		InputManager.ClaimCommandMove(true);
	}
}
