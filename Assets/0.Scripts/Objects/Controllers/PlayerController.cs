using System;
using UnityEngine;
using static UnityEditor.FilePathAttribute;



public class PlayerController : ControllerBase
{
	static PlayerController _instance;
	public static PlayerController Instance => _instance;

	

	int lastSelected = -1;

	Vector3Int clickedTilePosition; 

	public override void RegistrationFunctions()
	{
		base.RegistrationFunctions();
		InputManager.OnMouseLeftButton -= SelectUnderCursor;
		InputManager.OnMouseLeftButton += SelectUnderCursor;
		InputManager.OnMouseRightButton -= GuideUnderCursor;
		InputManager.OnMouseRightButton += GuideUnderCursor;
		InputManager.OnCommandClearGuide -= GuideClear;
		InputManager.OnCommandClearGuide += GuideClear;
		InputManager.OnSelectByNumber -= SelectByNumber;
		InputManager.OnSelectByNumber += SelectByNumber;
		InputManager.OnSelectNext -= SelectNext;
		InputManager.OnSelectNext += SelectNext;
		InputManager.OnSelectPrev -= SelectPrev;
		InputManager.OnSelectPrev += SelectPrev;
		InputManager.OnCommandMove -= CommandMove;
		InputManager.OnCommandMove += CommandMove;
		InputManager.OnCommandAttack -= CommandAttack;
		InputManager.OnCommandAttack += CommandAttack;
		InputManager.OnCommandCancel -= CommandCancel;
		InputManager.OnCommandCancel += CommandCancel;
		InputManager.OnCommandInfo -= CommandInfo;
		InputManager.OnCommandInfo += CommandInfo;
		GameManager.OnInitializeCharacter += Place;
		if (!Instance) _instance = this;
	}

	public override void UnregistrationFunctions()
	{
		base.UnregistrationFunctions();
		InputManager.OnMouseLeftButton -= SelectUnderCursor;
		InputManager.OnMouseRightButton -= GuideUnderCursor;
		InputManager.OnCommandClearGuide -= GuideClear;
		InputManager.OnSelectByNumber -= SelectByNumber;
		InputManager.OnSelectNext -= SelectNext;
		InputManager.OnSelectPrev -= SelectPrev;
		InputManager.OnCommandMove -= CommandMove;
		InputManager.OnCommandAttack -= CommandAttack;
		InputManager.OnCommandCancel -= CommandCancel;
		InputManager.OnCommandInfo -= CommandInfo;
    }

    private void SelectPrev(bool value)
	{
		if(lastSelected < 0) SelectByNumber(0);
		else SelectByNumber((lastSelected - 1 + Characters.Count) % Characters.Count);
	}

	private void SelectNext(bool value)
	{
		if(lastSelected < 0) SelectByNumber(0);
		else SelectByNumber((lastSelected + 1) % Characters.Count);
	}

	void SelectByNumber(int value)
	{
		if (GameManager.IsPaused) return;
		if (!Characters.IsValidRange(value))
		{
			UnselectCurrentCharacter(true);
		}
		else
		{
			Select(Characters[value]);
		}
		lastSelected = value;
	}

	void SelectUnderCursor(bool value, Vector2 screenPosition, Vector3 worldPosition)
	{
        if (!InputManager.IsCursorHoverOnUI)
        {
            if(SelectedCharacter && TileManager.IsWaitInput())
            {
                CommandMoveToDestination(worldPosition, 0.1f);
                Unselect(SelectedCharacter);
            }
            else
            {
                Select(InputManager.CursorHoverSelectable);
            }
        }
	}


	void GuideUnderCursor(bool value, Vector2 screenPosition, Vector3 worldPosition)
	{
		if (value)
		{
			clickedTilePosition = TileManager.GetTileCellPosition(worldPosition);
			if(SelectedCharacter) TileManager.PlaceObjectOnTile(SelectedCharacter.gameObject, clickedTilePosition);
		}
		else 
		{
			TileManager.ClaimCreateGuideLine(clickedTilePosition, TileManager.GetTileCellPosition(worldPosition));
		}
	}

	void GuideClear(bool value)
	{
		TileManager.ClaimClearGuideLine();
	}


	public virtual void CommandInfo(bool value)
	{
		if (!UIManager.ClaimCheckOpen(UIType.CharacterClickInfo)) return;
		UIManager.ClaimCloseUI(UIType.CharacterClickInfo);
	}

	public virtual void CommandAttack(bool value)
	{
		if (!UIManager.ClaimCheckOpen(UIType.CharacterClickInfo)) return;
		UIManager.ClaimCloseUI(UIType.CharacterClickInfo);
		TileManager.StartCharacterAttackInput(SelectedCharacter);
	}

	public virtual void CommandMove(bool value)
	{
		if (!UIManager.ClaimCheckOpen(UIType.CharacterClickInfo)) return;
		UIManager.ClaimCloseUI(UIType.CharacterClickInfo);
		TileManager.StartCharacterMoveInput(SelectedCharacter);
	}

	public virtual void CommandCancel(bool value)
	{
		if (!UIManager.ClaimCheckOpen(UIType.CharacterClickInfo)) return;
		Unselect(SelectTarget);
	}

    protected override void OnSelect(ISelectable newTarget)
	{
		base.OnSelect(newTarget);
		TileManager.EndInput();
		OpenCharacterClickInfo(SelectedCharacter);
	}

	protected override void OnReselect(ISelectable newTarget)
	{
		base.OnReselect(newTarget);
		TileManager.EndInput();
		OpenCharacterClickInfo(SelectedCharacter);
	}

	protected override void OnUnselect(ISelectable oldTarget)
	{
		base.OnUnselect(oldTarget);
		TileManager.EndInput();
		UIManager.ClaimCloseUI(UIType.CharacterClickInfo);
	}

	public void MoveToMousePosition(bool value, Vector2 screenPosition, Vector3 worldPosition)
	{
		if(value) CommandMoveToDestination(worldPosition, 0.0f);
	}

	public void MoveToDirection(Vector2 value)
	{
		CommandMoveToDirection(value);
	}

	public GameObject SpawnPiece(string wantName)
	{
		GameObject Result = ObjectManager.CreateObject(wantName);

		if (!Result) return Result;
		if(Result.TryGetComponent(out CharacterBase spawnedCharacter)) Possess(spawnedCharacter);
		return Result;
	}

	void Place()
	{
		TileManager.PlaceObjectOnTile(SpawnPiece("SamplePiece_Rook"),	new Vector3Int(0, 0));
		TileManager.PlaceObjectOnTile(SpawnPiece("SamplePiece_Knight"),	new Vector3Int(1, 0));
		TileManager.PlaceObjectOnTile(SpawnPiece("SamplePiece_Bishop"),	new Vector3Int(2, 0));
		TileManager.PlaceObjectOnTile(SpawnPiece("SamplePiece_Queen"),	new Vector3Int(3, 0));
		TileManager.PlaceObjectOnTile(SpawnPiece("SamplePiece_King"),	new Vector3Int(4, 0));
		TileManager.PlaceObjectOnTile(SpawnPiece("SamplePiece_Bishop"),	new Vector3Int(5, 0));
		TileManager.PlaceObjectOnTile(SpawnPiece("SamplePiece_Knight"),	new Vector3Int(6, 0));
		TileManager.PlaceObjectOnTile(SpawnPiece("SamplePiece_Rook"),	new Vector3Int(7, 0));
		for(int i = 0; i < 8; i++) TileManager.PlaceObjectOnTile(SpawnPiece("SamplePiece_Pawn"), new Vector3Int(i,1));
	}
}
