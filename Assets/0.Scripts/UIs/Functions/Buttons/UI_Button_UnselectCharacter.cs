using UnityEngine;

public class UI_Button_UnselectCharacter : UI_Button_PlayAction
{

	public override void Activate()
	{
		base.Activate();
		PlayerController.Instance?.Unselect(ConnectedCharacter);
	}
}
