using System.Collections.Generic;
using UnityEngine;

public delegate void MovementEvent(Vector3 move);
public delegate void LookAtEvent(Vector3 direction);
//                             실제 데미지를 "제공"한 사물      데미지를 주라고 시킨 놈
public delegate void DamageEvent(GameObject damageCauser, ControllerBase instigator, float damage);

public class CharacterBase : MonoBehaviour
{
	public event MovementEvent	OnMovement;
	public void MovementNotify(Vector3 move) => OnMovement?.Invoke(move);

	public event LookAtEvent	OnLookAt;
	public void LookAtNotify(Vector3 direction) => OnLookAt?.Invoke(direction);

	public event DamageEvent	OnDamage;
	public void DamageNotify(GameObject damageCauser, ControllerBase instigator, float damage)
		=> OnDamage?.Invoke(damageCauser, instigator, damage);

	//가장 중요한 기능!
	//말을 했을 때 말을 잘 들어먹는 것
	ControllerBase _controller;
	public ControllerBase Controller => _controller;

	protected Vector3 _lookRotation;
	public Vector3 LookRotation => _lookRotation;

	public virtual string DisplayName => "character";

	//모듈을 저장해놓기!
	//List : 추가/제거가 쉽다 <-> 메모리 효율이 낮고, 전체 순환이 느리다
	//           많고                                    적고
	//Array: 추가/제거가 어렵고 <-> 메모리 효율이 높고, 전체 순환이 빠르다
	//           적고                                    많다
	Dictionary<System.Type, CharacterModule> moduleDictionary = new();

	// 추가 / 제거 / 검색
	public void AddModule(System.Type wantType, CharacterModule wantModule)
	{
		if(moduleDictionary.TryAdd(wantType, wantModule))
		{//추가하는 데에 성공했으니까
			wantModule.OnRegistration(this); 
			//등록하는 것도 발동!
		}
	}
	public void AddAllModuleFromObject(GameObject target)
	{
		if (!target) return;

		foreach(CharacterModule currentModule in target.GetComponentsInChildren<CharacterModule>())
		{
			//           이 친구의 대분류 타입,          이 친구
			AddModule(currentModule.RegistrationType, currentModule);
		}
	}
	public void RemoveModule(System.Type wantType) 
	{
		//                      대상 타입이 있으면
		if (moduleDictionary.ContainsKey(wantType))
		{
			moduleDictionary[wantType]?.OnUnregistration(this); //넌 이제 해제된 거야
			moduleDictionary.Remove(wantType); //그 다음에 제거하기!
		}
	}
	public void RemoveAllModule()
	{
		// A B C D E      A B C D E
		// 0              0
		// B C D E        F A B C D E
		//   1              1
		// B D E          G F A B C D E
		//     2              2
		//자료구조를 도는 동안 그 안에 있는 내용물을 바꾸면 문제가 생김!
		foreach (CharacterModule currentModule in moduleDictionary.Values)
		{
			//             해제를 했다고 말해놓고
			currentModule.OnUnregistration(this);
		}
		//끝나고 나서 없애기!
		moduleDictionary.Clear();
	}
	public T GetModule<T>() where T : CharacterModule
	{
		//위에는 매개변수가 있었는데 아래에는 매개변수가 없는 이유
		moduleDictionary.TryGetValue(typeof(T), out CharacterModule result);
		return result as T;
	}

	//                    빙의되다
	protected virtual void OnPossessed(ControllerBase newController){}
	public ControllerBase Possessed(ControllerBase from)
	{
		//빙의를 하려고 했는데 원래 영혼이 들어있었어요
		//영혼이 있더라도 빙의가 가능
		//영혼이 있으면 튕겨냈다!
		if (Controller) Unpossessed();
		_controller = from;
		AddAllModuleFromObject(gameObject); // 모듈을 전부 붙여놓고
		OnPossessed(Controller); //그 다음에 복종하러 가기
		return Controller;
	}
	
	//          혼이 나가다
	protected virtual void OnUnpossessed(ControllerBase oldController){}
	public void Unpossessed()
	{
		if(Controller) OnUnpossessed(Controller); //빙의 해제되고 나서
		RemoveAllModule(); //그 다음에 모듈을 해제하기!
		_controller = null;
	}
	public bool Unpossessed(ControllerBase oldController)
	{
		if (Controller != oldController) return false;
		Unpossessed();
		return true;
	}

}
