using System;
using UnityEngine;

public class UI_CharacterHoverInfo : OpenableUIBase
{
	[SerializeField] Vector2 shiftedPosition;

	[SerializeField] UI_HPBar hpBar;
	[SerializeField] UI_TargetNameTag nameTag;
	
	CharacterBase target;

	//초기화
	public override void Registration(UIManager manager)
	{
		base.Registration(manager);
		InputManager.OnMouseHover -= HoverInfoChange;
		InputManager.OnMouseHover += HoverInfoChange;

		GameManager.OnUpdateUI -= MoveToTarget;
		GameManager.OnUpdateUI += MoveToTarget;
	}

	//해제
	public override void Unregistration(UIManager manager)
	{
		base.Unregistration(manager);
		InputManager.OnMouseHover -= HoverInfoChange;
		GameManager.OnUpdateUI -= MoveToTarget;
	}

	public bool SameAsClickInfo(CharacterBase targetCharacter)
	{
		return UIManager.ClaimCheckOpen(UIType.CharacterClickInfo, out IOpenable ClickInfo) && ClickInfo is ICharacterConnectable asCharacterConnector && asCharacterConnector.ConnectedCharacter == targetCharacter;
	}

	void HoverInfoChange(GameObject newTarget, GameObject oldTarget)
	{
		CharacterBase asCharacter = newTarget?.GetComponent<CharacterBase>();
		if (asCharacter && !SameAsClickInfo(asCharacter))
		{
			OpenWithCharacter(asCharacter);
		}
		else
		{
			Close(false);
		}
		target = asCharacter;
	}

	public void OpenWithCharacter(CharacterBase asCharacter)
	{
		hpBar.Connect(asCharacter);
		nameTag.Connect(asCharacter);
		Open(false);
	}

	public override void Close(bool isActiveByKey)
	{
		if (!IsOpen) return;
		base.Close(isActiveByKey);
		hpBar.Disconnect(target);
		nameTag.Disconnect(target);
	}

    void MoveToTarget(float deltaTime)
    {
        if (target == null) return;
        transform.position = Camera.main.WorldToScreenPoint(target.transform.position) + (Vector3)shiftedPosition;
    }

    void MoveToMouse(Vector2 screenPosition, Vector3 worldPosition)
	{
		transform.position = screenPosition + shiftedPosition;
	}
}