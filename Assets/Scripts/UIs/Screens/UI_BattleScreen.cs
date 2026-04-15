using UnityEngine;

public class UI_BattleScreen : UI_ScreenBase
{
	void OnEnable()
	{
		InputManager.OnCancel -= CancelMenu;
		InputManager.OnCancel += CancelMenu;
	}

	void OnDisable()
	{
		InputManager.OnCancel -= CancelMenu;
	}

	void CancelMenu(bool value)
	{
		//if(UIManager.IsOpen(UIType.Resign))
		if (UIManager.ClaimGetUI(UIType.Resign).isActiveAndEnabled)
		{
			UIManager.ClaimCloseUI(UIType.Resign);
		}
		else
		{
			UIManager.ClaimToggleUI(UIType.Menu);
		}
	}
}
