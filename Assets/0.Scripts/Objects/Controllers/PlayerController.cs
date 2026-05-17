using System;
using UnityEngine;

public delegate void CursorHoveredTilePositionChange(Vector3Int from, Vector3Int to);

public class PlayerController : ControllerBase
{
	static PlayerController _instance;
	public static PlayerController Instance => _instance;

	public static event CursorHoveredTilePositionChange OnCursorHoveredTilePositionChanged;

	int lastSelected = -1;

	Vector3Int currentTilePosition;
	Vector3Int clickedTilePosition;

	public override void RegistrationFunctions()
	{
		base.RegistrationFunctions();
		InputManager.OnMouseMove -= MoveCursor;
		InputManager.OnMouseMove += MoveCursor;
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
		if (!Instance) _instance = this;
	}

	public override void UnregistrationFunctions()
	{
		base.UnregistrationFunctions();
		InputManager.OnMouseMove -= MoveCursor;
		InputManager.OnMouseLeftButton -= SelectUnderCursor;
		InputManager.OnCommandClearGuide -= GuideClear;
		InputManager.OnSelectByNumber -= SelectByNumber;
		InputManager.OnSelectNext -= SelectNext;
		InputManager.OnSelectPrev -= SelectPrev;
		InputManager.OnMove -= MoveToDirection;
		InputManager.OnMouseRightButton -= GuideUnderCursor;
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
		if (value < 0 || value >= Characters.Count)
		{
			UnselectCurrentCharacter(true);
		}
		else
		{
			Select(Characters[value]);
		}
		lastSelected = value;
	}

	void MoveCursor(Vector2 screenPosition, Vector3 worldPosition)
	{
		Vector3Int lastTilePosition = currentTilePosition;
		currentTilePosition = TileManager.GetTileCellPosition(worldPosition);
		if (lastTilePosition != currentTilePosition) OnCursorHoveredTilePositionChanged(lastTilePosition, currentTilePosition);
	}

	void SelectUnderCursor(bool value, Vector2 screenPosition, Vector3 worldPosition)
	{
		if(!InputManager.IsCursorHoverOnUI)	Select(InputManager.CursorHoverSelectable);
	}


	void GuideUnderCursor(bool value, Vector2 screenPosition, Vector3 worldPosition)
	{
		if (value)
		{
			clickedTilePosition = currentTilePosition;
		}
		else 
		{
			TileManager.ClaimCreateGuideLine(clickedTilePosition, currentTilePosition);
		}
	}

	void GuideClear(bool value)
	{
		TileManager.ClaimClearGuideLine();
	}

	protected override void OnSelect(ISelectable newTarget)
	{
		base.OnSelect(newTarget);
		OpenCharacterClickInfo(SelectedCharacter);
	}

	protected override void OnReselect(ISelectable newTarget)
	{
		base.OnReselect(newTarget);
		OpenCharacterClickInfo(SelectedCharacter);
	}

	protected override void OnUnselect(ISelectable oldTarget)
	{
		base.OnUnselect(oldTarget);
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
}
