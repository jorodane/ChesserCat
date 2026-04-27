using UnityEngine;

public abstract class AIController : ControllerBase
{
	[SerializeField] GameObject _focusTarget = null;
	public GameObject FocusTarget => _focusTarget;

	protected abstract void Think(float deltaTime);

	public GameObject SetFocusTarget(GameObject newTarget)
	{
		if(IsFocussable(newTarget)) //포커스 지정이 가능한 대상이라면
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
}
