using UnityEngine;
using System.Collections;

public enum AnimationTriggerType
{
    Reset, JumpAttack,
    KnockBack,
    Growl,
    Out,
    Damaged
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
        newOwner.OnLookAt -= AnimationByLookRotation;
        newOwner.OnLookAt += AnimationByLookRotation;
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
        oldOwner.OnLookAt -= AnimationByLookRotation;
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
        if (!render) return;
        if (lookRotation.x > 0) render.flipX = true;
        else if (lookRotation.x < 0) render.flipX = false;
    }

    public void AnimationByMovement(Vector3 moveDelta)
    {
        if (!anim) return;
        if (isRotationByMovement && moveDelta.sqrMagnitude > 0)
        {
            AnimationByLookRotation(moveDelta);
        }
        anim.SetFloat("MoveSpeed", moveDelta.magnitude / Time.fixedDeltaTime);
    }

    public void AnimationReset()
    {
        Owner.AnimationTriggerNotify(AnimationTriggerType.Reset);
    }

    public IEnumerator PlayMove(Vector3Int destination)
    {
        float totalTime = 0.0f;
        Vector3 fromPosition = Owner.transform.position;
        Vector3 toPosition = TileManager.GetTileWorldPosition(destination);
        Vector3 direction = toPosition - fromPosition;
        while (totalTime < ChessMovementModule.moveTimeTotal)
        {
            Owner.transform.position = Vector3.Lerp(fromPosition, toPosition, totalTime / ChessMovementModule.moveTimeTotal);
            totalTime += Time.deltaTime;
            Owner.MovementNotify(direction);
            yield return null;
        }
        Owner.transform.position = toPosition;
    }

    public IEnumerator PlayKnockBack(Vector3Int destination)
    {
        float totalTime = 0.0f;
        Vector3 fromPosition = Owner.transform.position;
        Vector3 toPosition = TileManager.GetTileWorldPosition(destination);
        Vector3 direction = fromPosition - toPosition;
        Owner.MovementNotify(direction);
        Owner.AnimationTriggerNotify(AnimationTriggerType.KnockBack);
        while (totalTime < ChessMovementModule.moveTimeTotal)
        {
            Owner.transform.position = Vector3.Lerp(fromPosition, toPosition, totalTime / ChessMovementModule.moveTimeTotal);
            totalTime += Time.deltaTime;
            yield return null;
        }
        if (Owner.IsAlive)
        {
            Owner.AnimationTriggerNotify(AnimationTriggerType.Growl);
            yield return new WaitForSeconds(0.6f);
        }
        AnimationReset();
        Owner.transform.position = toPosition;
    }

    public IEnumerator PlayAttack(CharacterBase targetCharacter)
    {
        if(targetCharacter)
        {
            Vector3Int destination = targetCharacter.CurrentTilePosition;
            yield return PlayAttack(destination, targetCharacter);
        }
    }
    public IEnumerator PlayAttack(Vector3Int destination, CharacterBase targetCharacter)
    {
        Vector3 fromPosition = Owner.transform.position;
        Vector3 toPosition = TileManager.GetTileWorldPosition(destination);
        Vector3 direction = toPosition - fromPosition;
        Vector3 endPosition = toPosition - (direction * 0.5f);
        Owner.AnimationTriggerNotify(AnimationTriggerType.JumpAttack);
        Owner.MovementNotify(direction);
        yield return new WaitForSeconds(.25f);
        float totalTime = 0.25f;
        while (totalTime < 0.5f)
        {
            float percent = (totalTime - 0.25f) / 0.25f;
            Owner.transform.position = Vector3.Lerp(fromPosition, toPosition, percent);
            totalTime += Time.deltaTime;
            yield return null;
        }
        targetCharacter.MovementNotify(-direction);
        targetCharacter.AnimationTriggerNotify(AnimationTriggerType.Damaged);
        yield return new WaitForSeconds(.1f);
        while (totalTime < 0.7f)
        {
            float percent = (totalTime - 0.6f) / 0.1f;
            Owner.transform.position = Vector3.Lerp(toPosition, endPosition, percent);
            totalTime += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(.2f);
        AnimationReset();
        targetCharacter.AnimationTriggerNotify(AnimationTriggerType.Reset);
        //Owner.transform.position = fromPosition;
        yield break;
    }

    public IEnumerator PlayReturn()
    {
        Vector3 fromPosition = Owner.transform.position;
        Vector3 toPosition = TileManager.GetTileWorldPosition(Owner.CurrentTilePosition);
        Vector3 direction = toPosition - fromPosition;
        Vector3 originDirection = Owner.LookRotation;
        float totalTime = 0.0f;
        while (totalTime < 0.1f)
        {
            Owner.transform.position = Vector3.Lerp(fromPosition, toPosition, totalTime / 0.1f);
            totalTime += Time.deltaTime;
            Owner.MovementNotify(direction);
            yield return null;
        }
        Owner.MovementNotify(originDirection);
        Owner.transform.position = toPosition;
    }

    public IEnumerator PlayOut()
    {
        Owner.AnimationTriggerNotify(AnimationTriggerType.Out);
        yield return new WaitForSeconds(2f);
    }
}