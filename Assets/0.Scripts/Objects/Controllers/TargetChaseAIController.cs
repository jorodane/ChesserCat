using UnityEngine;

public class TargetChaseAIController : AIController
{
	//빙의가 된 순간부터
	protected override void OnPossess(CharacterBase newCharacter)
	{
		//생각하기
		GameManager.OnUpdateController -= ThinkWithTime;
		GameManager.OnUpdateController += ThinkWithTime;
	}

	//빙의가 해제되면
	protected override void OnUnpossess(CharacterBase oldCharacter)
	{
		//생각하는 것을 그만두기
		GameManager.OnUpdateController -= ThinkWithTime;
	}


	[SerializeField] GameObject _focusTarget = null;
	public GameObject FocusTarget => _focusTarget;
	public GameObject SetFocusTarget(GameObject newTarget)
	{
		if (IsFocussable(newTarget)) //포커스 지정이 가능한 대상이라면
		{
			_focusTarget = newTarget;//바꾸고
			OnFocusTargetChanged(FocusTarget, newTarget);//바뀌었을 때 할 일을 해놓기!
		}

		return FocusTarget; //결과를 돌려주기
	}

	protected virtual bool IsFocussable(GameObject target) => target != _focusTarget;

	protected virtual void OnFocusTargetChanged(GameObject oldTarget, GameObject newTarget)
	{

	}

	protected void ThinkWithTime(float deltaTime)
	{
		if (!FocusTarget) return; //대상이 없으면 안함
		CommandMoveToDestination(FocusTarget.transform.position, 1.0f); //대상의 위치로 이동
	}

	protected override void Think()
	{

	}
}