using System;
using UnityEngine;

public class PlayerController : ControllerBase
{
	 static PlayerController _instance;
	public static PlayerController Instance => _instance;

	public override void RegistrationFunctions()
	{
		base.RegistrationFunctions();
		InputManager.OnMouseLeftButton -= SelectUnderCursor;
		InputManager.OnMouseLeftButton += SelectUnderCursor;
		InputManager.OnMouseRightButton -= MoveToMousePosition;
		InputManager.OnMouseRightButton += MoveToMousePosition;
		InputManager.OnMove -= MoveToDirection;
		InputManager.OnMove += MoveToDirection;
		if (!Instance) _instance = this;
	}

	public override void UnregistrationFunctions()
	{
		base.UnregistrationFunctions();
		InputManager.OnMouseRightButton -= MoveToMousePosition;
		InputManager.OnMove -= MoveToDirection;
	}

	private void SelectUnderCursor(bool value, Vector2 screenPosition, Vector3 worldPosition)
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

	public void OpenCharacterClickInfo(CharacterBase target)
	{
		if (target)
		{
			if(!UIManager.ClaimCheckOpen(UIType.CharacterClickInfo, out IOpenable clickUI))
			{
				clickUI.Open();
				if (clickUI is ICharacterConnectable asCharacterConnector) asCharacterConnector.Connect(target);
			}
		}
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
