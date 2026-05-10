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
		//1.마우스를 따라 가는 경우!
		//InputManager.OnMouseMove -= MoveToMouse;
		//InputManager.OnMouseMove += MoveToMouse;

		//2.대상을 따라가는 경우
		GameManager.OnUpdateUI -= MoveToTarget;
		GameManager.OnUpdateUI += MoveToTarget;
	}

	private void MoveToTarget(float deltaTime)
	{
		if (target == null) return;
		transform.position = Camera.main.WorldToScreenPoint(target.transform.position) + (Vector3)shiftedPosition;
	}

	//해제
	public override void Unregistration(UIManager manager)
	{
		base.Unregistration(manager);
		InputManager.OnMouseHover -= HoverInfoChange;
		//InputManager.OnMouseMove -= MoveToMouse;
		GameManager.OnUpdateUI -= MoveToTarget;
	}

	void HoverInfoChange(GameObject newTarget, GameObject oldTarget)
	{
		CharacterBase asCharacter = newTarget?.GetComponent<CharacterBase>();
		if (asCharacter)
		{
			hpBar.Connect(asCharacter);
			nameTag.Connect(asCharacter);
			Open();
		}
		else
		{
			hpBar.Disconnect();
			nameTag.Disconnect();
			Close();
		}
		target = asCharacter;
	}

	void MoveToMouse(Vector2 screenPosition, Vector3 worldPosition)
	{
		transform.position = screenPosition + shiftedPosition;
	}
}
