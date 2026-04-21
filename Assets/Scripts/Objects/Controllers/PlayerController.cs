using UnityEngine;

public class PlayerController : ControllerBase
{
	//빙의가 되면 내 캐릭터 생긴 거니까 키 입력 시작!
	protected override void OnPossess(CharacterBase newCharacter)
	{
		base.OnPossess(newCharacter);
		InputManager.OnMouseRightButton -= MoveToMousePosition;
		InputManager.OnMouseRightButton += MoveToMousePosition;
	}

	//해제가 되면 내 캐릭터 뺏긴 거니까 키 입력 받을 필요가 없음!
	protected override void OnUnpossess(CharacterBase oldCharacter)
	{
		base.OnUnpossess(oldCharacter);
		InputManager.OnMouseRightButton -= MoveToMousePosition;
	}

	public void MoveToMousePosition(bool value, Vector2 screenPosition, Vector3 worldPosition)
	{
		if(value) CommandMoveToDestination(worldPosition, 0.0f);
	}
}
