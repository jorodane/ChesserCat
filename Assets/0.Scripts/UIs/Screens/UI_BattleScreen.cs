using UnityEngine;

public class UI_BattleScreen : UI_ScreenBase
{
	[SerializeField] UI_PlayerCharacterInfo playerCharacterInfo;
    [SerializeField] UI_TurnShower turnShower;
    [SerializeField] UI_IngameAreaVisalizer ingameCover;

	void OnEnable()
	{
		InputManager.OnCancel -= CancelMenu;
		InputManager.OnCancel += CancelMenu;
		playerCharacterInfo?.Connect(PlayerController.Instance);
        turnShower.Registration(UIManager.instance);
        UIManager.ClaimSetUI(ingameCover, UIType.IngameCover);
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
                if (PlayerController.Instance)
                {
                    PlayerController.Instance.ReselectCurrentCharacter(true);
                }
            }
        }
		else if (!CloseInnerUI()) UIManager.ClaimOpenUI(UIType.Menu);
	}
}
