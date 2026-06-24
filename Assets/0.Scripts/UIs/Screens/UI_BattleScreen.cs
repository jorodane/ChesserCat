using UnityEngine;

public class UI_BattleScreen : UI_ScreenBase
{
	[SerializeField] UI_PlayerCharacterInfo playerCharacterInfo;
    [SerializeField] UI_TurnShower turnShower;
	void OnEnable()
	{
		InputManager.OnCancel -= CancelMenu;
		InputManager.OnCancel += CancelMenu;
		playerCharacterInfo?.Connect(PlayerController.Instance);
        turnShower.Registration(UIManager.instance);
	}

	void OnDisable()
	{
		InputManager.OnCancel -= CancelMenu;
		playerCharacterInfo?.Disconnect(PlayerController.Instance);
        turnShower.Unregistration(UIManager.instance);
	}

	void CancelMenu(bool value)
	{
        //if(UIManager.IsOpen(UIType.Resign))
        if (TileManager.IsWaitInput())
        {
            if(UIManager.ClaimCheckOpen(UIType.CharacterClickInfo))
            {
                if(PlayerController.Instance)
                {
                    PlayerController.Instance.UnselectCurrentCharacter(true);
                }
            }
            else
            {
                TileManager.EndInput();
                UIManager.ClaimOpenUI(UIType.CharacterClickInfo);
            }
        }
		else if (!CloseInnerUI()) UIManager.ClaimOpenUI(UIType.Menu);
	}
}
