using UnityEngine;
using UnityEngine.EventSystems;

public delegate void DragStartEvent(UI_DraggableWindow dragTarget, Vector2 startPosition);

public class UI_DraggableWindow : UIBase, IPointerDownHandler
{
	public event DragStartEvent OnDragStart;

	//드래그하면 어떤 트랜스폼을 움직여야 할까?
	[SerializeField] RectTransform rootTransform;

	/// <summary> 마지막으로 수신받은 마우스의 위치 </summary>
	Vector2 currentScreenPosition;

	public void OnPointerDown(PointerEventData eventData)
	{
		OnDragStart?.Invoke(this, eventData.position);
	}

	public void SetMouseStartPosition(Vector2 screenPosition)
	{
		currentScreenPosition = screenPosition;
	}

	public void SetMouseCurrentPosition(Vector2 screenPosition)
	{
		//마우스의 위치가 바뀌었단 말이죠
		//얼마나 움직였는지 마우스의 값을 받아오기
		//움직인 거리 = 목적지 - 출발지
		//               5   -   3    =  2
		//움직인 거리가 원래 배율이 1이었으면 1만큼 움직이면 됐음!
		//배율이 1.3배가 되었다면 1만큼 움직이고 싶어도 1.3만큼 더 가게 되어버려요!
		//부모의 사이즈를 제거해줘야 하니까 1.3을 1로 만들려면 1.3으로 나눠주면 됩니다!
		Vector2 screenDelta = screenPosition - currentScreenPosition;
		//inverseAABB => 대상이 누구인가?
		//이게 갇혀 있는 상자
		Rect rootRect = rootTransform.rect;
		//지금은 안 나갔겠죠? 바뀐 뒤에 비교해봐야 한다
		rootRect.position += screenDelta;
		//바꾸고 나서 얼만큼 튀어나갔는가를 확인해보기!
		//튀어나온 걸 보정해주는 값을 InversedAABB가 돌려주니까
		//보정해주는 만큼 위치 이동을 자제한다!
		Vector2 overScreen = rootRect.InversedAABB(UIManager.UIBoundary);
		screenDelta += overScreen;

		Vector3 positionDelta = (Vector3)screenDelta;

		if(UIManager.UIScale > 0.0f) positionDelta /= UIManager.UIScale;

		rootTransform.localPosition += positionDelta;
		currentScreenPosition = screenPosition;
	}
}
