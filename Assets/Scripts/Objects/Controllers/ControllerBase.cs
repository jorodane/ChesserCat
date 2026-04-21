using NUnit.Framework.Constraints;
using UnityEngine;

public class ControllerBase : MonoBehaviour, IFunctionable
{
	CharacterBase _character;
	public CharacterBase Character => _character;

	//테스트용! 나중에 지울거임!
	void Start()
	{
		//게임매니저한테 초기화 신청하기!		  함수 등록을 대신 해주세요!
		GameManager.OnInitializeController += RegistrationFunctions;
	}

	public virtual void RegistrationFunctions()
	{
		//나랑 같은 오브젝트에 들어있는 캐릭터에 빙의하고 싶다!
		Possess(GetComponent<CharacterBase>());
	}

	public virtual void UnregistrationFunctions()
	{
		Unpossess();
	}

	protected virtual void OnPossess(CharacterBase newCharacter) { }
    public void Possess(CharacterBase target)
	{
		if (!target) return; //대상이 없습니다.
		//        빙의된컨트롤러             빙의   내가 너에게 가겠다
		ControllerBase result = target.Possessed(this);
		//내가 당첨되었어! => 제대로 빙의가 된 거구나!
		if (result == this)
		{
			_character = target;
			OnPossess(target);
		}
	}

	protected virtual void OnUnpossess(CharacterBase oldCharacter) { }
	public void Unpossess()
	{
		if(Character)
		{
			//이미 주인이 바뀌었다면?
			//제가 원래 살던 집을 팔거예요
			//집주인이 이미 바뀐 상태
			//이 상태에서 집을 팝니다. => 팔렸다고 가정
			//                      그래서 Unpossess할 대상을 찾아놓기!
			if(Character.Unpossessed(this))
			{
				OnUnpossess(Character);
			}
		}
		_character = null;
	}

	public void CommandMoveToDirection(Vector3 direction)
	{
		if(Character is IRunnable target) target.MoveToDirection(direction);
	}

	public void CommandMoveToDestination(Vector3 destination, float tolerance)
	{
		if(Character is IRunnable target) target.MoveToDestination(destination, tolerance);
	}

	public void CommandStop()
	{
		if (Character is IRunnable target) target.StopMovement();
	}
}
