using System;
using UnityEngine;

public class UI_BattleScreen : UI_ScreenBase
{
	[SerializeField] UI_PlayerCharacterInfo playerCharacterInfo;
    [SerializeField] UI_TurnShower turnShower;
    [SerializeField] UI_IngameAreaVisalizer ingameCover;
	[SerializeField] Animator resultAnim;

	void OnEnable()
	{
		InputManager.OnCancel -= CancelMenu;
		InputManager.OnCancel += CancelMenu;
		BattleManager.OnLocalPlayerControllerChanged -= OnLocalPlayerChanged;
		BattleManager.OnLocalPlayerControllerChanged += OnLocalPlayerChanged;
		BattleManager.OnBattleStart -= OnBattleStart;
		BattleManager.OnBattleStart += OnBattleStart;
		BattleManager.OnBattleEnd -= OnBattleEnd;
		BattleManager.OnBattleEnd += OnBattleEnd;
        turnShower.Registration(UIManager.instance);
        UIManager.ClaimSetUI(ingameCover, UIType.IngameCover);
	}

	void OnDisable()
	{
		InputManager.OnCancel -= CancelMenu;
		BattleManager.OnLocalPlayerControllerChanged -= OnLocalPlayerChanged;
		BattleManager.OnBattleStart -= OnBattleStart;
		BattleManager.OnBattleEnd -= OnBattleEnd;
		turnShower.Unregistration(UIManager.instance);
	}

	void OnBattleEnd(in BattleSaveData? data, bool isPlayerWin)
	{
		if (resultAnim)
		{
			resultAnim.gameObject.SetActive(true);
			//UIManager.ClaimOpenScreen(UIType.Title, ScreenChangeType.FadeChanger);

		}
	}

	void OnBattleStart(in BattleSaveData? data)
	{
		if(resultAnim)
		{
			resultAnim.gameObject.SetActive(false);

		}

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
