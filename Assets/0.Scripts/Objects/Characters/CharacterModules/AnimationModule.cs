using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class AnimationModule : CharacterModule
{
	//클래스간의 결합
	//is - a 관계 : 상속관계
	//has - a 관계 : 소유관계 MovementModule movementModule; 
	[SerializeField] Animator anim;
	[SerializeField] bool isRotationByMovement;

	public sealed override System.Type RegistrationType => typeof(AnimationModule);

	public override void OnRegistration(CharacterBase newOwner)
	{
		base.OnRegistration(newOwner);
		newOwner.OnLookAt -= AnimationByLookRotation;
		newOwner.OnLookAt += AnimationByLookRotation;
		newOwner.OnMovement -= AnimationByMovement;
		newOwner.OnMovement += AnimationByMovement;
	}

	public override void OnUnregistration(CharacterBase oldOwner)
	{
		base.OnUnregistration(oldOwner);
		oldOwner.OnLookAt -= AnimationByLookRotation;
		oldOwner.OnMovement -= AnimationByMovement;
	}

	public void AnimationByLookRotation(Vector3 lookRotation)
	{
		if (!anim) return;
		anim.SetFloat("MoveX", lookRotation.x);
		anim.SetFloat("MoveY", lookRotation.y);
	}

	public void AnimationByMovement(Vector3 moveDelta)
	{
		if (!anim) return;
		if(isRotationByMovement && moveDelta.sqrMagnitude > 0)
		{
			AnimationByLookRotation(moveDelta);
		}
		anim.SetFloat("MoveSpeed", moveDelta.magnitude / Time.fixedDeltaTime);
	}
}
