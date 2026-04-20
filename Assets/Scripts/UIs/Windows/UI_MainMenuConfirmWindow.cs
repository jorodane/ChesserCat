using UnityEngine;

public class UI_MainMenuConfirmWindow : OpenableUIBase
{
	public void Cancel()  => UIManager.ClaimCloseUI(UIType.ToMainMenu);
}
