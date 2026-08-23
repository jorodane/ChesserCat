using UnityEngine;

public class CharacterModule : MonoBehaviour
{
	//MovementModule
	//ChessTileModule
	//저는 한 칸씩 움직여야 할 거고 => 체스움직임
	//MovementModule을 찾아온 사람이 있음
	//ChessTileModule이 있으면? 이거를 보고 갈까?
	//나의 "대분류"를 저장하는 방법!
	public virtual System.Type RegistrationType => typeof(CharacterModule);

	CharacterBase _owner;
	public CharacterBase Owner => _owner;

	//모듈이 캐릭터에 부착되었을 때!
	public virtual void OnRegistration(CharacterBase newOwner) { _owner = newOwner; }
	//모듈이 캐릭터에서 분리되었을 때!
	public virtual void OnUnregistration(CharacterBase oldOwner) { _owner = null; }

	public virtual void ApplySetting(CharacterBaseSetting setting) { }
}
