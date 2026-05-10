using System.Collections.Generic;
using UnityEngine;
public class ControllerBase : MonoBehaviour, IFunctionable
{
	List<CharacterBase> _characters = new();
	public List<CharacterBase> Characters => _characters;

	ISelectable selectedTarget;
	public ISelectable SelectTarget => selectedTarget;

	public CharacterBase SelectedCharacter => selectedTarget as CharacterBase;

	public virtual void RegistrationFunctions()
	{
		//나랑 같은 오브젝트에 들어있는 캐릭터에 빙의하고 싶다!
		Possess(GetComponent<CharacterBase>());
	}
	public virtual void UnregistrationFunctions()
	{
		//Unpossess(null);
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
			_characters.Add(target);
			OnPossess(target);
		}
	}

	protected virtual void OnUnpossess(CharacterBase oldCharacter) { }
	public void Unpossess(CharacterBase target)
	{
		_characters.Remove(target);
		if (target.Controller == this)
		{
			target.Unpossessed();
			OnUnpossess(target);
		}
	}

	protected virtual void OnSelect(ISelectable newTarget) { }
	public void Select(ISelectable target)
	{
		if (selectedTarget == target) return;
		else if(selectedTarget is not null) Unselect(selectedTarget); 
		if (target is null) return;
		target.Select(this);
		selectedTarget = target;
		OnSelect(target);
	}

	protected virtual void OnUnselect(ISelectable oldTarget) { }
	public void Unselect(ISelectable oldTarget)
	{
		if (selectedTarget is null) return;
		selectedTarget.Unselect(this);
		selectedTarget = null;
		OnUnselect(oldTarget);
	}

	public void CommandMoveToDirection(Vector3 direction)
	{
		if (SelectedCharacter && SelectedCharacter.GetModule<MovementModule>() is IRunnable target) target.MoveToDirection(direction);
	}

	public void CommandMoveToDestination(Vector3 destination, float tolerance)
	{
		if (SelectedCharacter && SelectedCharacter.GetModule<MovementModule>() is IRunnable target) target.MoveToDestination(destination, tolerance);
	}

	public void CommandStop()
	{
		if (SelectedCharacter && SelectedCharacter.GetModule<MovementModule>() is IRunnable target) target.StopMovement();
	}
}
