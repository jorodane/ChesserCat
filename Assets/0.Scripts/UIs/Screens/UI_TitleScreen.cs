using UnityEngine;

public class UI_TitleScreen : UI_ScreenBase
{
	private void OnEnable()
	{
		InputManager.OnCancel -= QuitToggle;
		InputManager.OnCancel += QuitToggle;
	}

	private void OnDisable()
	{
		InputManager.OnCancel -= QuitToggle;
	}

	void QuitToggle(bool value)
	{
		UIManager.ClaimToggleUI(UIType.GameQuit);
	}
}
