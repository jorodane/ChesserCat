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
		if (!UIManager.ClaimCloseUI(UIType.Option, UIType.Resign, UIType.Map, UIType.OutBox, UIType.Dictionary, UIType.Info))
		{
			UIManager.ClaimToggleUI(UIType.Menu);
		}
	}
}
