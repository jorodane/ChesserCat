using UnityEngine;

//Pawn : 조종할 수 있지만 이동할 수 없는 캐릭터
//Character : 조종하고 이동 기능이 있는 캐릭터

public class MovableCharacter : CharacterBase
{
	[SerializeField] Animator anim;
	public void AnimationUpdate(Vector3 moveDelta)
	{
		if (!anim) return;
		anim.SetFloat("MoveX",		LookRotation.x);
		anim.SetFloat("MoveY",		LookRotation.y);
		anim.SetFloat("MoveSpeed",  moveDelta.magnitude / Time.fixedDeltaTime);
	}
}
