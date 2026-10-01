using UnityEngine;
using UnityEngine.UI;

public class UI_Button_StageStart : UIBase
{
	[SerializeField] Button mainButton;
	StageBase receivedStage;

	public override void Registration(UIManager manager)
	{
		OnSelectStage(null);
		base.Registration(manager);
		InputManager.OnSelectStage -= OnSelectStage;
		InputManager.OnSelectStage += OnSelectStage;
	}

	public override void Unregistration(UIManager manager)
	{
		base.Unregistration(manager);
		InputManager.OnSelectStage -= OnSelectStage;
	}

	void OnSelectStage(StageBase value)
	{
		mainButton.interactable = receivedStage = value;
	}

	public void StageStart()
	{
		if (!receivedStage) return;
		void LoadStage()
		{
			BoardBase loadedBoard = DataManager.LoadDataFile<BoardBase>(receivedStage.boardID);
			GameManager.Tile?.LoadBoard(loadedBoard);
			BattleManager.ClaimStartBattleFromStage(receivedStage);
		}
		UIManager.ClaimOpenScreen(ScreenType.Battle, ScreenChangeType.FadeChanger, LoadStage);

	}
}
