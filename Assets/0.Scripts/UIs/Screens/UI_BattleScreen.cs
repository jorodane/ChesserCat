using UnityEngine;

public class UI_BattleScreen : UI_ScreenBase
{
	[SerializeField] UI_PlayerCharacterInfo playerCharacterInfo;
	void OnEnable()
	{
		InputManager.OnCancel -= CancelMenu;
		InputManager.OnCancel += CancelMenu;
		playerCharacterInfo?.Connect(PlayerController.Instance);
	}

	void OnDisable()
	{
		InputManager.OnCancel -= CancelMenu;
		playerCharacterInfo?.Disconnect(PlayerController.Instance);
	}

	void CancelMenu(bool value)
	{
        //if(UIManager.IsOpen(UIType.Resign))
        if (TileManager.IsWaitInput())
        {
            TileManager.EndInput();
            UIManager.ClaimOpenUI(UIType.CharacterClickInfo);
        }
		else if (!CloseInnerUI()) UIManager.ClaimOpenUI(UIType.Menu);
	}
}
