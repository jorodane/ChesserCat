using System;
using UnityEngine;

public class PlayerController : ControllerBase
{
	static PlayerController _instance;
	public static PlayerController Instance => _instance;

	int lastSelected = -1;

	public override void RegistrationFunctions()
	{
		base.RegistrationFunctions();
		InputManager.OnMouseLeftButton -= SelectUnderCursor;
		InputManager.OnMouseLeftButton += SelectUnderCursor;
		InputManager.OnSelectByNumber -= SelectByNumber;
		InputManager.OnSelectByNumber += SelectByNumber;
		InputManager.OnSelectNext -= SelectNext;
		InputManager.OnSelectNext += SelectNext;
		InputManager.OnSelectPrev -= SelectPrev;
		InputManager.OnSelectPrev += SelectPrev;
		InputManager.OnMouseRightButton -= MoveToMousePosition;
		InputManager.OnMouseRightButton += MoveToMousePosition;
		if (!Instance) _instance = this;
	}

	public override void UnregistrationFunctions()
	{
		base.UnregistrationFunctions();
		InputManager.OnMouseLeftButton -= SelectUnderCursor;
		InputManager.OnSelectByNumber -= SelectByNumber;
		InputManager.OnSelectNext -= SelectNext;
		InputManager.OnSelectPrev -= SelectPrev;
		InputManager.OnMove -= MoveToDirection;
		InputManager.OnMouseRightButton -= MoveToMousePosition;
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

	void SelectUnderCursor(bool value, Vector2 screenPosition, Vector3 worldPosition)
	{
		if(!InputManager.IsCursorHoverOnUI)	Select(InputManager.CursorHoverSelectable);
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
