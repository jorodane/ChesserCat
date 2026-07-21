using UnityEngine;

public enum AnimationTriggerType
{ 
	Reset, JumpAttack
}


public class AnimationModule : CharacterModule
{
	//클래스간의 결합
	//is - a 관계 : 상속관계
	//has - a 관계 : 소유관계 MovementModule movementModule; 
	[SerializeField] Animator anim;
	[SerializeField] SpriteRenderer render;
	[SerializeField] bool isRotationByMovement;

	public sealed override System.Type RegistrationType => typeof(AnimationModule);

	public override void OnRegistration(CharacterBase newOwner)
	{
		base.OnRegistration(newOwner);
		if (!newOwner) return;
		newOwner.OnSelected -= AnimationBySelect;
		newOwner.OnSelected += AnimationBySelect;
		newOwner.OnLookAt	-= AnimationByLookRotation;
		newOwner.OnLookAt	+= AnimationByLookRotation;
		newOwner.OnMovement -= AnimationByMovement;
		newOwner.OnMovement += AnimationByMovement;
		newOwner.OnAnimationTrigger -= AnimationByTrigger;
		newOwner.OnAnimationTrigger += AnimationByTrigger;
	}

    public override void OnUnregistration(CharacterBase oldOwner)
	{
		base.OnUnregistration(oldOwner);
		if (!oldOwner) return;
		oldOwner.OnSelected -= AnimationBySelect;
		oldOwner.OnLookAt	-= AnimationByLookRotation;
		oldOwner.OnMovement -= AnimationByMovement;
        oldOwner.OnAnimationTrigger -= AnimationByTrigger;
	}
	void AnimationBySelect(bool isSelected, ControllerBase from)
	{
		if (!anim) return;
		anim.SetBool("Selected", isSelected);
	}

    void AnimationByTrigger(AnimationTriggerType wantType)
    {
		if (!anim) return;
		anim.SetTrigger(wantType.ToString());
    }

    public void AnimationByLookRotation(Vector3 lookRotation)
	{
		if		(!render) return;
		if		(lookRotation.x > 0) render.flipX = true;
		else if (lookRotation.x < 0) render.flipX = false;
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
