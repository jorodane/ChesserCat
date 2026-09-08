using UnityEngine;
using UnityEngine.UI;

public class UI_CharacterFollowUI : UI_ObjectFollowUI
{
	protected CharacterBase _targetCharacter;
	public CharacterBase TargetCharacter => _targetCharacter;

	public bool HasValidCharacter() => TargetCharacter && TargetCharacter.IsAlive;

	protected override void OnSetObject(GameObject newObject)
	{
		base.OnSetObject(newObject);
		if(newObject && newObject.TryGetComponent(out CharacterBase asCharacter))
		{
			SetCharacter(asCharacter);
		}
	}

	protected void SetCharacter(CharacterBase asCharacter)
	{
		if (TargetCharacter) UnSetCharacter();
		_targetCharacter = asCharacter;
		if (!TargetCharacter) return;
		TargetCharacter.OnOuted -= OnCharacterOut;
		TargetCharacter.OnOuted += OnCharacterOut;
		OnSetCharacter(asCharacter);
		OnCharacterOut(!TargetCharacter.IsAlive);
	}

	protected virtual void OnSetCharacter(CharacterBase asCharacter){}

	protected override void OnUnsetObject(GameObject oldObject)
	{
		base.OnUnsetObject(oldObject);
		UnSetCharacter();
	}

	protected void UnSetCharacter()
	{
		if (!TargetCharacter) return;
		CharacterBase origin = TargetCharacter;
		_targetCharacter = null;
		if (!origin) return;
		origin.OnOuted -= OnCharacterOut;
		OnCharacterOut(true);
	}
	protected virtual void OnUnSetCharacter(CharacterBase asCharacter){}

	protected void OnCharacterOut(bool isOuted)
	{
		if (!Target) isOuted = true;
		gameObject.SetActive(!isOuted);
	}
}