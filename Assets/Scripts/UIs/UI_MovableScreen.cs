using System;
using System.Collections.Generic;
using UnityEngine;

public class UI_MovableScreen : UIBase
{
	[SerializeField] List<UIBase> popUpList = new();
	Vector3 popupPosition = Vector3.zero;
	Vector3 popupShift = new(20.0f, -20.0f);

	UI_DraggableWindow currentDragTarget = null;

	public override void Registration(UIManager manager)
	{
		base.Registration(manager);
		InputManager.OnMouseMove -= MouseMove;
		InputManager.OnMouseMove += MouseMove;
		UIManager.OnPopUp -= PopUp;
		UIManager.OnPopUp += PopUp;
	}

	public override void Unregistration(UIManager manager)
	{
		base.Unregistration(manager);
		InputManager.OnMouseMove -= MouseMove;
		UIManager.OnPopUp -= PopUp;
	}

	protected override GameObject OnSetChild(GameObject newChild)
	{
		//새로운 자식한테 UIManager한테 가서 등록 받아오라고 시킬 것!
		UIManager.ClaimSetUI(newChild);

		//너 이자식 드래그 하려고? 나한테 말해!
		//내가 교통정리를 해줄게!
		if(newChild)
		{
			UI_DraggableWindow asDraggable = newChild.GetComponentInChildren<UI_DraggableWindow>();
			if (asDraggable)
			{
				//ㅇㅋ 너 움직일 수 있다는 거 알겠어!
				//이 친구가 움직임을 원할 때 내 SetDragTarget함수를 실행시킬 수 있게
				asDraggable.OnDragStart -= SetDragTarget;
				asDraggable.OnDragStart += SetDragTarget;
			}
		}

		return base.OnSetChild(newChild);
	}

	protected override void OnUnsetChild(GameObject oldChild)
	{
		UIManager.ClaimUnsetUI(oldChild);

		if (oldChild)
		{
			UI_DraggableWindow asDraggable = oldChild.GetComponentInChildren<UI_DraggableWindow>();
			if (asDraggable)
			{
				asDraggable.OnDragStart -= SetDragTarget;
			}
		}

		base.OnUnsetChild(oldChild);
	}

	void SetDragTarget(UI_DraggableWindow dragTarget, Vector2 startPosition)
	{
		currentDragTarget = dragTarget;
		if (currentDragTarget)
		{
			currentDragTarget.SetMouseStartPosition(startPosition);
		}
	}

	void MouseMove(Vector2 screenPosition, Vector3 worldPosition)
	{
		if(currentDragTarget) //지금 움직여야 하는 친구한테
		{
			//움직이라고 이야기하기!
			currentDragTarget.SetMouseCurrentPosition(screenPosition);
		}
	}

	void PopUp(string title, string context, string confirm)
	{
		//팝업 오브젝트를 만들기!
		GameObject newChild = SetChild(ObjectManager.CreateObject("PopUp"));
		if(newChild)
		{
			//만들어졌어? 일단 자리 잡아!
			newChild.transform.localPosition = GetNextPopUpPosition();

			if (newChild.TryGetComponent(out UIBase newUI))
			{
				//너 팝업창에 합류해라!
				//대신 원래 네가 여기 없었다면! => 하나의 팝업인데 두 번 리스트에 들어가면...?
				//일단 띠껍긴 함
				if(!popUpList.Contains(newUI)) popUpList.Add(newUI);
			}

			//이 친구가 시스템 메시지를 받을 수 있는 걸까?
			//ISystemMessagePossible인지 체크를하고
			if(newChild.TryGetComponent(out ISystemMessagePossible target))
			{
				//메시지를 보내주기만 하면 끝!
				target.SetSystemMessage(title, context, confirm);
			}

			if(newChild.TryGetComponent(out IConfirmable confirmTarget))
			{
				confirmTarget.SetConfirmAction(() => //팝업창을 누르면
				{
					if(newUI) popUpList.Remove(newUI); //너는 팝업도 아니고
					UnsetChild(newChild); //자식에서 제외하고
					ObjectManager.DestroyObject(newChild); //파괴한다
				});
			}
			
		}
	}

	public Vector3 GetNextPopUpPosition()
	{
		//그러면 팝업 포지션은 어떻게 계산할까?
		//지금 가지고 있는 팝업 리스트 중에서 가장 오른쪽 아래에 있는 녀석을 구하기!
		//아무도 없으면? Vector3.zero
		//1등 구하기 문제
		//x축으로 1등인 친구는 +방향으로 달렸을 것이고
		//y축으로 1등인 친구는 -방향으로 달렸을 겁니다!
		Vector3 bestScore = Vector3.zero;

		//아무 팝업도 없으니까 (0,0)으로 빼주기!
		if (popUpList.Count == 0) return bestScore;

		//반에 있는 모든 애들을 전부 돌지 않으면 무슨 일이 생기는가?
		//학부모에게 신고당하지 않게 하기 위해 모두를 돌아줍시다!
		foreach (UIBase currentPopup in popUpList)
		{
			//지금 보고 있는 학생의 위치!
			Vector3 currentScore = currentPopup.transform.localPosition;
			//1.  x축 일등인지
			if (bestScore.x < currentScore.x) bestScore.x = currentScore.x;
			//2. y축 일등인지
			if (bestScore.y > currentScore.y) bestScore.y = currentScore.y;
		}

		return bestScore + popupShift;
	}

}
