using System;
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
		BattleManager.OnLocalPlayerControllerChanged -= OnLocalPlayerChanged;
		BattleManager.OnLocalPlayerControllerChanged += OnLocalPlayerChanged;
        turnShower.Registration(UIManager.instance);
        UIManager.ClaimSetUI(ingameCover, UIType.IngameCover);
	}

	private void OnLocalPlayerChanged(PlayerController newController)
	{
		if (!playerCharacterInfo) return;
		if(newController)
		{
			playerCharacterInfo.Connect(newController);
		}
		else
		{
			playerCharacterInfo.Disconnect(playerCharacterInfo.ConnectedController);
		}
	}

	void OnDisable()
	{
		InputManager.OnCancel -= CancelMenu;
		BattleManager.OnLocalPlayerControllerChanged -= OnLocalPlayerChanged;
		turnShower.Unregistration(UIManager.instance);
	}

	void CancelMenu(bool value)
	{
		//if(UIManager.IsOpen(UIType.Resign))
		if (TileManager.IsWaitInput())
		{

		}
		else if (BattleManager.ClaimAnalysisModeEnd())
		{
			BattleManager.ClaimShowFinalTurn();
		}
		else if (!CloseInnerUI())
		{
			UIManager.ClaimOpenUI(UIType.Menu);
		}
	}
}
