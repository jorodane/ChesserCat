using UnityEngine;
using UnityEngine.EventSystems;

public delegate void DragStartEvent(UI_DraggableWindow dragTarget, Vector2 startPosition);

public class UI_DraggableWindow : UIBase, IPointerDownHandler, IPointerUpHandler
{
	public event DragStartEvent OnDragStart;

	//드래그하면 어떤 트랜스폼을 움직여야 할까?
	[SerializeField] RectTransform rootTransform;

	public void OnPointerDown(PointerEventData eventData)
	{
		OnDragStart?.Invoke(this, eventData.position);
		//나의 "대립 중인 형제들" 중에서 맨 마지막 순번
		//         => 맨 마지막이 이기잖아요?
		//        Sibling
		rootTransform.SetAsLastSibling();
	}

	public void OnPointerUp(PointerEventData eventData)
	{

	}

	public void SetMouseStartPosition(Vector2 screenPosition)
	{
		Debug.Log($"{gameObject} : {screenPosition}");
	}

	public void SetMouseCurrentPosition(Vector2 screenPosition)
	{
		Debug.Log($"{gameObject} : {screenPosition}");
	}
}
