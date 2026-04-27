using UnityEngine;

public class ChaseAIController : AIController
{
	//빙의가 된 순간부터
	protected override void OnPossess(CharacterBase newCharacter)
	{
		//생각하기
		GameManager.OnUpdateController -= Think;
		GameManager.OnUpdateController += Think;
	}

	//빙의가 해제되면
	protected override void OnUnpossess(CharacterBase oldCharacter)
	{
		//생각하는 것을 그만두기
		GameManager.OnUpdateController -= Think;
	}

	protected override void Think(float deltaTime)
	{
		if (!FocusTarget) return; //대상이 없으면 안함
		CommandMoveToDestination(FocusTarget.transform.position, 1.0f); //대상의 위치로 이동
	}
}
