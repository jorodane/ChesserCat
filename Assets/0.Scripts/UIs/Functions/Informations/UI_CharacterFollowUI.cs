using UnityEngine;
using UnityEngine.UI;

public class UI_CharacterFollowUI : OpenableUIBase
{
	[SerializeField] protected Vector2 shiftedPosition;

	protected CharacterBase _target;
	public CharacterBase Target => _target;

	public override void Registration(UIManager manager)
	{
		base.Registration(manager);
		GameManager.OnUpdateUI -= MoveToTarget;
		GameManager.OnUpdateUI += MoveToTarget;
	}

	void SetAsLastSibiling()
	{
		transform.SetAsLastSibling();
	}

	public override void Unregistration(UIManager manager)
	{
		base.Unregistration(manager);
		UnSetCharacter();
		GameManager.OnUpdateUI -= MoveToTarget;
	}

	public bool HasValidCharacter() => _target && _target.IsAlive;

	public virtual void OpenWithCharacter(CharacterBase asCharacter)
	{
		if (!asCharacter) { Close(false); return; }
		SetCharacter(asCharacter);
		LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
	}

	public override void Close(bool isActiveByKey)
	{
		if (!IsOpen) return;
		base.Close(isActiveByKey);
	}

	public void SetCharacter(CharacterBase asCharacter)
	{
		if (Target) UnSetCharacter();
		_target = asCharacter;
		if (!Target) return;
		_target.OnOuted -= OnCharacterOut;
		_target.OnOuted += OnCharacterOut;
		OnSetCharacter(asCharacter);
		OnCharacterOut(!_target.IsAlive);
	}

	public virtual void OnSetCharacter(CharacterBase asCharacter){}

	public void UnSetCharacter()
	{
		CharacterBase origin = _target;
		_target = null;

		if (!origin) return;
		origin.OnOuted -= OnCharacterOut;
		OnCharacterOut(true);
	}
	public virtual void OnUnSetCharacter(CharacterBase asCharacter){}

	protected void OnCharacterOut(bool isOuted)
	{
		if (!Target) isOuted = true;
		gameObject.SetActive(!isOuted);
	}

	protected void MoveToTarget(float deltaTime)
	{
		if (!_target) return;
		transform.position = Camera.main.WorldToScreenPoint(_target.transform.position) + (Vector3)shiftedPosition;
	}

	protected void MoveToMouse(Vector2 screenPosition, Vector3 worldPosition)
	{
		transform.position = screenPosition + shiftedPosition;
	}
}